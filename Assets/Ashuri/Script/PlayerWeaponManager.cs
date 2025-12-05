using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : NetworkBehaviour
{
    [Header("変更後のプレイヤーPrefab")]
    [Tooltip("変身後のプレイヤーPrefabを入れる")]
    public List<GameObject> newPlayerPrefab = new List<GameObject>();

    // ----------------------------------------------------
    // アイテム側から呼び出されるメソッド
    // ----------------------------------------------------
    public void TryChangePlayer(int playerNumber)
    {
        if (!isLocalPlayer) return;

        Debug.Log("CmdChangePlayer を実行します");

        // コマンド発動
        CmdChangePlayer(playerNumber);
    }

    // ----------------------------------------------------
    // プレイヤーの変更処理（サーバで実行）
    // ----------------------------------------------------
    [Command]
    private void CmdChangePlayer(int playerNumber)
    {
        GameObject oldPlayer = this.gameObject;
        NetworkConnectionToClient conn = connectionToClient;

        GameObject newPlayer = Instantiate(
            newPlayerPrefab[playerNumber],
            oldPlayer.transform.position,
            oldPlayer.transform.rotation
        );

        // 置き換え
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, false);

        //StatePlayer_Ashuri にモデル番号を保存する
        StatePlayer_Ashuri state = FindObjectOfType<StatePlayer_Ashuri>();
        if (state != null)
        {
            state.SavePlayerModel(conn, playerNumber);
            Debug.Log($"[WeaponManager] モデル {playerNumber} を保存しました");
        }
        else
        {
            Debug.LogWarning("[WeaponManager] StatePlayer_Ashuri が見つかりませんでした");
        }

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
