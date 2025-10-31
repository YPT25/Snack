using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSelecter_Tanabe : NetworkBehaviour
{
    private CustomNetworkManager_Tanabe m_networkManager;
    [SerializeField] private GameObject[] m_playerPrefabs;

    private void Start()
    {
        m_networkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager_Tanabe>();
    }

    [Command]
    public void Player1()
    {
        m_networkManager.playerPrefab = m_playerPrefabs[0];
    }

    [Command]
    public void Player2()
    {
        m_networkManager.playerPrefab = m_playerPrefabs[1];
    }
}
