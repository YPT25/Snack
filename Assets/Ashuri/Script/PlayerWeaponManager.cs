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

    [Command]
    private void CmdChangePlayer(int playerNumber)
    {
        // ---- すでに同じモデルを使っているかチェック ----
        StatePlayer_Ashuri state = StatePlayer_Ashuri.Instance;

        if (state != null)
        {
            int currentModel = state.GetSavedModel(connectionToClient);

            // 1つ上：同じモデルなら途中で終了
            if (currentModel == playerNumber)
            {
                //Debug.LogError($"[WeaponManager] すでにモデル {playerNumber} のため変更を行いません");
                return;
            }
        }

        // この下は「違うモデルに変える場合だけ」実行される

        GameObject oldPlayer = this.gameObject;
        NetworkConnectionToClient conn = connectionToClient;

        GameObject newPlayer = Instantiate(
            newPlayerPrefab[playerNumber],
            oldPlayer.transform.position,
            oldPlayer.transform.rotation
        );

        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, false);

        StartCoroutine(FixRotationNextFrame(newPlayer, oldPlayer.transform.rotation));

        if (state != null)
        {
            state.SavePlayerModel(conn, playerNumber);
        }

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

    // ----------------------------------------------------
    // 1フレーム後に新しいプレイヤーの角度を変更
    // ----------------------------------------------------
    private IEnumerator FixRotationNextFrame(GameObject newPlayer, Quaternion rot)
    {
        yield return null;
        if (newPlayer != null)
            newPlayer.transform.rotation = rot;
    }
}
