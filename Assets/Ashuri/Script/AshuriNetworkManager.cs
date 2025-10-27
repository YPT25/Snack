using UnityEngine; // Unityの基本的な機能を使用
using UnityEngine.SceneManagement; // シーン管理機能を使用
using Mirror; // Mirrorネットワーク機能を使用
using System.Collections; // コルーチンを使用

// MirrorのNetworkManagerを拡張するカスタムクラス
public class AshuriNetworkManager : NetworkManager
{
    [Header("プレイヤーオブジェクト")]
    [Tooltip("1人チームのオブジェクト")]
    public GameObject playerPrefab1;
    [Tooltip("3人チームのオブジェクト")]
    public GameObject playerPrefab2;

    private int nextPlayerNumber = 1;

    /// <summary>
    /// クライアントがサーバーに接続して「プレイヤーを追加」するときに呼ばれる。
    /// Mirrorが内部的に呼び出すサーバー側のイベント関数。
    /// </summary>
    /// <param name="conn">
    /// サーバーに接続してきたクライアントの接続情報。
    /// Mirrorが管理する「NetworkConnectionToClient」オブジェクトです。
    /// </param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerobj;

        PlayerScript_Ashuri playerScript;

        PlayerControl_Tanabe playerScript_Tanabe;

        // 1人目はplayerPrefab1、それ以降はplayerPrefab2
        if (nextPlayerNumber == 1)
        {
            playerobj = Instantiate(playerPrefab1);

            // InstantiateしたオブジェクトからPlayerScriptを取得
            playerScript_Tanabe = playerobj.GetComponent<PlayerControl_Tanabe>();
            //playerScript_Tanabe.playerNumber = nextPlayerNumber;
        }
        else
        {
            playerobj = Instantiate(playerPrefab2);

            // InstantiateしたオブジェクトからPlayerScriptを取得
            playerScript = playerobj.GetComponent<PlayerScript_Ashuri>();
            playerScript.playerNumber = nextPlayerNumber;

            Debug.Log($"プレイヤー番号{playerScript.playerNumber}が参加しました");
        }

        // Mirrorに登録
        NetworkServer.AddPlayerForConnection(conn, playerobj);

