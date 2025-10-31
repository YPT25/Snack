using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager_Tanabe : MonoBehaviour
{
    private Player_Tanabe m_localPlayer;
    private List<Player_Tanabe> m_players = new List<Player_Tanabe>();
    private TPSCameraController_Tanabe m_cameraController;
    private int m_index = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_cameraController == null || m_localPlayer == null || m_players.Count <= 0) { return; }
        if(m_localPlayer.GetHp() <= 0.0f)
        {
            if (!Input.GetButtonDown("Jump")) { return; }

            if(m_index >= m_players.Count - 1)
            {
                m_index = 0;
            }
            else
            {
                for (int i = m_index + 1; i < m_players.Count; i++)
                {
                    if (m_players[i].GetHp() > 0.0f)
                    {
                        m_index = i;
                        break;
                    }
                }
            }
            m_cameraController.SetTarget(m_players[m_index].transform);
        }
    }

    public void SetLocalPlayer(Player_Tanabe _player)
    {
        m_localPlayer = _player;
    }

    public void SetPlayer(Player_Tanabe _player)
    {
        m_players.Add(_player);
    }

    public void SetCameraController(TPSCameraController_Tanabe _cameraController)
    {
        m_cameraController = _cameraController;
    }
}
