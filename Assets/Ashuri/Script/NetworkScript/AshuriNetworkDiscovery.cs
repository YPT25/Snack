using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LAN内サーバー検索と接続を行うクラス
/// ホスト開始・クライアント接続時に動画Loadingを表示します
/// </summary>
public class AshuriNetworkDiscovery : MonoBehaviour
{
    // ------------------------------
    // NetworkDiscovery参照
    // ------------------------------
    [Header("Network Discovery")]
    [Tooltip("MirrorのNetworkDiscoveryコンポーネント")]
    [SerializeField] private NetworkDiscovery networkDiscovery;

    // ------------------------------
    // UI参照
    // ------------------------------
    [Header("UI参照")]
    [Tooltip("ホスト開始ボタン")]
    [SerializeField] private Button hostButton;

    [Tooltip("クライアント検索開始ボタン")]
    [SerializeField] private Button clientButton;

    [Tooltip("サーバー一覧コンテンツ")]
    [SerializeField] private Transform serverListContent;

    [Tooltip("サーバー一覧の1要素プレハブ")]
    [SerializeField] private GameObject serverItemPrefab;

    [Tooltip("ネットワーク開始時のUIパネル")]
    [SerializeField] private GameObject networkPanel;

    // ------------------------------
    // 見つかったサーバー一覧
    // ------------------------------
    private readonly Dictionary<long, ServerResponse> discoveredServers = new();

    // ====================================================
    // 初期化：ボタンイベント登録
    // ====================================================
    private void Start()
    {
        // ホスト開始ボタンに処理を登録
        hostButton.onClick.AddListener(OnHostClicked);

        // クライアント検索ボタンに処理を登録
        clientButton.onClick.AddListener(OnClientClicked);

        // サーバー発見時のイベントに処理を登録
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
    }

    // ====================================================
    // ホスト開始処理
    // ====================================================
    private void OnHostClicked()
    {
        // NetworkManagerを取得してLoading動画を表示
        AshuriNetworkManager manager = NetworkManager.singleton.GetComponent<AshuriNetworkManager>();
        manager?.ShowLoading();

        // Mirrorのホスト開始処理
        //NetworkManager.singleton.StartHost();
        manager.OnStartHost();

        // LAN内にサーバーを通知
        networkDiscovery.AdvertiseServer();

        // ネットワークパネルを非表示
        networkPanel.SetActive(false);

        Debug.Log("ホスト開始（動画Loading）");
    }

    // ====================================================
    // クライアント側：サーバー検索開始
    // ====================================================
    private void OnClientClicked()
    {
        // 既存のサーバー一覧UIを削除
        foreach (Transform child in serverListContent)
        {
            Destroy(child.gameObject);
        }

        // 見つかったサーバー情報を初期化
        discoveredServers.Clear();

        // LANサーバー探索開始
        networkDiscovery.StartDiscovery();

        Debug.Log("サーバー探索開始（動画Loading）...");
    }

    // ====================================================
    // サーバーに接続する処理
    // ====================================================
    private void ConnectToServer(ServerResponse info)
    {
        // サーバー検索停止
        networkDiscovery.StopDiscovery();

        // NetworkManagerにLoading表示を依頼
        AshuriNetworkManager manager = NetworkManager.singleton.GetComponent<AshuriNetworkManager>();
        manager?.ShowLoading();

        // Mirrorのクライアント接続処理
        NetworkManager.singleton.StartClient(info.uri);

        // 接続UIパネルを非表示
        networkPanel.SetActive(false);

        Debug.Log($"クライアント接続中: {info.EndPoint.Address}");
    }

    // ====================================================
    // サーバーを検出した時に呼ばれる処理
    // ====================================================
    private void OnServerFound(ServerResponse info)
    {
        // すでに登録済みなら無視
        if (discoveredServers.ContainsKey(info.serverId))
            return;

        // 新しく発見したサーバーを追加
        discoveredServers[info.serverId] = info;

        // サーバー一覧UIを生成
        GameObject item = Instantiate(serverItemPrefab, serverListContent);
        item.SetActive(true);

        // UIテキストにサーバーのIPを表示
        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = info.EndPoint.Address.ToString();
        }

        // ボタンに接続処理を登録
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ConnectToServer(info));
        }
    }
}
