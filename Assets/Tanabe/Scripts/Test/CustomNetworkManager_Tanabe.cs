using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static Mirror.Examples.CharacterSelection.NetworkManagerCharacterSelection;

public class CustomNetworkManager_Tanabe : NetworkManager
{
    //====================
    // �z�X�g�E�N���C�A���g�E�T�[�o�[�̊J�n�E��~
    //====================
    // �z�X�g�̊J�n���ɌĂ΂��
    public override void OnStartHost()
    {
        base.OnStartHost();
        print("OnStartHost");
    }

    // �N���C�A���g�̊J�n���ɌĂ΂��
    public override void OnStartClient()
    {
        base.OnStartClient();
        print("OnStartClient");
    }

    // �T�[�o�[�̊J�n���ɌĂ΂��
    public override void OnStartServer()
    {
        base.OnStartServer();
        print("OnStartServer");
    }

    // �z�X�g�̒�~���ɌĂ΂��
    public override void OnStopHost()
    {
        base.OnStopHost();
        print("OnStopHost");
    }

    // �N���C�A���g�̒�~���ɌĂ΂��
    public override void OnStopClient()
    {
        base.OnStopClient();
        print("OnStopClient");
    }

    // �T�[�o�[�̒�~���ɌĂ΂��
    public override void OnStopServer()
    {
        base.OnStopServer();
        print("OnStopServer");
    }

    //====================
    // �N���C�A���g
    //====================
    // �N���C�A���g�̐ڑ����ɌĂ΂��
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        print("OnClientConnect");
    }

    // �N���C�A���g�̐ؒf���ɌĂ΂��
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        print("OnClientDisconnect");
    }

    // �N���C�A���g�̃G���[���ɌĂ΂��
    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        print("OnClientError : " + error + " , " + reason);
    }

    // �N���C�A���g�̖��������ɌĂ΂��
    public override void OnClientNotReady()
    {
        base.OnClientNotReady();
        print("OnClientDisconnect");
    }

    // �N���C�A���g�̃V�[���ǂݍ��݊������ɌĂ΂��
    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandlin)
    {
        base.OnClientChangeScene(sceneName, sceneOperation, customHandlin);
        print("OnClientChangeScene : " + sceneName);
    }

    //====================
    // �T�[�o�[
    //====================
    // �T�[�o�[�̐ڑ����ɌĂ΂��
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        print("OnServerConnect : " + conn.connectionId);
    }

    // �T�[�o�[�̐ؒf���ɌĂ΂��
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        print("OnServerDisconnect : " + conn.connectionId);
    }

    // �T�[�o�[�̏����������ɌĂ΂��
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        print("OnServerReady : " + conn.connectionId);
    }

    // �T�[�o�[�̃G���[���ɌĂ΂��
    public override void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason)
    {
        base.OnServerError(conn, error, reason);
        print("OnServerError : " + conn.connectionId + "," + error + ", " + reason);
    }

    // �T�[�o�[�̃v���C���[�ǉ����ɌĂ΂��
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        print("OnServerAddPlayer : " + conn.connectionId);
    }

    // �T�[�o�[�̃V�[���ǂݍ��݊������ɌĂ΂��
    public override void OnServerChangeScene(string sceneName)
    {
        base.OnServerChangeScene(sceneName);
        print("OnServerChangeScene : " + sceneName);
    }


}
