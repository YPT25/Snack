using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;
using System.Linq;
using static EnemyBase;

/// <summary>
/// リスポーンUIを表示し、選択されたプレイヤーPrefabをサーバーに送信する
/// - 最初の1回目は全キャラ選択可能
/// - 2回目以降は「今生きているNPCと同じ EnemyType のキャラのみ」選択可能
/// - ReplacePlayerForConnection で古い本体は自動削除される
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("リスポーン設定")]
    [SerializeField] private List<GameObject> playerPrefabs;
    [SerializeField] private Transform respawnPoint;

    [Header("UI")]
    [SerializeField] private GameObject respawnUI;
    [SerializeField] private Button buttonPrefab;

    private bool isWaiting = false;
    private bool isFirstRespawn = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        respawnUI.SetActive(false);
    }

    // =========================
    // UI表示（Local専用）
    // =========================
    public void ShowRespawnUI()
    {
        if (respawnUI == null || buttonPrefab == null)
            return;

        foreach (Transform c in respawnUI.transform)
            Destroy(c.gameObject);

        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            int index = i;
            GameObject prefab = playerPrefabs[index];
            if (prefab == null) continue;

            Button btn = Instantiate(buttonPrefab, respawnUI.transform);

            var text = btn.GetComponentInChildren<TMP_Text>();
            if (text) text.text = prefab.name;

            var icon = btn.GetComponentInChildren<Image>();
            var mp = prefab.GetComponent<MPlayerBase>();
            if (icon && mp && mp.GetRespawnIcon())
                icon.sprite = mp.GetRespawnIcon();

            btn.onClick.AddListener(() => OnCharacterSelected(index));
        }

        respawnUI.SetActive(true);
        isWaiting = true;
    }

    private void OnCharacterSelected(int index)
    {
        if (!isWaiting) return;

        isWaiting = false;
        respawnUI.SetActive(false);
        isFirstRespawn = false;

        // ★ LocalPlayer 経由で Command を送る
        NetworkClient.localPlayer
            .GetComponent<MPlayerBase>()
            .CmdRequestRespawn(index);
    }

    // =========================
    // Server専用：実リスポーン
    // =========================
    [Server]
    public void ServerRespawn(int index, NetworkConnectionToClient conn)
    {
        if (index < 0 || index >= playerPrefabs.Count) return;
        if (respawnPoint == null) return;

        if (conn.identity != null)
            NetworkServer.Destroy(conn.identity.gameObject);

        GameObject prefab = playerPrefabs[index];
        GameObject newPlayer =
            Instantiate(prefab, respawnPoint.position, Quaternion.identity);

        NetworkServer.ReplacePlayerForConnection(conn, newPlayer);
    }
}