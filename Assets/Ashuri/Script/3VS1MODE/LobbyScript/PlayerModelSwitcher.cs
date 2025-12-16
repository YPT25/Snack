using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerModelSwitcher : NetworkBehaviour
{
    [Header("一人側のオブジェクト")]
    [Tooltip("変更先のオブジェクトをアタッチ")]
    [SerializeField] private GameObject _firstGameObject;

    // プレイヤーのモードID (1=A, 2=B)
    [SyncVar]
    private int _modeId = 0;

    public static object Instance { get; internal set; }

    // モードIDを取得する
    public int GetModeId()
    {
        return _modeId;
    }

    // サーバー側でモードIDを設定する
    [Server]
    public void SetModeId(int id)
    {
        _modeId = id;
    }

    // ----------------------------------------------------
    // モデル変更ボタンが押された時の処理
    // ----------------------------------------------------
    public void ModelButton()
    {
        // サーバーでのみ処理する
        if (!isServer) return;

        // StatePlayer に ModeIDを保存させる
        // 1つ上：3VS1 で一人側になった時
        StatePlayer_Ashuri.Instance.SetModeId(connectionToClient, _modeId);

        // ModeID が 1 の時だけモデル切り替え
        if (_modeId == 1)
        {
            ModelSwitch();
        }
    }

    // ----------------------------------------------------
    // モデル切り替え処理（サーバー専用）
    // ----------------------------------------------------
    [Server]
    public void ModelSwitch()
    {
        // 現在のプレイヤーオブジェクトを保持
        GameObject oldPlayer = gameObject;

        // 接続情報を取得
        NetworkConnectionToClient conn = connectionToClient;

        // 新しいプレイヤーを生成
        GameObject newPlayer = Instantiate(
            _firstGameObject,
            oldPlayer.transform.position,
            oldPlayer.transform.rotation
        );

        // 接続に対してプレイヤーを差し替える
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, false);

        // 1フレーム後に古いプレイヤーを削除
        StartCoroutine(DeleteOldPlayer(oldPlayer));
    }

    // ----------------------------------------------------
    // 1フレーム後に古いプレイヤーを削除
    // ----------------------------------------------------
    private IEnumerator DeleteOldPlayer(GameObject oldPlayer)
    {
        yield return null;

        if (oldPlayer != null)
        {
            NetworkServer.Destroy(oldPlayer);
        }
    }
}
