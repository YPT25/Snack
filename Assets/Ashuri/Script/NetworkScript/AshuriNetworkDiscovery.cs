using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// LAN内サーバー検索と接続を行うクラス
/// ・ホスト開始やクライアント接続時に動画Loadingを表示
/// ・動画終了後に実際の接続やホスト開始を実行
/// </summary>
public class AshuriNetworkDiscovery : MonoBehaviour
{
    [Header("Network Discovery")]
    [Tooltip("MirrorのNetworkDiscoveryコンポーネント")]
    [SerializeField] private NetworkDiscovery networkDiscovery;

    [Header("UI参照")]
    [Tooltip("ホスト開始ボタン")]
    [SerializeField] private Button hostButton;

    [Tooltip("クライアント検索開始ボタン")]
    [SerializeField] private Button clientButton;

    [Tooltip("クレジットボタン")]
    [SerializeField] private Button creditButton;

    [Tooltip("サーバー一覧コンテンツ")]
    [SerializeField] private Transform serverListContent;

    [Tooltip("サーバー一覧の1要素プレハブ")]
    [SerializeField] private GameObject serverItemPrefab;

    [Tooltip("ネットワーク開始時のUIパネル")]
    [SerializeField] private GameObject networkPanel;

    // 見つかったサーバー一覧を保持する辞書
    private readonly Dictionary<long, ServerResponse> discoveredServers = new();

    // ====================================================
    // 初期化処理：ボタンとNetworkDiscoveryのイベント登録
    // ====================================================
    private void Start()
    {
        // ホスト開始ボタン押下時のイベント登録
        hostButton.onClick.AddListener(OnHostClicked);

        // クライアント検索開始ボタン押下時のイベント登録
        clientButton.onClick.AddListener(OnClientClicked);

        // サーバー検出時のイベント登録
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
    }

    // ====================================================
    // ホスト開始ボタン押下時の処理
    // ====================================================
    private void OnHostClicked()
    {
        // NetworkManagerからカスタムManager取得
        AshuriNetworkManager manager = NetworkManager.singleton.GetComponent<AshuriNetworkManager>();
        if (manager != null)
        {
            // 動画終了イベントの重複登録防止
            manager.loadingVideoPlayer.loopPointReached -= manager.OnLoadingVideoEnd;
            manager.loadingVideoPlayer.loopPointReached += manager.OnLoadingVideoEnd;

            // ホスト開始専用の動画終了イベント登録
            manager.loadingVideoPlayer.loopPointReached -= OnVideoEndStartHost;
            manager.loadingVideoPlayer.loopPointReached += OnVideoEndStartHost;

            // Loading動画を表示
            manager.ShowLoading();
        }
    }

    // ====================================================
    // Loading動画終了後にホストを開始する処理
    // ====================================================
    private void OnVideoEndStartHost(VideoPlayer vp)
    {
        // Mirrorでホストを開始
        NetworkManager.singleton.StartHost();

        // LAN内にサーバーを通知
        networkDiscovery.AdvertiseServer();

        // イベント解除（非表示はManager側で制御）
        vp.loopPointReached -= OnVideoEndStartHost;

        Debug.Log("動画終了 → ホスト開始完了");
    }

    // ====================================================
    // クライアント側：サーバー検索開始ボタン押下時の処理
    // ====================================================
    private void OnClientClicked()
    {
        // 既存のサーバーリストを削除
        foreach (Transform child in serverListContent)
            Destroy(child.gameObject);

        // 検出サーバー一覧をクリア
        discoveredServers.Clear();

        // NetworkDiscoveryでLAN内サーバー探索開始
        networkDiscovery.StartDiscovery();

        Debug.Log("サーバー探索開始（動画Loading）...");
    }

    // ====================================================
    // 指定サーバーに接続する処理
    // ====================================================
    private void ConnectToServer(ServerResponse info)
    {
        // NetworkManagerからカスタムManager取得
        AshuriNetworkManager manager = NetworkManager.singleton.GetComponent<AshuriNetworkManager>();
        if (manager != null)
        {
            // 動画終了イベントの重複登録防止
            manager.loadingVideoPlayer.loopPointReached -= manager.OnLoadingVideoEnd;
            manager.loadingVideoPlayer.loopPointReached += manager.OnLoadingVideoEnd;

            // クライアント接続専用の動画終了イベント登録（ラムダ式で情報を渡す）
            manager.loadingVideoPlayer.loopPointReached -= (vp) => OnVideoEndConnectClient(vp, info);
            manager.loadingVideoPlayer.loopPointReached += (vp) => OnVideoEndConnectClient(vp, info);

            // Loading動画を表示
            manager.ShowLoading();
        }
    }

    // ====================================================
    // Loading動画終了後にクライアント接続を実行する処理
    // ====================================================
    private void OnVideoEndConnectClient(VideoPlayer vp, ServerResponse info)
    {
        // サーバー検索停止
        networkDiscovery.StopDiscovery();

        // Mirrorのクライアント接続処理
        NetworkManager.singleton.StartClient(info.uri);

        // イベント解除（非表示はManager側で制御）
        Debug.Log("動画終了 → クライアント接続完了");
    }

    // ====================================================
    // LAN内でサーバーを検出したときの処理
    // ====================================================
    private void OnServerFound(ServerResponse info)
    {
        // すでに検出済みなら無視
        if (discoveredServers.ContainsKey(info.serverId))
            return;

        // 辞書にサーバー情報を追加
        discoveredServers[info.serverId] = info;

        // UIリストにサーバー要素を生成
        GameObject item = Instantiate(serverItemPrefab, serverListContent);
        item.SetActive(true);

        // UIテキストにサーバーのIPアドレスを表示
        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = info.EndPoint.Address.ToString();

        // ボタン押下で接続するイベント登録
        Button button = item.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => ConnectToServer(info));
    }
}
