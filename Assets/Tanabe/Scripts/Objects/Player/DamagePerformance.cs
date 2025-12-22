using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePerformance : NetworkBehaviour
{
    [Header("このオブジェクトのメッシュレンダラ"), SerializeField] private MeshRenderer[] m_meshRenderers;
    [Header("ヒット時のカウンタ"), SerializeField] private float HITTIME;
    [Header("ダメージ表示色"), SerializeField] private Color m_damageColor;
    private float m_hitTimer = 0f;
    private Color m_defaultColor;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();
        m_hitTimer = 0f;
        m_defaultColor = m_meshRenderers[0].material.color;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        m_hitTimer = 0f;
        m_defaultColor = m_meshRenderers[0].material.color;
    }

    // Update is called once per frame
    public void Update()
    {
        if(!isLocalPlayer) { return; }

        if (m_hitTimer > 0f)
        {
            m_hitTimer -= Time.deltaTime;

            if((int)(m_hitTimer * 10f) % 2 == 0)
            {
                this.CmdChangeColor(false);
            }
            else
            {
                this.CmdChangeColor(true);
            }

            if (m_hitTimer <= 0f)
            {
                this.CmdChangeColor(true);
            }
        }
    }

    public void Damage()
    {
        if(!isLocalPlayer) { return; }
        Debug.Log("ダメージの色変え");
        this.CmdDamage();
    }

    [Command]
    public void CmdDamage()
    {
        m_hitTimer = HITTIME;
        this.RpcDamage();

        this.ChangeColor(false);
    }

    [ClientRpc]
    public void RpcDamage()
    {
        m_hitTimer = HITTIME;

        this.ChangeColor(false);
    }

    public void ChangeColor(Color _color)
    {
        Debug.Log("色変え");

        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].material.color = _color;
        }
    }

    [Command]
    public void CmdChangeColor(Color _color)
    {
        this.RpcChangeColor(_color);
    }

    [ClientRpc]
    public void RpcChangeColor(Color _color)
    {
        this.ChangeColor(_color);
    }


    public void ChangeColor(bool _flag)
    {
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].enabled = _flag;
        }
    }

    [Command]
    public void CmdChangeColor(bool _flag)
    {
        this.RpcChangeColor(_flag);
    }

    [ClientRpc]
    public void RpcChangeColor(bool _flag)
    {
        this.ChangeColor(_flag);
    }
}
