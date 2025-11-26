using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyWeaponSelector : NetworkBehaviour
{
    [Header("対応するプレイヤーPrefab")]
    [Tooltip("この武器に触れたときに切り替えるプレイヤーPrefab")]
    public GameObject targetPlayerPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        NetworkIdentity oldPlayer = collision.gameObject.GetComponent<NetworkIdentity>();
        if (oldPlayer == null) return;

        if (isServer)
        {
            ReplacePlayer(oldPlayer, targetPlayerPrefab);
        }
    }

    private void ReplacePlayer(NetworkIdentity oldPlayer, GameObject newPrefab)
    {
        if (oldPlayer == null || newPrefab == null) return;

        // 元プレイヤーの接続情報を取得
        NetworkConnectionToClient conn = oldPlayer.connectionToClient;
        if (conn == null)
        {
            Debug.LogError("ReplacePlayer: connectionToClient が null です");
            return;
        }

        // 新しいプレイヤーPrefabを元プレイヤーと同じ位置・回転で生成
        Vector3 pos = oldPlayer.transform.position;
        Quaternion rot = oldPlayer.transform.rotation;
        GameObject newPlayer = Instantiate(newPrefab, pos, rot);

        // 新しいプレイヤーを接続に追加（Authority付き）
        NetworkServer.AddPlayerForConnection(conn, newPlayer);

        // 元プレイヤーを削除
        NetworkServer.Destroy(oldPlayer.gameObject);
    }
}
