using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LAN内サーバー検索・接続
/// Host開始・Client接続時に動画Loadingを表示
/// </summary>
public class AshuriNetworkDiscovery : MonoBehaviour
{
    [Header("Network Discovery")]
    [SerializeField] private NetworkDiscovery networkDiscovery;

    [Header("UI参照")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Transform serverListContent;
    [SerializeField] private GameObject serverItemPrefab;
    [SerializeField] private GameObject networkPanel;

    private readonly Dictionary<long, ServerResponse> discoveredServers = new();

    // ====================================================
    // 初期化
    // ====================================================
    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
    }

    // ====================================================
    // ホスト開始
    // ====================================================
    private void OnHostClicked()
    {
        // NetworkManagerにLoading再生指示
        AshuriNetworkManager manager = NetworkManager.singleton.GetComponent<AshuriNetworkManager>();
        manager?.ShowLoading();

        NetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer();

        Debug.Log("ホスト開始（動画Loading）");
        networkPanel.SetActive(false);
    }

    // ====================================================
    // クライアント検索開始
    // ====================================================
    private void OnClientClicked()
    {
        foreach (Transform child in serverListContent)
            Destroy(child.gameObject);

        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();

        Debug.Log("サーバー探索開始（動画Loading）...");
    }

    // ====================================================
    // サーバー接続
    // ====================================================
    private void ConnectToServer(ServerResponse info)
    {
        networkDiscovery.StopDiscovery();

        // Loading表示
        AshuriNetworkManager manager = NetworkManager.singleton.GetComponent<AshuriNetworkManager>();
        manager?.ShowLoading();

        Debug.Log($"クライアント接続中: {info.EndPoint.Address}");
        NetworkManager.singleton.StartClient(info.uri);
        networkPanel.SetActive(false);
    }

    // ====================================================
    // サーバー発見
    // ====================================================
    private void OnServerFound(ServerResponse info)
    {
        if (discoveredServers.ContainsKey(info.serverId))
            return;

        discoveredServers[info.serverId] = info;

        GameObject item = Instantiate(serverItemPrefab, serverListContent);
        item.SetActive(true);

        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = info.EndPoint.Address.ToString();

        Button button = item.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => ConnectToServer(info));
    }
}
