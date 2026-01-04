using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyBase;

public static class RespawnSystem
{
    private static Dictionary<EnemyType, GameObject> prefabTable;

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //private static void Init()
    //{
    //    prefabTable = new Dictionary<EnemyType, GameObject>();

    //    foreach (var prefab in NetworkManager.singleton.spawnPrefabs)
    //    {
    //        if (prefab.GetComponent<MPlayerBase>() == null)
    //            continue;

    //        var enemy = prefab.GetComponent<EnemyBase>();
    //        if (enemy == null) continue;

    //        prefabTable[enemy.GetEnemyType()] = prefab;
    //    }
    //}

    public static void EnsureInitialized()
    {
        if (prefabTable != null && prefabTable.Count > 0)
            return;

        prefabTable = new Dictionary<EnemyType, GameObject>();

        Debug.Log("[RespawnSystem] EnsureInitialized() running");

        foreach (var prefab in NetworkManager.singleton.spawnPrefabs)
        {
            var enemy = prefab.GetComponent<EnemyBase>();
            if (enemy == null) continue;

            prefabTable[enemy.GetEnemyType()] = prefab;
            Debug.Log($"  + Register {enemy.GetEnemyType()}");
        }
    }

    public static void ServerRespawn(
       NetworkConnectionToClient conn,
       EnemyType type)
    {
        Debug.Log($"[RespawnSystem] ServerRespawn type={type} conn={conn}");
        EnsureInitialized();

        if (!prefabTable.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"[RespawnSystem] Prefab NOT FOUND for {type}");
            return;
        }

        Debug.Log($"[RespawnSystem] Instantiate {prefab.name}");

        if (conn.identity != null)
            NetworkServer.Destroy(conn.identity.gameObject);

        var player = Object.Instantiate(prefab);

        Debug.Log("[RespawnSystem] ReplacePlayer BEFORE");

        NetworkServer.ReplacePlayerForConnection(conn, player);

        Debug.Log("[RespawnSystem] ReplacePlayer AFTER");
    }

    public static EnemyType[] GetAllPlayerTypes()
    {
        EnsureInitialized();
        return new List<EnemyType>(prefabTable.Keys).ToArray();
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