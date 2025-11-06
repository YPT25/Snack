using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DiscoveryUIManager : MonoBehaviour
{
    [Header("Network Discovery 参照")]
    [Tooltip("NetworkDiscoveryオブジェクトを指定")]
    [SerializeField] private NetworkDiscovery networkDiscovery;

    [Header("UI 参照")]
    [Tooltip("ホストを開始するボタン")]
    [SerializeField] private Button hostButton;

    [Tooltip("クライアントとしてサーバーを探すボタン")]
    [SerializeField] private Button clientButton;

    [Tooltip("サーバーリストを表示するScrollViewのContent部分")]
    [SerializeField] private Transform serverListContent;

    [Tooltip("サーバー情報を表示するUIプレハブ（ボタンなど）")]
    [SerializeField] private GameObject serverItemPrefab;

    [Tooltip("ユーザーネームを決めるボタン")]
    [SerializeField] private Button userNameButton;

    [Tooltip("後ろの背景パネル")]
    [SerializeField] private GameObject networkPanel;

    [Header("YouserNameを操作するCanvas")]
    [Tooltip("ユーザネームを決めているCanvas")]
    [SerializeField] private Canvas userNameCanvas;

    // ------------------------------
    // すでに使われているプレイヤー名のリスト
    // ------------------------------
    private List<string> usedNames = new List<string>();

    // 見つかったサーバーを記録する辞書（重複防止用）
    private readonly Dictionary<long, ServerResponse> discoveredServers = new();

    void Start()
    {
        // ------------------------------
        // ボタンのクリックイベント登録
        // ------------------------------
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        userNameButton.onClick.AddListener(OnUserNameClicked);

        // ------------------------------
        // NetworkDiscoveryのイベント登録
        // ------------------------------
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
    }

    // ------------------------------
    // ホスト開始ボタンが押されたとき
    // ------------------------------
    private void OnHostClicked()
    {
        // 名前がセットされているかチェック
        if (!PlayerNameHolder.HasPlayerName())
        {
            Debug.LogWarning("名前が設定されていません。先にプレイヤー名を決めてください。");
            return;
        }

        // プレイヤー名の重複チェック
        string playerName = PlayerNameHolder.GetPlayerName();
        if (usedNames.Contains(playerName))
        {
            Debug.LogWarning("この名前はすでに使用されています。別の名前を入力してください。");
            return;
        }
        // 重複していない場合は登録
        usedNames.Add(playerName);
        // NetworkManagerを使ってホスト起動
        NetworkManager.singleton.StartHost();
        // LAN内へ自分をブロードキャスト
        networkDiscovery.AdvertiseServer();
        // ボタンを非表示にする
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        userNameButton.gameObject.SetActive(false);
        // ScrollView の Content を非表示にする
        serverListContent.gameObject.SetActive(false);
        // UIパネルを非表示にする
        networkPanel.SetActive(false);

        Debug.Log("ホストを開始しました。LAN内でブロードキャストを開始します。");
    }

    // ------------------------------
    // クライアントボタンが押されたとき
    // ------------------------------
    private void OnClientClicked()
    {
        // 名前がセットされているかチェック
        if (!PlayerNameHolder.HasPlayerName())
        {
            Debug.LogWarning("名前が設定されていません。先にプレイヤー名を決めてください。");
            return;
        }
        // 既にリストがある場合はクリア
        foreach (Transform child in serverListContent)
        {
            Destroy(child.gameObject);
        }

        discoveredServers.Clear();

        // LAN内のサーバーを検索
        networkDiscovery.StartDiscovery();

        Debug.Log("サーバーを検索しています...");
    }

    /// <summary>
    ///  ユーザネームボタンが押されたとき
    /// </summary>
    private void OnUserNameClicked()
    {
        //このCanvasを非表示
        this.gameObject.SetActive(false);
        //NameCanvasを表示
        userNameCanvas.gameObject.SetActive(true);
    }

    // ------------------------------
    // サーバーが見つかったときの処理
    // ------------------------------
    private void OnServerFound(ServerResponse info)
    {
        // 同じサーバーが既にリストにある場合はスキップ
        if (discoveredServers.ContainsKey(info.serverId))
            return;

        discoveredServers[info.serverId] = info;

        Debug.Log($"サーバー発見: {info.EndPoint.Address}");

        // ------------------------------
        // ScrollViewにサーバー情報を追加
        // ------------------------------
        GameObject item = Instantiate(serverItemPrefab, serverListContent);
        item.SetActive(true);
        // UI上のTextコンポーネントを取得してIPを表示
        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"{info.EndPoint.Address}";
        }

        // クリックで接続する処理を登録
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                ConnectToServer(info);
            });
        }
    }

    // ------------------------------
    // 選択されたサーバーに接続する
    // ------------------------------
    private void ConnectToServer(ServerResponse info)
    {
        // 名前がセットされていない場合の安全チェック
        if (!PlayerNameHolder.HasPlayerName())
        {
            Debug.LogWarning("名前が設定されていません。先にプレイヤー名を決めてください。");
            return;
        }

        // 名前を取得
        string playerName = PlayerNameHolder.GetPlayerName();

        // 接続先情報を表示
        Debug.Log($"サーバーに接続します。接続先: {info.EndPoint.Address}, 名前: {playerName}");
        // 検索停止
        networkDiscovery.StopDiscovery();
        // クライアントとして接続
        NetworkManager.singleton.StartClient(info.uri);
        // ボタンを非表示にする
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        userNameButton.gameObject.SetActive(false);
        // ScrollView の Content を非表示にする
        serverListContent.gameObject.SetActive(false);
        // UIパネルを非表示にする
        networkPanel.SetActive(false);
        // 検索
        Debug.Log($"サーバー({info.EndPoint.Address}) に接続中...");
    }
}
