using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class RespawnUIManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject respawnUI;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform buttonParent;

    private CustomNetworkManager networkManager;

    private void Start()
    {
        networkManager = FindObjectOfType<CustomNetworkManager>();

        if (respawnUI != null)
            respawnUI.SetActive(false);

        // ゲーム開始時に選択UIを表示
        ShowCharacterSelectUI();
    }

    public void ShowCharacterSelectUI()
    {
        if (respawnUI == null || buttonPrefab == null || networkManager == null) return;

        // 既存のボタン削除
        foreach (Transform child in buttonParent)
            Destroy(child.gameObject);

        // プレイヤーリスト分だけボタン生成
        for (int i = 0; i < networkManager.playerPrefabs.Count; i++)
        {
            int index = i;
            var prefab = networkManager.playerPrefabs[index];
            var btn = Instantiate(buttonPrefab, buttonParent);
            var text = btn.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = prefab.name;

            btn.onClick.AddListener(() => OnCharacterSelected(index));
        }

        respawnUI.SetActive(true);
    }

    private void OnCharacterSelected(int index)
    {
        respawnUI.SetActive(false);
        CustomNetworkManager.selectedCharacterIndex = index;

        // Mirrorのクライアント生成フロー
        if (!NetworkClient.isConnected)
        {
            Debug.LogWarning("クライアントがまだ接続されていません！");
            return;
        }

        if (!NetworkClient.ready)
            NetworkClient.Ready();

        if (NetworkClient.localPlayer == null)
            NetworkClient.AddPlayer();
    }
}