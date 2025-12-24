using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyBase;

public static class RespawnSystem
{
    private static Dictionary<EnemyType, GameObject> prefabTable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        prefabTable = new Dictionary<EnemyType, GameObject>();

        foreach (var prefab in NetworkManager.singleton.spawnPrefabs)
        {
            if (prefab.GetComponent<MPlayerBase>() == null)
                continue;

            var enemy = prefab.GetComponent<EnemyBase>();
            if (enemy == null) continue;

            prefabTable[enemy.GetEnemyType()] = prefab;
        }
    }

    public static void ServerRespawn(
       NetworkConnectionToClient conn,
       EnemyType type)
    {
        if (!GetAliveEnemyTypes().Contains(type))
        {
            Debug.LogWarning($"Invalid respawn type: {type}");
            return;
        }

        if (!prefabTable.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"Respawn prefab not found for {type}");
            return;
        }

        if (conn.identity != null)
            NetworkServer.Destroy(conn.identity.gameObject);

        var player = Object.Instantiate(prefab);
        NetworkServer.ReplacePlayerForConnection(conn, player);
    }

    [Server]
    public static HashSet<EnemyType> GetAliveEnemyTypes()
    {
        var set = new HashSet<EnemyType>();

        foreach (var npc in Object.FindObjectsOfType<NPCBase>())
        {
            if (npc == null) continue;
            if (npc.GetHp() <= 0) continue;

            set.Add(npc.GetEnemyType());
        }

        return set;
    }

}