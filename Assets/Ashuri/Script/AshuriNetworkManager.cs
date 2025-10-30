using UnityEngine; // Unityの基本機能
using UnityEngine.SceneManagement; // シーン管理
using Mirror; // Mirrorネットワーク
using System.Collections; // コルーチン使用

/// <summary>
/// MirrorのNetworkManagerを拡張したカスタムクラス
/// プレイヤー管理 + フェード付きシーン遷移機能を追加
/// </summary>
public class AshuriNetworkManager : NetworkManager
{
    [Header("プレイヤーオブジェクト")]
    [Tooltip("1人チームのオブジェクト")]
    public GameObject playerPrefab1;
    [Tooltip("3人チームのオブジェクト")]
    public GameObject playerPrefab2;

    // 次のプレイヤー番号
    private int nextPlayerNumber = 1;

    /// <summary>
    /// クライアントがサーバーに接続して「プレイヤーを追加」するときに呼ばれる
    /// Mirror内部イベント
    /// </summary>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerobj;
        Player_Tanabe playerScript_Tanabe;

        // 1人目はplayerPrefab1、それ以降はplayerPrefab2
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

        Debug.Log($"プレイヤー番号{playerScript_Tanabe.playerNumber}が参加しました");

        // Mirrorにプレイヤーを登録
        NetworkServer.AddPlayerForConnection(conn, playerobj);

        // 次のプレイヤー番号を増やす
        nextPlayerNumber++;
    }

    /// <summary>
    /// プレイヤーが切断したとき
    /// </summary>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// サーバーを停止したときに呼ばれる
    /// </summary>
    public override void OnStopServer()
    {
        base.OnStopServer();
        // 新しいセッション用に番号リセット
        nextPlayerNumber = 1;
    }

   
}
