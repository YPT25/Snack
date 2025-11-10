using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AshuriNetworkDiscovery : MonoBehaviour
{
    [Header("Network Discovery 参照")]
    [Tooltip("NetworkDiscoveryオブジェクトを指定")]
    [SerializeField] private NetworkDiscovery networkDiscovery;

    [Header("UI 参照")]
    [Tooltip("ホストを開始するボタン")]
    [SerializeField] private Button hostButton;

    [Tooltip("クライアントとしてサーバーを探すボタン（LAN内検索用）")]
    [SerializeField] private Button clientButton;

    [Tooltip("サーバーリストを表示するScrollViewのContent部分")]
    [SerializeField] private Transform serverListContent;

    [Tooltip("サーバー情報を表示するUIプレハブ（ボタンなど）")]
    [SerializeField] private GameObject serverItemPrefab;

    [Tooltip("後ろの背景パネル")]
    [SerializeField] private GameObject networkPanel;

    // ------------------------------
    // 見つかったサーバーを記録する辞書（重複防止用）
    // ------------------------------
    private readonly Dictionary<long, ServerResponse> discoveredServers = new();

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start()
    {
        // Hostボタンが押されたときの処理登録
        hostButton.onClick.AddListener(OnHostClicked);

        // Clientボタンが押されたときの処理登録
        clientButton.onClick.AddListener(OnClientClicked);

        // NetworkDiscoveryでサーバーを見つけたときの処理登録
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
    }

    /// <summary>
    /// ホスト開始ボタンが押されたとき
    /// </summary>
    private void OnHostClicked()
    {
        // Hostとして接続を開始
        NetworkManager.singleton.StartHost();

        // LAN内に自分の存在をブロードキャスト
        networkDiscovery.AdvertiseServer();

        Debug.Log("ホストを開始しました。LAN内にサーバーを公開中。");

        // UIを非表示
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// クライアントがLAN内サーバーを検索するとき
    /// </summary>
    private void OnClientClicked()
    {
        // 古いServerリストをすべて削除
        foreach (Transform child in serverListContent)
        {
            Destroy(child.gameObject);
        }
        discoveredServers.Clear();

        // LAN内のサーバー検索を開始
        networkDiscovery.StartDiscovery();

        Debug.Log("サーバー検索を開始しました（LAN探索中）...");
    }

    /// <summary>
    /// 選択したサーバーに接続する処理
    /// </summary>
    /// <param name="info">接続するサーバー情報</param>
    private void ConnectToServer(ServerResponse info)
    {
        // サーバー検索を停止
        networkDiscovery.StopDiscovery();

        Debug.Log($"クライアント接続中: {info.EndPoint.Address}");

        // MirrorのNetworkManagerを使って接続
        NetworkManager.singleton.StartClient(info.uri);

        // UIを非表示（ゲーム開始）
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// サーバーを見つけたときの処理
    /// </summary>
    /// <param name="info">発見したサーバー情報</param>
    private void OnServerFound(ServerResponse info)
    {
        // 既に登録済みならスキップ
        if (discoveredServers.ContainsKey(info.serverId))
            return;

        // サーバー情報を記録
        discoveredServers[info.serverId] = info;

        // Server Item プレハブを生成
        GameObject item = Instantiate(serverItemPrefab, serverListContent);
        item.SetActive(true);

        // IPアドレスをTextMeshProに表示
        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = $"{info.EndPoint.Address}";

        // ボタン押下で接続
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ConnectToServer(info));
        }
    }
}
