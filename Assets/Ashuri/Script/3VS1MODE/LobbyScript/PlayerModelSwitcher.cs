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

    // モデルの変更の指示
    public void ModelSwitch()
    {
        GameObject oldPlayer = this.gameObject;
        NetworkConnectionToClient conn = connectionToClient;

        GameObject newPlayer = Instantiate(
            _firstGameObject,
            oldPlayer.transform.position,
            oldPlayer.transform.rotation
        );

        // 置き換え
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, false);

        // 古いプレイヤー削除
        StartCoroutine(DeleteOldPlayer(oldPlayer));
    }

    // ----------------------------------------------------
    // 1フレーム後に古いプレイヤー削除
    // ----------------------------------------------------
    private IEnumerator DeleteOldPlayer(GameObject oldPlayer)
    {
        yield return null;
        if (oldPlayer != null)
            NetworkServer.Destroy(oldPlayer);
    }
}
