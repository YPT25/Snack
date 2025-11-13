using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    [Header("プレイヤープレハブリスト")]
    public List<GameObject> playerPrefabs;

    // クライアントが選んだキャラインデックスを保持
    public static int selectedCharacterIndex = -1;

    [Header("初期スポーン地点")]
    public Transform spawnPoint;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // キャラ未選択なら、デフォルトキャラ生成（安全措置）
        int index = Mathf.Clamp(selectedCharacterIndex, 0, playerPrefabs.Count - 1);

        GameObject prefab = playerPrefabs[index];
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject player = Instantiate(prefab, pos, rot);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    // サーバーが起動した時（確認用ログ）
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[NetworkManager] サーバー起動");
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("[NetworkManager] クライアント接続完了");
    }
}
