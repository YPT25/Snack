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
    private bool m_isThrow = false;
    private bool m_isAttract = false;
    private bool m_isDestroy = false;
    private Vector3 m_startDir = Vector3.zero;
    private GameObject m_attractObject = null;
    private int m_lagAdjustment = 5;

    [SerializeField] private GameObject m_head;
    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_foot;
    [SerializeField] private float m_attractPower;
    private Vector3 m_prevHandPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        m_item = GetComponent<ItemStateMachine>();
    }

    [ServerCallback]
    void Update()
    {
        if (m_item.GetItemStateType() != ItemStateMachine.ItemStateType.THROW || m_isDestroy) { return; }
        else if(m_lagAdjustment > 0)
        {
            m_lagAdjustment--;
            return;
        }
        else if(!m_isThrow && m_item.GetItemStateType() == ItemStateMachine.ItemStateType.THROW)
        {
            m_isThrow = true;
            this.RpcThrow();
            m_player = m_item.GetPlayerData();
            Collider[] colliders = m_item.GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
            m_head.GetComponent<Collider>().enabled = true;

            m_body.transform.parent = m_item.GetPlayerData().transform;
            m_foot.transform.parent = m_item.GetPlayerData().transform;
        }

        this.RpcBody();

        if(m_isAttract && !m_isDestroy)
        {
            this.RpcAttract(m_item.GetPlayerData().gameObject);
            if(Vector3.Distance(m_head.transform.position, m_item.GetPlayerData().transform.position) <= 1.5f)
            {
                Destroy(m_item.gameObject);
                m_isDestroy = true;
            }
        }

        m_time -= Time.deltaTime;
        if(m_time <= 0f)
        {
            Destroy(m_item.gameObject);
            m_isDestroy = true;
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (m_item.GetItemStateType() != ItemStateMachine.ItemStateType.THROW || !m_isThrow || m_isDestroy || m_isAttract || other.GetComponent<Player_Tanabe>() != null) { return; }
        
        if (other.GetComponentInParent<ItemStateMachine>() != null && other.GetComponentInParent<ItemStateMachine>().GetItemStateType() == ItemStateType.DROP)
        {
            m_attractObject = other.GetComponentInParent<ItemStateMachine>().gameObject;
            this.RpcSetAttractObject(other.GetComponentInParent<ItemStateMachine>().gameObject);
        }

        m_isAttract = true;
        this.RpcHead(other.ClosestPoint(m_head.transform.position));
        m_head.transform.parent = null;
        Vector3 dir = other.ClosestPoint(m_head.transform.position) - m_head.transform.position;
        m_head.transform.rotation = Quaternion.LookRotation(dir.normalized);


        m_player = m_item.GetPlayerData();

        if (m_player == null || m_attractObject != null) { return; }
        m_player.GetRigidbody().useGravity = false;
        Vector3 velocity = m_player.GetRigidbody().velocity;
        velocity.y = 0f;
        m_player.GetRigidbody().velocity = velocity;

        Vector2 vec2 = new Vector2(m_head.transform.position.x, m_head.transform.position.z) - new Vector2(m_player.transform.position.x, m_player.transform.position.z);
        float z = Mathf.Atan2(vec2.y, vec2.x) * Mathf.Rad2Deg;

        Vector3 eu = Quaternion.LookRotation(dir.normalized).eulerAngles;
        m_head.transform.rotation = Quaternion.Euler(eu.x, eu.y, z - 90f);
    }

    private void OnDestroy()
    {
        Destroy(m_head);
        Destroy(m_body);
        Destroy(m_foot);

        if (m_player == null) { return; }
        m_player.GetRigidbody().useGravity = true;
    }

    [ClientRpc]
    private void RpcAttract(GameObject _player)
    {
        if (_player == null) { return; }

        if (m_attractObject != null)
        {
            Vector3 objectDir = _player.transform.position - m_head.transform.position;
            m_head.transform.position += objectDir.normalized * m_attractPower * Time.deltaTime;
            m_attractObject.transform.position += objectDir.normalized * m_attractPower * Time.deltaTime;
            return;
        }

        Vector3 dir = m_head.transform.position - _player.transform.position;
        _player.transform.position += dir.normalized * m_attractPower * Time.deltaTime;

        if(m_startDir == Vector3.zero)
        {
            m_startDir = dir.normalized;
        }

        //if (Vector3.Distance(m_head.transform.position, m_item.GetPlayerData().transform.position) <= 1.5f && m_startDir.y > 0f)
        //{
        //    m_item.GetPlayerData().GetRigidbody().AddForce(Vector3.up * 1f, ForceMode.Impulse);
        //}

    }

    [ClientRpc]
    private void RpcBody()
    {
        Vector3 headDir = m_head.transform.position - m_prevHandPos;
        //m_head.transform.rotation = Quaternion.LookRotation(headDir.normalized);

        m_foot.transform.localPosition = /*m_item.GetPlayerTransform().position + */new Vector3(0.6f, 0.0f, 0.8f);
        Vector3 dir = m_head.transform.position - m_foot.transform.position;
        m_body.transform.rotation = Quaternion.LookRotation(dir.normalized);
        m_foot.transform.rotation = Quaternion.LookRotation(dir.normalized);
        m_body.transform.position = m_foot.transform.position + dir.normalized * Mathf.Abs(Vector3.Distance(m_head.transform.position, m_foot.transform.position)) * 0.5f;
        Vector3 scale = m_body.transform.localScale;
        scale.z = Mathf.Abs(Vector3.Distance(m_head.transform.position, m_foot.transform.position)) * 0.9f;
        m_body.transform.localScale = scale;

        m_prevHandPos = m_head.transform.position;
    }

    [ClientRpc]
    private void RpcThrow()
    {
        Collider[] colliders = m_item.GetComponents<Collider>();
        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        m_head.GetComponent<Collider>().enabled = true;

        m_player = m_item.GetPlayerData();

        m_prevHandPos = m_head.transform.position;
        m_body.transform.parent = m_item.GetPlayerData().transform;
        m_foot.transform.parent = m_item.GetPlayerData().transform;
    }

    [ClientRpc]
    private void RpcHead(Vector3 closestPoint)
    {
        m_startDir = Vector3.zero;
        m_head.transform.parent = null;
        Vector3 dir = closestPoint - m_head.transform.position;
        m_head.transform.rotation = Quaternion.LookRotation(dir.normalized);

        m_player = m_item.GetPlayerData();

        if (m_player == null || m_attractObject != null) { return; }
        m_player.GetRigidbody().useGravity = false;
        Vector3 velocity = m_player.GetRigidbody().velocity;
        velocity.y = 0f;
        m_player.GetRigidbody().velocity = velocity;

        Vector2 vec2 = new Vector2(m_head.transform.position.x, m_head.transform.position.z) - new Vector2(m_player.transform.position.x, m_player.transform.position.z);
        float z = Mathf.Atan2(vec2.y, vec2.x) * Mathf.Rad2Deg;

        Vector3 eu = Quaternion.LookRotation(dir.normalized).eulerAngles;
        m_head.transform.rotation = Quaternion.Euler(eu.x, eu.y, z - 90f);
    }

    [ClientRpc]
    private void RpcSetAttractObject(GameObject _gameObject)
    {
        m_attractObject = _gameObject;
    }
}
