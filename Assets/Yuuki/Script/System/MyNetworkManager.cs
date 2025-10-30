using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MirrorのNetworkManagerを拡張して、
/// プレイヤー生成をPlayerBaseに統一。
/// </summary>
public class MyNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}