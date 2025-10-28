using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    [Header("リスポーン設定")]
    [SerializeField] private List<GameObject> playerPrefabs; // リスポーン候補
    [SerializeField] private Transform respawnPoint;          // リスポーン位置
    [SerializeField] private GameObject respawnUI;            // リスポーンUI（Canvas配下）
    [SerializeField] private Button buttonPrefab;             // 選択ボタンのプレハブ

    private GameObject currentPlayer;
    private bool isWaitingForSelection = false;

    private void Start()
    {
        if (respawnUI != null) respawnUI.SetActive(false);
        SpawnPlayer(0); // 仮で最初のキャラ
    }

    public void OnPlayerDeath()
    {
        Debug.Log("プレイヤー死亡。リスポーン選択UIを表示。");
        ShowRespawnUI();
    }

    /// <summary>
    /// 現在ステージに存在するNPC(敵)のタイプを取得
    /// </summary>
    private HashSet<NPCBase.EnemyType> GetExistingEnemyTypes()
    {
        HashSet<NPCBase.EnemyType> existingTypes = new();
        var enemies = FindObjectsOfType<NPCBase>();

        foreach (var enemy in enemies)
        {
            if (enemy.GetEnemyType() != NPCBase.EnemyType.TYPE_NULL)
                existingTypes.Add(enemy.GetEnemyType());
        }

        return existingTypes;
    }

    /// <summary>
    /// キャラ選択UIを表示（敵のタイプに応じてフィルタ）
    /// </summary>
    private void ShowRespawnUI()
    {
        if (respawnUI == null || buttonPrefab == null)
        {
            Debug.LogError("Respawn UI または ボタンプレハブ が設定されていません！");
            return;
        }

        // 既存ボタンを削除
        foreach (Transform child in respawnUI.transform)
        {
            Destroy(child.gameObject);
        }

        // ステージ上の敵タイプを取得
        HashSet<NPCBase.EnemyType> allowedTypes = GetExistingEnemyTypes();
        Debug.Log($"[RespawnManager] 現在の敵タイプ: {string.Join(", ", allowedTypes)}");


        // 対応プレイヤーのみボタン生成
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            var playerPrefab = playerPrefabs[i];
            
            if (!playerPrefab.TryGetComponent<MPlayerBase>(out var playerBase))
            {
                Debug.LogWarning($"[{playerPrefab.name}] に PlayerBase がアタッチされていません。");
                continue;
            }

            var pType = playerBase.GetEnemyType();

            // 敵のタイプに存在するプレイヤーのみボタン生成
            if (allowedTypes.Contains(pType))
            {
                var newButton = Instantiate(buttonPrefab, respawnUI.transform);
                var textComp = newButton.GetComponentInChildren<TMP_Text>();
                var imageComp = newButton.GetComponentInChildren<Image>();

                // テキスト設定
                if (textComp != null)
                    textComp.text = playerPrefab.name;

                // 画像設定（ボタンに Image がある場合）
                var icon = playerBase.GetRespawnIcon();
                if (imageComp != null && icon != null)
                    imageComp.sprite = icon;

                int index = i; // クロージャー対策
                newButton.onClick.AddListener(() => OnCharacterSelected(index));
                Debug.Log("リスポーンボタン生成：" + playerPrefab.name);
            }
        }

        respawnUI.SetActive(true);
        isWaitingForSelection = true;
    }

    /// <summary>
    /// プレイヤー選択後の処理
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
    /// プレイヤー生成
    /// </summary>
    private void SpawnPlayer(int index)
    {
        if (currentPlayer != null)
            Destroy(currentPlayer);

        GameObject prefab = playerPrefabs[index];
        currentPlayer = Instantiate(prefab, respawnPoint.position, Quaternion.identity);

        Debug.Log($"プレイヤー[{prefab.name}]をリスポーンしました。");
    }
}
