using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemStateMachine;

public class HandItem_Tanabe : NetworkBehaviour
{
    private ItemStateMachine m_item;
    private Player_Tanabe m_player;
    private float m_time = 2.5f;
    private bool m_isWarp = false;
    private bool m_isThrow = false;
    private bool m_isAttract = false;
    private bool m_isDestroy = false;

    [SerializeField] private GameObject m_head;
    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_foot;
    [SerializeField] private float m_attractPower;

    // Start is called before the first frame update
    void Start()
    {
        m_item = GetComponent<ItemStateMachine>();
    }

    [ServerCallback]
    private void Update()
    {
        if (m_item.GetItemStateType() != ItemStateMachine.ItemStateType.THROW || m_isDestroy) { return; }
        else if(!m_isThrow && m_item.GetItemStateType() == ItemStateMachine.ItemStateType.THROW)
        {
            m_isThrow = true;
            this.RpcThrow();
        }

        this.RpcBody();

        if(m_isAttract)
        {
            this.RpcAttract(m_item.GetPlayerData().gameObject);
            if(Vector3.Distance(m_head.transform.position, m_item.GetPlayerData().transform.position) <= 0.5f)
            {
                Destroy(m_item.gameObject);
            }
        }

        m_time -= Time.deltaTime;
        if(m_time <= 0f)
        {
            Destroy(m_item.gameObject);
            m_isDestroy = true;
            //m_isAttract = true;
            //m_item.GetEffectObject().transform.position = m_item.transform.position;
            //m_item.RpcExplode();
            //this.RpcWarp(m_item.GetPlayerData().gameObject);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (m_item.GetItemStateType() != ItemStateMachine.ItemStateType.THROW || m_isDestroy || m_isAttract || other.GetComponent<Player_Tanabe>() != null) { return; }
        m_isWarp = true;
        //this.RpcWarp(m_item.GetPlayerData().gameObject);

        m_isAttract = true;
        this.RpcHead();
    }

    private void OnDestroy()
    {
        Destroy(m_head);
        Destroy(m_body);
        Destroy(m_foot);
    }

    public void RpcWarp(GameObject _player)
    {
        if(_player == null) { return; }
        _player.transform.position = this.transform.position;
    }

    [ClientRpc]
    private void RpcAttract(GameObject _player)
    {
        if (_player == null) { return; }
        Vector3 dir = m_head.transform.position - _player.transform.position;
        _player.transform.position += dir.normalized * m_attractPower * Time.deltaTime;
    }

    [ClientRpc]
    private void RpcBody()
    {
        Vector3 dir = m_head.transform.position - m_foot.transform.position;
        m_body.transform.rotation = Quaternion.LookRotation(dir.normalized);
        m_foot.transform.rotation = Quaternion.LookRotation(dir.normalized);
        Vector3 scale = m_body.transform.localScale;
        scale.y = Vector3.Distance(m_head.transform.position, m_foot.transform.position);
        m_body.transform.localScale = scale;
    }

    [ClientRpc]
    private void RpcThrow()
    {
        SphereCollider[] colliders = m_item.GetComponents<SphereCollider>();
        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        m_body.transform.parent = m_item.GetPlayerData().transform;
        m_foot.transform.parent = m_item.GetPlayerData().transform;
    }

    [ClientRpc]
    private void RpcHead()
    {
        m_head.transform.parent = null;
    }
}
