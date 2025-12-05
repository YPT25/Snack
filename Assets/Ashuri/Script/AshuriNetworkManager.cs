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
/// ・動画再生完了かつ接続完了で非表示
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

    // ------------------------------
    // 動画再生・接続完了フラグ
    // ------------------------------
    [HideInInspector] public bool videoFinished = false;
    [HideInInspector] public bool connectionFinished = false;

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
            videoFinished = false;
            connectionFinished = false;

            loadingVideoPlayer.gameObject.SetActive(true);

            // 動画終了時に呼ばれるイベントを登録（重複防止）
            loadingVideoPlayer.loopPointReached -= OnLoadingVideoEnd;
            loadingVideoPlayer.loopPointReached += OnLoadingVideoEnd;

            loadingVideoPlayer.Play();
            Debug.Log("【Loadingログ】Loading動画再生開始");
        }
    }

    // ====================================================
    // 動画終了時の処理（フラグ設定）
    // ====================================================
    public void OnLoadingVideoEnd(VideoPlayer vp)
    {
        videoFinished = true;
        CheckHideLoading();
    }

    // ====================================================
    // 動画＆接続完了時にLoading非表示
    // ====================================================
    private void CheckHideLoading()
    {
        if (videoFinished && connectionFinished)
        {
            if (loadingVideoPlayer != null)
            {
                //loadingVideoPlayer.gameObject.SetActive(false);
            }
            Debug.Log("【Loadingログ】動画終了＆接続完了 → Loading非表示");
        }
    }

    // ====================================================
    // ホスト開始時の処理（接続完了とみなす）
    // ====================================================
    public override void OnStartHost()
    {
        base.OnStartHost();
        connectionFinished = true;
        CheckHideLoading();
    }

    // ====================================================
    // クライアント接続成功時の処理（接続完了とみなす）
    // ====================================================
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        connectionFinished = true;
        CheckHideLoading();
    }

    // ====================================================
    // クライアント切断時の処理
    // ====================================================
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
    }

    // ====================================================
    // 毎フレーム処理：現在シーンが途中参加禁止か確認
    // ====================================================
    public override void Update()
    {
        base.Update();

        string sceneName = SceneManager.GetActiveScene().name;
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
    //public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    //{
    //    //int randomIndex = Random.Range(0, 2);
    //    //GameObject selectedPrefab = (randomIndex == 0 ? playerPrefab1 : playerPrefab2);

    //    //Transform startPos = GetStartPosition();
    //    //GameObject player = (startPos != null)
    //    //    ? Instantiate(selectedPrefab, startPos.position, startPos.rotation)
    //    //    : Instantiate(selectedPrefab);

    //    //NetworkServer.AddPlayerForConnection(conn, player);
    //}

    // ====================================================
    // サーバー停止時の処理
    // ====================================================
    public override void OnStopServer()
    {
        base.OnStopServer();
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
