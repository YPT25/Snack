using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// カスタム NetworkManager
/// ・Loadingを動画対応に変更
/// ・プレイヤーPrefabをランダムで追加
/// ・途中参加禁止シーン対応
/// </summary>
public class AshuriNetworkManager : NetworkManager
{
    // ------------------------------
    // プレイヤー関連Prefab
    // ------------------------------
    [Header("プレイヤー関連")]
    [Tooltip("プレイヤー1のPrefab")]
    public GameObject playerPrefab1;

    [Tooltip("プレイヤー2のPrefab")]
    public GameObject playerPrefab2;

    // ------------------------------
    // 途中参加禁止シーン設定
    // ------------------------------
    [Header("途中参加禁止シーン設定")]
    [Tooltip("ゲーム進行中に途中参加を禁止するシーン名のリスト")]
    public List<string> blockedSceneNames = new List<string>();

    // ------------------------------
    // Loading UI（動画対応）
    // ------------------------------
    [Header("Loading UI（動画対応）")]
    [Tooltip("Inspectorで設定するVideoPlayer")]
    public VideoPlayer loadingVideoPlayer;

    // ------------------------------
    // ゲーム進行状態
    // ------------------------------
    public bool gameInProgress = false;

    // ------------------------------
    // プレイヤー番号管理（必要なら使用）
    // ------------------------------
    private int nextPlayerNumber = 1;

    // ====================================================
    // 起動時の初期化
    // ====================================================
    public override void Awake()
    {
        Debug.Log("【Loadingログ】NetworkManager Awake");
        base.Awake();

        // 途中参加禁止シーンリストが空なら警告
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

            // 動画終了時に呼ばれるイベントを登録（重複防止）
            loadingVideoPlayer.loopPointReached -= OnLoadingVideoEnd;
            loadingVideoPlayer.loopPointReached += OnLoadingVideoEnd;

            loadingVideoPlayer.Play();
            Debug.Log("【Loadingログ】Loading動画再生開始");
        }
    }

    // ====================================================
    // 動画終了時の処理（ホスト・クライアント共通）
    // ====================================================
    private void OnLoadingVideoEnd(VideoPlayer vp)
    {
        Debug.Log("【Loadingログ】Loading動画再生終了");

        // シーン遷移後にCanvas非表示にするためイベント登録
        SceneManager.sceneLoaded -= OnSceneLoadedAfterVideo;
        SceneManager.sceneLoaded += OnSceneLoadedAfterVideo;

        // ホスト・クライアントどちらでも同じシーンに遷移
        SceneManager.LoadScene("LobbyScene"); // 遷移先のシーン名を指定
    }

    // ====================================================
    // シーン遷移完了時の処理
    // ====================================================
    private void OnSceneLoadedAfterVideo(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("【Loadingログ】シーン遷移完了 → Canvas非表示");

        // Canvasを非表示にする
        if (loadingVideoPlayer != null)
        {
            loadingVideoPlayer.gameObject.SetActive(false);
        }

        // 既にホストなら不要、必要ならここで処理
        NetworkManager.singleton.StartHost();

        // イベント登録解除
        SceneManager.sceneLoaded -= OnSceneLoadedAfterVideo;
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
    // ホスト開始時の処理
    // ====================================================
    public override void OnStartHost()
    {
        ShowLoading();
        Debug.Log("【Loadingログ】ホスト開始 → Loading Start");

        base.OnStartHost();
    }

    // ====================================================
    // クライアント接続開始時の処理
    // ====================================================
    public override void OnStartClient()
    {
        ShowLoading();
        Debug.Log("【Loadingログ】クライアント接続開始 → Loading Start");
        base.OnStartClient();
    }

    // ====================================================
    // クライアント接続成功時の処理
    // ====================================================
    public override void OnClientConnect()
    {
        Debug.Log("【Loadingログ】クライアント接続成功 → Loading End");
        HideLoading();
        base.OnClientConnect();
    }

    // ====================================================
    // クライアント切断時の処理
    // ====================================================
    public override void OnClientDisconnect()
    {
        Debug.Log("【Loadingログ】クライアント切断 → Loading End");
        HideLoading();
        base.OnClientDisconnect();
    }

    // ====================================================
    // 毎フレーム処理：現在シーンが途中参加禁止か確認
    // ====================================================
    public override void Update()
    {
        base.Update();

        // 現在のシーン名を取得
        string sceneName = SceneManager.GetActiveScene().name;

        // 途中参加禁止シーンならフラグをON
        gameInProgress = blockedSceneNames.Contains(sceneName);
    }

    // ====================================================
    // サーバー接続時のチェック（途中参加禁止判定）
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
    // プレイヤー追加（ランダムでPrefab選択）
    // ====================================================
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // ランダムでどちらのプレイヤーPrefabを使用するか決定
        int randomIndex = Random.Range(0, 2);
        GameObject selectedPrefab = (randomIndex == 0 ? playerPrefab1 : playerPrefab2);

        // スポーン位置があればその位置に生成、なければデフォルト位置に生成
        Transform startPos = GetStartPosition();
        GameObject player = (startPos != null)
            ? Instantiate(selectedPrefab, startPos.position, startPos.rotation)
            : Instantiate(selectedPrefab);

        // Mirrorにプレイヤーを登録
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    // ====================================================
    // サーバー停止時の処理
    // ====================================================
    public override void OnStopServer()
    {
        base.OnStopServer();

        // プレイヤー番号をリセット
        nextPlayerNumber = 1;
    }

    // ====================================================
    // ホスト終了時の処理
    // ====================================================
    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.LogError("ホストが切断されました");
    }

    // ====================================================
    // プレイヤー番号リセット（必要に応じて使用）
    // ====================================================
    public void PlayerNumberReset()
    {
        nextPlayerNumber = 1;
    }
}
