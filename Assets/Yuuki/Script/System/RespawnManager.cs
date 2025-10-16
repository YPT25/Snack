using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    // TODO: 将来的に「現存するNPCの種類」と同じキャラだけ選択可能にする
    //       FindObjectsOfType<NPCBase>() でリスト化して判定する予定

    [Header("リスポーン設定")]
    [SerializeField] private List<GameObject> playerPrefabs; // リスポーン候補（プレイヤーのプレハブ）
    [SerializeField] private Transform respawnPoint;          // リスポーン位置
    [SerializeField] private GameObject respawnUI;            // リスポーンUI（Canvas配下の親）
    [SerializeField] private Button buttonPrefab;             // 選択ボタンのプレハブ（ルートにButton）

    private GameObject currentPlayer; // 現在のプレイヤー
    private bool isWaitingForSelection = false;

    private void Start()
    {
        // 初回起動時はUIを非表示
        if (respawnUI != null)
            respawnUI.SetActive(false);

        // 初期プレイヤー生成（仮に最初のキャラ）
        SpawnPlayer(0);
    }

    /// <summary>
    /// プレイヤー死亡時に呼び出される（MPlayerBaseから）
    /// </summary>
    public void OnPlayerDeath()
    {
        Debug.Log("プレイヤー死亡。キャラ選択画面を表示。");
        ShowRespawnUI();
    }

    /// <summary>
    /// キャラ選択UIを表示
    /// </summary>
    private void ShowRespawnUI()
    {
        if (respawnUI == null || buttonPrefab == null) return;

        respawnUI.SetActive(true);
        isWaitingForSelection = true;

        // 既存ボタンをクリア
        foreach (Transform child in respawnUI.transform)
            Destroy(child.gameObject);

        // 現在ステージに存在するNPCの種類を取得
        var existingTypes = GetExistingNPCTypes();

        // ボタンを生成
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            string playerName = playerPrefabs[i].name;
            if (!existingTypes.Contains(playerName))
            {
                // 存在しない種類はスキップ
                continue;
            }

            var newButton = Instantiate(buttonPrefab, respawnUI.transform);

            // ボタン名設定
            TMP_Text textComp = newButton.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = playerName;

            // Button コンポーネント取得＆Listener登録
            Button btn = newButton.GetComponent<Button>();
            if (btn == null)
                btn = newButton.GetComponentInChildren<Button>();

            if (btn != null)
            {
                int index = i; // クロージャ対策
                btn.onClick.AddListener(() => OnCharacterSelected(index));
            }
        }
    }

    /// <summary>
    /// キャラ選択後の処理（ボタン押下で呼ばれる）
    /// </summary>
    private void OnCharacterSelected(int index)
    {
        if (!isWaitingForSelection) return;

        Debug.Log($"プレイヤー[{playerPrefabs[index].name}]を選択。リスポーンします。");

        respawnUI.SetActive(false);
        isWaitingForSelection = false;

        SpawnPlayer(index);
    }

    /// <summary>
    /// プレイヤーを生成
    /// </summary>
    private void SpawnPlayer(int index)
    {
        // 既存プレイヤーを破棄
        if (currentPlayer != null)
            Destroy(currentPlayer);

        // 新しいプレイヤーを生成
        GameObject prefab = playerPrefabs[index];
        currentPlayer = Instantiate(prefab, respawnPoint.position, Quaternion.identity);

        Debug.Log($"プレイヤー[{prefab.name}]をリスポーンしました。");
    }

    /// <summary>
    /// ステージ内に存在しているNPCの名前を取得
    /// </summary>
    private HashSet<string> GetExistingNPCTypes()
    {
        HashSet<string> existingTypes = new HashSet<string>();
        var npcs = FindObjectsOfType<NPCBase>();

        foreach (var npc in npcs)
        {
            string npcName = npc.name.Replace("(Clone)", "").Trim();
            existingTypes.Add(npcName);
        }

        return existingTypes;
    }
}
