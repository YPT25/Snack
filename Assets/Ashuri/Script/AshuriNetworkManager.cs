using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// カスタム NetworkManager（Loadingを動画対応に変更）
/// </summary>
public class AshuriNetworkManager : NetworkManager
{
    [Header("プレイヤー関連")]
    public GameObject playerPrefab1;
    public GameObject playerPrefab2;

    [Header("途中参加禁止シーン設定")]
    public List<string> blockedSceneNames = new List<string>();

    [Header("Loading UI（動画対応）")]
    [Tooltip("VideoPlayerをInspectorで設定")]
    public VideoPlayer loadingVideoPlayer;

    public bool gameInProgress = false;
    private int nextPlayerNumber = 1;

    // ====================================================
    // 起動時の初期化
    // ====================================================
    public override void Awake()
    {
        Debug.Log("【Loadingログ】NetworkManager Awake");
        base.Awake();

        if (blockedSceneNames.Count == 0)
            Debug.LogWarning("途中参加禁止シーンのリストが空です。Inspectorで設定してください");
    }

    // ====================================================
    // Loading動画再生開始
    // ====================================================
    public void ShowLoading()
    {
        if (loadingVideoPlayer != null)
        {
            loadingVideoPlayer.gameObject.SetActive(true);
            loadingVideoPlayer.Play();
            Debug.Log("【Loadingログ】Loading動画再生開始");
        }
    }

    // ====================================================
    // Loading動画停止
    // ====================================================
    public void HideLoading()
    {
        if (loadingVideoPlayer != null)
        {
            loadingVideoPlayer.Stop();
            loadingVideoPlayer.gameObject.SetActive(false);
            Debug.Log("【Loadingログ】Loading動画停止");
        }
    }

    // ====================================================
    // ホスト開始時
    // ====================================================
    public override void OnStartHost()
    {
        ShowLoading();
        Debug.Log("【Loadingログ】ホスト開始 → Loading Start");
        base.OnStartHost();
    }

    // ====================================================
    // クライアント接続開始時
    // ====================================================
    public override void OnStartClient()
    {
        ShowLoading();
        Debug.Log("【Loadingログ】クライアント接続開始 → Loading Start");
        base.OnStartClient();
    }

    // ====================================================
    // クライアント接続成功時
    // ====================================================
    public override void OnClientConnect()
    {
        Debug.Log("【Loadingログ】クライアント接続成功 → Loading End");
        HideLoading();
        base.OnClientConnect();
    }

    // ====================================================
    // クライアント切断時
    // ====================================================
    public override void OnClientDisconnect()
    {
        Debug.Log("【Loadingログ】クライアント切断 → Loading End");
        HideLoading();
        base.OnClientDisconnect();
    }

    // ====================================================
    // 毎フレーム：途中参加判定
    // ====================================================
    public override void Update()
    {
        base.Update();
        string sceneName = SceneManager.GetActiveScene().name;
        gameInProgress = blockedSceneNames.Contains(sceneName);
    }

    // ====================================================
    // サーバー接続時チェック
    // ====================================================
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (gameInProgress)
        {
            Debug.Log($"シーン '{SceneManager.GetActiveScene().name}' は途中参加禁止。接続拒否");
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);
    }

    // ====================================================
    // プレイヤー追加（ランダム）
    // ====================================================
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        int randomIndex = Random.Range(0, 2);
        GameObject selectedPrefab = (randomIndex == 0 ? playerPrefab1 : playerPrefab2);

        Transform startPos = GetStartPosition();
        GameObject player = (startPos != null)
            ? Instantiate(selectedPrefab, startPos.position, startPos.rotation)
            : Instantiate(selectedPrefab);

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    // ====================================================
    // サーバー停止
    // ====================================================
    public override void OnStopServer()
    {
        base.OnStopServer();
        nextPlayerNumber = 1;
    }

    // ====================================================
    // ホスト終了時
    // ====================================================
    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.LogError("ホストが切断されました");
    }

    // ====================================================
    // プレイヤー番号リセット
    // ====================================================
    public void PlayerNumberReset()
    {
        nextPlayerNumber = 1;
    }
}
