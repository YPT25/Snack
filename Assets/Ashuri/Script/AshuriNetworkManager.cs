using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MirrorのNetworkManagerを拡張したクラス
/// シーン名リストによって「途中参加できる／できない」を制御する
/// </summary>
public class AshuriNetworkManager : NetworkManager
{
    // ------------------------------
    // プレイヤー関連設定
    // ------------------------------
    [Header("プレイヤーオブジェクト")]
    [Tooltip("1人チーム用のプレイヤープレハブ")]
    public GameObject playerPrefab1;

    [Tooltip("3人チーム用のプレイヤープレハブ")]
    public GameObject playerPrefab2;

    // ------------------------------
    // ゲーム状態管理
    // ------------------------------
    [Header("ゲーム状態管理")]
    [Tooltip("trueの時、途中参加を禁止します")]
    public bool gameInProgress = false;

    // ------------------------------
    // 途中参加禁止シーンの設定（インスペクターから入力）
    // ------------------------------
    [Header("途中参加禁止シーン設定（シーン名で入力）")]
    [Tooltip("ここに途中参加を禁止したいシーン名を文字で入力してください")]
    public List<string> blockedSceneNames = new List<string>();

    // 次に割り当てるプレイヤー番号
    private int nextPlayerNumber = 1;

    // ----------------------------------------------------
    // 起動時に初期化処理を行う
    // ----------------------------------------------------
    public override void Awake()
    {
        base.Awake();

        // blockedSceneNamesリストが空なら警告を表示
        if (blockedSceneNames.Count == 0)
        {
            Debug.LogWarning("途中参加禁止シーンのリストが空です。インスペクターで設定してください。");
        }
    }

    // ----------------------------------------------------
    // 毎フレーム、現在のシーン名をチェックして参加可否を切り替える
    // ----------------------------------------------------
    public override void Update()
    {
        base.Update();

        // 現在のシーン名を取得
        string currentScene = SceneManager.GetActiveScene().name;

        // 現在のシーンが「禁止リスト」に含まれている場合、途中参加を禁止
        if (blockedSceneNames.Contains(currentScene))
        {
            gameInProgress = true;
        }
        else
        {
            gameInProgress = false;
        }
    }

    /// <summary>
    /// Host時に自分のプレイヤーをSpawnさせる関数
    /// </summary>
    public void SpawnLocalPlayer()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            // Hostの場合、自分のプレイヤーを追加
            OnServerAddPlayer(NetworkServer.localConnection);
        }
    }

    // ----------------------------------------------------
    // クライアントがサーバーに接続してきたときの処理
    // ----------------------------------------------------
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        // 現在のシーンで途中参加が禁止されている場合、接続を拒否する
        if (gameInProgress)
        {
            Debug.Log($"シーン '{SceneManager.GetActiveScene().name}' は途中参加禁止です。接続を拒否します。");
            conn.Disconnect();
            return;
        }

        // 通常の接続処理を継続
        base.OnServerConnect(conn);
    }

    // ----------------------------------------------
    // サーバーが起動したときに呼ばれる処理
    // ----------------------------------------------
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("サーバーが起動しました");
    }

    // ----------------------------------------------
    // クライアントが接続したときに呼ばれる処理
    // ----------------------------------------------
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("クライアントがサーバーに接続しました");
    }   

    // ----------------------------------------------------
    // プレイヤー追加時の処理
    // ----------------------------------------------------
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerobj;
        Player_Tanabe playerScript_Tanabe;

        // 1人目はplayerPrefab1、それ以降はplayerPrefab2を使用
        if (nextPlayerNumber == 1)
        {
            playerobj = Instantiate(playerPrefab1);
        }
        else
        {
            playerobj = Instantiate(playerPrefab2);
        }

        // プレイヤースクリプトに番号を割り当て
        playerScript_Tanabe = playerobj.GetComponent<Player_Tanabe>();
        playerScript_Tanabe.playerNumber = nextPlayerNumber;

        //string currentPlayerName = PlayerNameHolder.GetPlayerName();
        //Debug.Log($"プレイヤー {playerScript_Tanabe.playerNumber} が参加しました。名前: {currentPlayerName}");


        // Mirrorにプレイヤーを登録
        NetworkServer.AddPlayerForConnection(conn, playerobj);

        // 次のプレイヤー番号を増やす
        nextPlayerNumber++;
    }

    // ----------------------------------------------------
    // クライアントが切断したとき
    // ----------------------------------------------------
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }

    // ----------------------------------------------------
    // サーバーを停止したときに番号をリセット
    // ----------------------------------------------------
    public override void OnStopServer()
    {
        base.OnStopServer();

        // 新しいセッション用に番号をリセット
        nextPlayerNumber = 1;
    }

    // ----------------------------------------------------
    // プレイヤー番号を手動でリセットしたい場合
    // ----------------------------------------------------
    public void PlayerNumberReset()
    {
        nextPlayerNumber = 1;
    }
}
