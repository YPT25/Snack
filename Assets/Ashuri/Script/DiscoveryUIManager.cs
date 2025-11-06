using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// サーバー設立/検索および接続時のUI制御を行うクラス
/// HostボタンやClientボタンの処理、Serverリスト表示、名前入力Canvasの表示まで管理
/// </summary>
public class DiscoveryUIManager : MonoBehaviour
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

    [Header("UserName入力Canvas")]
    [Tooltip("ユーザーネームを決めるCanvas")]
    [SerializeField] private GameObject userNameCanvas;

    // ------------------------------
    // 見つかったサーバーを記録する辞書（重複防止用）
    // ------------------------------
    private readonly Dictionary<long, ServerResponse> discoveredServers = new();

    /// <summary>
    /// 初期化処理
    /// ボタンイベント登録とNetworkDiscoveryイベント登録を行う
    /// </summary>
    private void Start()
    {
        // Hostボタンが押されたときの処理登録
        hostButton.onClick.AddListener(OnHostClicked);

        // ClientボタンはLAN内のサーバー検索・リスト表示のみ
        clientButton.onClick.AddListener(OnClientClicked);

        // NetworkDiscoveryでサーバーを見つけたときの処理登録
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
    }

    /// <summary>
    /// Hostボタン押下時の処理
    /// 接続を開始し、プレイヤーはSpawnさせずに名前入力Canvasを表示
    /// </summary>
    private void OnHostClicked()
    {
        // プレイヤー自動SpawnをOFFにして、接続時にSpawnさせないようにする
        NetworkManager.singleton.autoCreatePlayer = false;

        // Hostとして接続を開始
        NetworkManager.singleton.StartHost();

        // LAN内に自分の存在をブロードキャスト
        networkDiscovery.AdvertiseServer();

        Debug.Log("Host接続開始（プレイヤーはまだSpawnしていません）");
        
        // 名前入力Canvasを表示
        ShowUserNameCanvas();
    }

    /// <summary>
    /// Clientボタン押下時の処理
    /// LAN内のサーバーを検索してServerリストを生成する
    /// </summary>
    private void OnClientClicked()
    {
        // 古いServerリストをすべて削除
        foreach (Transform child in serverListContent)
        {
            Destroy(child.gameObject);
        }
        discoveredServers.Clear();

        // プレイヤー自動SpawnをOFF
        NetworkManager.singleton.autoCreatePlayer = false;

        // LAN内のサーバー検索を開始
        networkDiscovery.StartDiscovery();

        Debug.Log("Clientサーバー検索開始（Serverリスト生成用）");
    }

    /// <summary>
    /// Server Itemボタン押下時の処理
    /// 選択したサーバーに接続するが、プレイヤーはSpawnせず、Canvasを表示
    /// </summary>
    /// <param name="info">接続するサーバー情報</param>
    private void ConnectToServer(ServerResponse info)
    {
        // サーバー検索を停止
        networkDiscovery.StopDiscovery();

        // Client接続開始（プレイヤーはSpawnさせない）
        NetworkManager.singleton.autoCreatePlayer = false;
        NetworkManager.singleton.StartClient(info.uri);

        // 名前入力Canvasを表示
        ShowUserNameCanvas();

        Debug.Log($"Client接続中: {info.EndPoint.Address}（プレイヤーはまだSpawnしていません）");
    }

    /// <summary>
    /// UserName入力Canvasを表示する共通処理
    /// Host/ClientボタンやServerリスト、背景パネルを非表示にする
    /// </summary>
    private void ShowUserNameCanvas()
    {
        // Host/Clientボタンを非表示
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);

        // Serverリスト非表示
        serverListContent.gameObject.SetActive(false);

        // 背景パネル非表示
        networkPanel.SetActive(false);

        //自分自身も非表示
        this.gameObject.SetActive(false);

        // Canvasを表示
        userNameCanvas.SetActive(true);
    }

    /// <summary>
    /// NetworkDiscoveryでサーバーを見つけたときの処理
    /// ScrollViewにServer Item Prefabを生成して接続ボタンを設定
    /// </summary>
    /// <param name="info">発見したサーバー情報</param>
    private void OnServerFound(ServerResponse info)
    {
        // 既に登録済みならスキップ
        if (discoveredServers.ContainsKey(info.serverId))
            return;

        // サーバー情報を記録
        discoveredServers[info.serverId] = info;

        // Server Item Prefabを生成してScrollViewに追加
        GameObject item = Instantiate(serverItemPrefab, serverListContent);
        item.SetActive(true);

        // IPアドレスをTextMeshProUGUIに表示
        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = $"{info.EndPoint.Address}";

        // ボタン押下で接続処理＋Canvas表示
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ConnectToServer(info));
        }
    }
}
