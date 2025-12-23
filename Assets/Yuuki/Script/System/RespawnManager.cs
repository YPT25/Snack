using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;
using System.Linq;
using static EnemyBase;
using System;

/// <summary>
/// リスポーンUIを表示し、選択されたプレイヤーPrefabをサーバーに送信する
/// - 最初の1回目は全キャラ選択可能
/// - 2回目以降は「今生きているNPCと同じ EnemyType のキャラのみ」選択可能
/// - ReplacePlayerForConnection で古い本体は自動削除される
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Player Prefabs")]
    [SerializeField] private List<GameObject> playerPrefabs;

    [Header("UI")]
    [SerializeField] private GameObject respawnUI;
    [SerializeField] private Button buttonPrefab;

    private MPlayerBase localPlayer;
    private bool isWaiting;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        respawnUI.SetActive(false);
    }

    /// <summary>
    /// Player から呼ばれる（Client）
    /// </summary>
    public void Show(MPlayerBase player)
    {
        localPlayer = player;
        respawnUI.SetActive(true);
        isWaiting = true;

        foreach (Transform c in respawnUI.transform)
            Destroy(c.gameObject);

        foreach (var prefab in playerPrefabs)
        {
            var enemy = prefab.GetComponent<EnemyBase>();
            if (enemy == null) continue;

            var btn = Instantiate(buttonPrefab, respawnUI.transform);

            // アイコンは Prefab から取る
            var img = btn.GetComponent<Image>();
            if (img != null && player.GetRespawnIcon() != null)
                img.sprite = prefab.GetComponent<MPlayerBase>()?.GetRespawnIcon();

            btn.onClick.AddListener(() =>
            {
                if (!isWaiting) return;
                isWaiting = false;
                respawnUI.SetActive(false);

                // ★ EnemyType を送る
                localPlayer.CmdRequestRespawn(enemy.GetEnemyType());
            });
        }
    }

    private void OnSelect(int index)
    {
        if (!isWaiting || localPlayer == null)
            return;

        isWaiting = false;
        respawnUI.SetActive(false);

        // ★ Player 経由で Command を呼ぶ
        //localPlayer.CmdRequestRespawn(index);
    }
}