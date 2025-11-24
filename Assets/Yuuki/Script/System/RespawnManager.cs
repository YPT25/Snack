using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;
using System.Linq;

/// <summary>
/// リスポーンUIを表示し、選択されたプレイヤーPrefabをサーバーに送信する
/// - 最初の1回目は全キャラ選択可能
/// - 2回目以降は「今生きているNPCと同じ EnemyType のキャラのみ」選択可能
/// - ReplacePlayerForConnection で古い本体は自動削除される
/// </summary>
public class RespawnManager : NetworkBehaviour
{
    // ================================
    // Singleton（クライアント専用）
    // ================================
    public static RespawnManager Instance { get; private set; }

    [Header("リスポーン設定")]
    [Tooltip("選択できるプレイヤーのPrefabリスト")]
    [SerializeField] private List<GameObject> playerPrefabs = new List<GameObject>();

    [Tooltip("リスポーン地点")]
    [SerializeField] private Transform respawnPoint;

    [Header("UI設定")]
    [Tooltip("リスポーン用UIのルートパネル（Canvas配下）")]
    [SerializeField] private GameObject respawnUI;

    [Tooltip("キャラ選択ボタンのプレハブ")]
    [SerializeField] private Button buttonPrefab;

    private bool isWaitingForSelection = false;

    // 「最初の1回だけ全キャラOK」判定
    private bool isFirstRespawn = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (respawnUI != null)
        {
            respawnUI.SetActive(false);
            Debug.Log("[RespawnManager] UI初期化完了 (非アクティブ)");
        }
        else
        {
            Debug.LogError("[RespawnManager] respawnUI が設定されていません！");
        }
    }

    // ======================================================
    // 今シーンに存在する EnemyType を取得（クライアント側実行OK）
    // ======================================================
    private HashSet<EnemyBase.EnemyType> GetAliveEnemyTypes()
    {
        var set = new HashSet<EnemyBase.EnemyType>();

        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (var e in enemies)
        {
            if (e == null) continue;

            try
            {
                set.Add(e.GetEnemyType());
            }
            catch { }
        }

        return set;
    }

    // ======================================================
    // UIを表示（生きているNPCのタイプに応じたフィルタ）
    // ======================================================
    public void ShowRespawnUI()
    {
        Debug.Log("[RespawnManager] ShowRespawnUI() 呼び出し");

        if (respawnUI == null || buttonPrefab == null)
        {
            Debug.LogError("[RespawnManager] UIまたはボタンPrefabなし！");
            return;
        }

        // UIを一旦クリア
        foreach (Transform child in respawnUI.transform)
            Destroy(child.gameObject);

        List<int> allowedIndices = new List<int>();

        // -----------------------------
        // ① 最初の1回は全キャラ表示
        // -----------------------------
        if (isFirstRespawn)
        {
            Debug.Log("[RespawnManager] 最初の選択 → 全てのキャラを表示");

            for (int i = 0; i < playerPrefabs.Count; i++)
                allowedIndices.Add(i);
        }
        else
        {
            // -----------------------------
            // ② 2回目以降 → NPCのEnemyTypeでフィルタ
            // -----------------------------
            HashSet<EnemyBase.EnemyType> aliveTypes = GetAliveEnemyTypes();
            Debug.Log("[RespawnManager] 生存NPCタイプ: " + string.Join(", ", aliveTypes));

            for (int i = 0; i < playerPrefabs.Count; i++)
            {
                var prefab = playerPrefabs[i];
                if (prefab == null) continue;

                var enemyBase = prefab.GetComponent<EnemyBase>();
                if (enemyBase == null)
                {
                    // 想定外だけど一応許可する
                    allowedIndices.Add(i);
                    continue;
                }

                var prefabType = enemyBase.GetEnemyType();

                if (aliveTypes.Contains(prefabType))
                {
                    allowedIndices.Add(i);
                }
            }

            // 該当無し → フォールバックで全表示
            if (allowedIndices.Count == 0)
            {
                Debug.Log("[RespawnManager] 該当タイプが無いため全表示へフォールバック");
                for (int i = 0; i < playerPrefabs.Count; i++)
                    allowedIndices.Add(i);
            }
        }

        // -----------------------------
        // ボタンを生成
        // -----------------------------
        foreach (int index in allowedIndices)
        {
            int captured = index;
            GameObject prefab = playerPrefabs[captured];
            if (prefab == null) continue;

            var btn = Instantiate(buttonPrefab, respawnUI.transform);

            // 名前
            var text = btn.GetComponentInChildren<TMP_Text>();
            if (text) text.text = prefab.name;

            // アイコン
            var img = btn.GetComponentInChildren<Image>();
            var mp = prefab.GetComponent<MPlayerBase>();
            if (img && mp)
            {
                Sprite icon = mp.GetRespawnIcon();
                if (icon) img.sprite = icon;
            }

            btn.onClick.AddListener(() => OnCharacterSelected(captured));
        }

        respawnUI.SetActive(true);
        isWaitingForSelection = true;
        Debug.Log("[RespawnManager] リスポーンUI表示完了");
    }

    // ======================================================
    // キャラ選択
    // ======================================================
    private void OnCharacterSelected(int index)
    {
        if (!isWaitingForSelection)
            return;

        isWaitingForSelection = false;
        respawnUI.SetActive(false);

        Debug.Log($"[RespawnManager] 選択: {playerPrefabs[index].name}");

        // ここで「初回フラグ」を false にする
        isFirstRespawn = false;

        CmdRequestRespawn(index);
    }

    // ======================================================
    // サーバー側 Respawn
    // ======================================================
    [Command(requiresAuthority = false)]
    private void CmdRequestRespawn(int index, NetworkConnectionToClient sender = null)
    {
        if (index < 0 || index >= playerPrefabs.Count)
        {
            Debug.LogWarning("[RespawnManager] indexエラー");
            return;
        }

        if (respawnPoint == null)
        {
            Debug.LogError("[RespawnManager] respawnPoint 未設定");
            return;
        }

        // 旧プレイヤーの削除
        if (sender.identity != null)
        {
            GameObject oldPlayer = sender.identity.gameObject;
            NetworkServer.Destroy(oldPlayer);
        }

        GameObject prefab = playerPrefabs[index];

        GameObject newPlayer = Instantiate(prefab, respawnPoint.position, Quaternion.identity);
        Debug.Log($"[RespawnManager] 新しいプレイヤー生成: {prefab.name}");

        // 重要：古いプレイヤーを自動削除して置き換え
        NetworkServer.ReplacePlayerForConnection(sender, newPlayer);
    }
}