        // 番号を増やす
        nextPlayerNumber++;
    }

    /// <summary>
    /// プレイヤーが退出（切断）したときに呼ばれる。
    /// MirrorのNetworkManagerの既定イベントをオーバーライドしています。
    /// </summary>
    /// <param name="conn">切断したプレイヤーの接続情報</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // ベースクラス（NetworkManager）の標準動作を呼ぶ
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// サーバーを停止したときに呼ばれます。
    /// プレイヤー番号カウンタなどをリセットしておきます。
    /// </summary>
    public override void OnStopServer()
    {
        base.OnStopServer();

        // 番号リセット（新しいセッションのため）
        nextPlayerNumber = 1;
    }

    // このスクリプト自体にはFadeSceneTransitionへの直接参照は不要です
    // シングルトンパターンでアクセスします

    // NetworkManagerのAwakeはMirror内部で使われるため、特別な処理がない限りオーバーライドしない
    // もしAwakeで何かしたい場合はbase.Awake()を呼び出す
    // protected override void Awake()
    // {
    //     base.Awake();
    //     Debug.Log("MyNetworkManager Awake custom logic.");
    // }


    // ゲーム開始ボタンなどが押されたときに呼び出すメソッド
    // ホスト（サーバー兼クライアント）がロビーシーンで呼び出すことを想定
    //public void StartGame()
    //{
    //    // サーバーがアクティブで、現在のシーンが"LobbyScene"の場合のみ処理を進める
    //    if (NetworkServer.active && SceneManager.GetActiveScene().name == "testScene")
    //    {
    //        Debug.Log("MyNetworkManager: Host starting game and changing scene to GameScene.");

    //        // FadeSceneTransitionのシングルトンインスタンスが存在するか確認
    //        if (FadeSceneTransition.Instance != null)
    //        {
    //            Debug.Log("MyNetworkManager: Found FadeSceneTransition instance. Starting fade and scene load.");
    //            // フェードアウトコルーチンを開始し、それが完了するのを待ってからシーン変更を行う
    //            StartCoroutine(ServerChangeSceneWithFade("FirstScene"));
    //        }
    //        else
    //        {
    //            // FadeSceneTransitionが見つからない場合は、フェードなしで直接シーンを変更
    //            Debug.LogWarning("MyNetworkManager: FadeSceneTransition.Instance not found. Changing scene without fade.");
    //            ServerChangeScene("FirstScene"); // Mirrorのサーバー側シーン変更メソッドを呼び出す
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("MyNetworkManager: StartGame called, but not as active server in LobbyScene, or conditions not met.");
    //    }
    //}

    //// フェードアウトを伴ってMirrorのサーバーシーンを変更するコルーチン
    //private IEnumerator ServerChangeSceneWithFade(string newSceneName)
    //{
    //    // FadeSceneTransitionのインスタンスが存在する場合のみフェードアウト処理を行う
    //    if (FadeSceneTransition.Instance != null)
    //    {
    //        // FadeOutコルーチンを実行し、完了するまで待機
    //        yield return StartCoroutine(FadeSceneTransition.Instance.FadeOut());
    //        Debug.Log("MyNetworkManager: Fade out completed for server scene change.");
    //    }

    //    // フェードアウトが完了（またはスキップ）したら、サーバーに対して新しいシーンへの変更を指示
    //    // これにより、接続されているすべてのクライアントにも新しいシーンへのロードが通知される
    //    ServerChangeScene(newSceneName);

    //    // クライアント側でのフェードイン処理は、各クライアントのOnClientChangeSceneメソッドで処理される
    //}

    //// クライアントがサーバーからのシーン変更通知を受け取った際に呼び出されるコールバック
    //public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool isCustomHandling)
    //{
    //    // NetworkManagerのデフォルトの処理を呼び出す（非常に重要）
    //    base.OnClientChangeScene(newSceneName, sceneOperation, isCustomHandling);
    //    Debug.Log($"MyNetworkManager: Client received scene change to: {newSceneName}. Operation: {sceneOperation}");

    //    // クライアント側でフェードイン処理を開始
    //    if (FadeSceneTransition.Instance != null)
    //    {
    //        Debug.Log("MyNetworkManager: Client starting fade in after receiving scene change notification.");
    //        StartCoroutine(FadeSceneTransition.Instance.FadeIn()); // FadeInコルーチンを呼び出す
    //    }
    //    else
    //    {
    //        Debug.LogWarning("MyNetworkManager: FadeSceneTransition.Instance not found on client. No fade in applied.");
    //    }
    //}

    //// クライアントが新しいシーンのロードを完了した際に呼び出されるコールバック
    //// Mirrorのバージョンによってはこのメソッドの引数が異なる場合があるが、
    //// 最新版ではNetworkConnection connを引数に取るのが標準
    //public override void OnClientSceneChanged()
    //{
    //    // NetworkManagerのデフォルトの処理を呼び出す
    //    base.OnClientSceneChanged();

    //    // ここでクライアント側の初期化処理、プレイヤーオブジェクトのスポーン、UIの更新などを行うことができる
    //    // 例: クライアント側のプレイヤーキャラクターがスポーンされていない場合、ここでスポーンを要求するRPCをサーバーに送る
    //}

    //// サーバーが新しいシーンのロードを完了した際に呼び出されるコールバック
    //public override void OnServerSceneChanged(string newSceneName)
    //{
    //    // NetworkManagerのデフォルトの処理を呼び出す
    //    base.OnServerSceneChanged(newSceneName);
    //    Debug.Log($"MyNetworkManager: Server finished loading new scene: {newSceneName}");

    //    // ここでサーバー側の初期化処理、AIの配置、ゲームロジックの開始などを行うことができる
    //    // 例えば、新しいシーンでプレイヤーをスポーンさせるための処理など
    //}
}
