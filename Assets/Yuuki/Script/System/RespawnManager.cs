using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// リスポーンUIを表示し、選択されたプレイヤーPrefabをサーバーに送信する
/// - シーンに常駐（UIはCanvasごとアタッチ）
/// - UIはクライアント専用、サーバーでは生成しない
/// - サーバーはCommand経由で新しいプレイヤーをSpawnする
/// </summary>
public class RespawnManager : NetworkBehaviour
{
    // ================================
    // Singleton設定（クライアント専用）
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

    // ================================
    // 初期化
    // ================================
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

    // ================================
    // クライアントでUI表示
    // ================================
    public void ShowRespawnUI()
    {
        Debug.Log("[RespawnManager] ShowRespawnUI() 呼び出し");

        if (respawnUI == null || buttonPrefab == null)
        {
            Debug.LogError("[RespawnManager] respawnUI または buttonPrefab が設定されていません！");
            return;
        }

        // 既存ボタンを削除
        foreach (Transform child in respawnUI.transform)
            Destroy(child.gameObject);

        // 各プレイヤーPrefabに対応したボタンを生成
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            int index = i;
            var prefab = playerPrefabs[index];
            var btn = Instantiate(buttonPrefab, respawnUI.transform);

            // ボタンにキャラ名を設定
            var text = btn.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = prefab.name;

            // アイコン設定
            var image = btn.GetComponentInChildren<Image>();
            var mplayer = prefab.GetComponent<MPlayerBase>();
            if (image != null && mplayer != null)
            {
                var icon = mplayer.GetRespawnIcon();
                if (icon != null)
                    image.sprite = icon;
            }

            // ボタン押下でキャラ選択
            btn.onClick.AddListener(() => OnCharacterSelected(index));
        }

        // UI表示
        respawnUI.SetActive(true);
        isWaitingForSelection = true;
        Debug.Log("[RespawnManager] リスポーンUIを表示しました");
    }

    // ================================
    // キャラ選択時
    // ================================
    private void OnCharacterSelected(int index)
    {
        if (!isWaitingForSelection)
            return;

        isWaitingForSelection = false;
        respawnUI.SetActive(false);

        Debug.Log($"[RespawnManager] キャラ {playerPrefabs[index].name} が選択されました。CmdRequestRespawn送信。");

        // サーバーにリスポーン要求送信
        CmdRequestRespawn(index);
    }

    // ================================
    // サーバー側：プレイヤー再生成
    // ================================
    [Command(requiresAuthority = false)]
    private void CmdRequestRespawn(int index, NetworkConnectionToClient sender = null)
    {
        if (index < 0 || index >= playerPrefabs.Count)
        {
            Debug.LogWarning($"[RespawnManager] 無効なインデックス指定: {index}");
            return;
        }

        if (respawnPoint == null)
        {
            Debug.LogError("[RespawnManager] respawnPoint が設定されていません！");
            return;
        }

        // 旧プレイヤーの削除
        if (sender.identity != null)
        {
            GameObject oldPlayer = sender.identity.gameObject;
            NetworkServer.Destroy(oldPlayer);
        }

        GameObject prefab = playerPrefabs[index];

        // 新プレイヤー生成
        GameObject newPlayer = Instantiate(prefab, respawnPoint.position, Quaternion.identity);
        Debug.Log($"[RespawnManager] 新プレイヤー生成: {prefab.name}");

        // 接続先プレイヤーを置き換え
        NetworkServer.ReplacePlayerForConnection(sender, newPlayer);
        Debug.Log($"[RespawnManager] ReplacePlayerForConnection 実行完了 (connId={sender?.connectionId})");
    }
}