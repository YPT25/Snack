using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static Mirror.Examples.CharacterSelection.NetworkManagerCharacterSelection;

public class CustomNetworkManager_Tanabe : NetworkManager
{
    //====================
    // ホスト・クライアント・サーバーの開始・停止
    //====================
    // ホストの開始時に呼ばれる
    public override void OnStartHost()
    {
        base.OnStartHost();
        print("OnStartHost");
    }

    // クライアントの開始時に呼ばれる
    public override void OnStartClient()
    {
        base.OnStartClient();
        print("OnStartClient");
    }

    // サーバーの開始時に呼ばれる
    public override void OnStartServer()
    {
        base.OnStartServer();
        print("OnStartServer");
    }

    // ホストの停止時に呼ばれる
    public override void OnStopHost()
    {
        base.OnStopHost();
        print("OnStopHost");
    }

    // クライアントの停止時に呼ばれる
    public override void OnStopClient()
    {
        base.OnStopClient();
        print("OnStopClient");
    }

    // サーバーの停止時に呼ばれる
    public override void OnStopServer()
    {
        base.OnStopServer();
        print("OnStopServer");
    }

    //====================
    // クライアント
    //====================
    // クライアントの接続時に呼ばれる
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        print("OnClientConnect");
    }

    // クライアントの切断時に呼ばれる
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        print("OnClientDisconnect");
    }

    // クライアントのエラー時に呼ばれる
    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        print("OnClientError : " + error + " , " + reason);
    }

    // クライアントの未準備時に呼ばれる
    public override void OnClientNotReady()
    {
        base.OnClientNotReady();
        print("OnClientDisconnect");
    }

    // クライアントのシーン読み込み完了時に呼ばれる
    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandlin)
    {
        base.OnClientChangeScene(sceneName, sceneOperation, customHandlin);
        print("OnClientChangeScene : " + sceneName);
    }

    //====================
    // サーバー
    //====================
    // サーバーの接続時に呼ばれる
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        print("OnServerConnect : " + conn.connectionId);
    }

    // サーバーの切断時に呼ばれる
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        print("OnServerDisconnect : " + conn.connectionId);
    }

    // サーバーの準備完了時に呼ばれる
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        print("OnServerReady : " + conn.connectionId);
    }

    // サーバーのエラー時に呼ばれる
    public override void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason)
    {
        base.OnServerError(conn, error, reason);
        print("OnServerError : " + conn.connectionId + "," + error + ", " + reason);
    }

    // サーバーのプレイヤー追加時に呼ばれる
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        print("OnServerAddPlayer : " + conn.connectionId);
    }

    // サーバーのシーン読み込み完了時に呼ばれる
    public override void OnServerChangeScene(string sceneName)
    {
        base.OnServerChangeScene(sceneName);
        print("OnServerChangeScene : " + sceneName);
    }


}
