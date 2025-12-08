using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testNetworkManager : NetworkBehaviour
{
    [Header("変更後のプレイヤーPrefab")]
    [Tooltip("変身後のプレイヤーPrefabを入れる")]
    public GameObject newPlayerPrefab;

    private void Update()
    {

        // 自分のプレイヤーだけが操作できるようにする
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdChangePlayer();
            Debug.Log("押されたよ");
        }
    }
    [Command]
    public void CmdChangePlayer()
    {
        GameObject oldPlayer = this.gameObject;
        NetworkConnectionToClient conn = connectionToClient;

        GameObject newPlayer = Instantiate(
            newPlayerPrefab,
            oldPlayer.transform.position,
            oldPlayer.transform.rotation
        );

        // プレイヤー置き換え（Authority を移さない）
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, false);

        // 古いプレイヤーを1フレーム後に削除
        StartCoroutine(DestroyOldPlayerNextFrame(oldPlayer));
    }

    private IEnumerator DestroyOldPlayerNextFrame(GameObject oldPlayer)
    {
        yield return null; // 1フレーム待機
        if (oldPlayer != null)
            NetworkServer.Destroy(oldPlayer);
    }

}
