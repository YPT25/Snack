using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// リスポーン時のキャラ選択UIを表示し、
/// 選ばれたモジュールIDをPlayerBaseに送る。
/// </summary>
public class RespawnManager : NetworkBehaviour
{
    [Header("リスポーン設定")]
    [SerializeField] private List<GameObject> playerPrefabs;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameObject respawnUI;
    [SerializeField] private Button buttonPrefab;

    private bool isWaitingForSelection = false;

    private void Start()
    {
        if (respawnUI != null)
            respawnUI.SetActive(false);
    }

    public void OnPlayerDeath()
    {
        if (!isLocalPlayer) return;
        ShowRespawnUI();
    }

    private void ShowRespawnUI()
    {
        if (respawnUI == null || buttonPrefab == null) return;

        foreach (Transform child in respawnUI.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            int index = i;
            var prefab = playerPrefabs[index];
            var btn = Instantiate(buttonPrefab, respawnUI.transform);
            var text = btn.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = prefab.name;

            btn.onClick.AddListener(() => OnCharacterSelected(index));
        }

        respawnUI.SetActive(true);
        isWaitingForSelection = true;
    }

    private void OnCharacterSelected(int index)
    {
        if (!isWaitingForSelection) return;
        isWaitingForSelection = false;
        respawnUI.SetActive(false);
        CmdRequestRespawn(index);
    }

    [Command]
    private void CmdRequestRespawn(int index, NetworkConnectionToClient sender = null)
    {
        if (index < 0 || index >= playerPrefabs.Count) return;
        GameObject prefab = playerPrefabs[index];
        var playerObj = Instantiate(prefab, respawnPoint.position, Quaternion.identity);
        NetworkServer.Spawn(playerObj, sender);
    }
}