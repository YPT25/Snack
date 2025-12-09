using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class WarpItem_Tanabe : NetworkBehaviour
{
    private ItemStateMachine m_item;
    private Player_Tanabe m_player;
    private float m_time = 1.5f;
    private bool m_isWarp = false;
    // Start is called before the first frame update
    void Start()
    {
        m_item = GetComponent<ItemStateMachine>();
    }

    [ServerCallback]
    private void Update()
    {
        if (m_item.GetItemStateType() != ItemStateMachine.ItemStateType.THROW || m_isWarp) { return; }
        m_time -= Time.deltaTime;
        if(m_time <= 0f)
        {
            m_isWarp = true;
            m_item.GetEffectObject().transform.position = m_item.transform.position;
            m_item.RpcExplode();
            this.RpcWarp(m_item.GetPlayerData().gameObject);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (m_item.GetItemStateType() != ItemStateMachine.ItemStateType.THROW || m_isWarp || other.GetComponent<Player_Tanabe>() != null) { return; }
        m_isWarp = true;
        this.RpcWarp(m_item.GetPlayerData().gameObject);
    }

    public void RpcWarp(GameObject _player)
    {
        if(_player == null) { return; }
        _player.transform.position = this.transform.position;
    }
}
