using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox_Tanabe : CharacterBase
{
    [Header("このオブジェクトのメッシュレンダラ"), SerializeField] private MeshRenderer[] m_meshRenderers;
    [Header("ヒット時のカウンタ"), SerializeField] private float HITTIME;
    [Header("死亡演出のエフェクト"), SerializeField] GameObject m_explosionEffect;
    [Header("このオブジェクトが消えるまでのタイマー"), SerializeField] private float m_deadTimer;
    [Header("攻撃された時にアイテムを少しづつ落とすか"), SerializeField] private bool m_isDropLittleByLittle;
    private GenerateDropItem_Tanabe m_generateDropItem;
    private float m_hitTimer = 0.7f;
    private Color m_defaultColor;
    private bool m_isDead = false;

    // Start is called before the first frame update
    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void Start()
    {
        m_defaultColor = m_meshRenderers[0].material.color;
        m_explosionEffect?.SetActive(false);
        base.Initialize();
        base.SetCharacterType(CharacterType.ITEMBOX_TYPE);
        m_generateDropItem = this.GetComponent<GenerateDropItem_Tanabe>();
    }

    // Update is called once per frame
    [ServerCallback]
    public override void Update()
    {
        if(m_isDead)
        {
            m_deadTimer -= Time.deltaTime;
            if(m_deadTimer <= 0f)
            {
                Destroy(this.gameObject);
            }
        }

        if(GetHp() <= 0f && !m_isDead)
        {
            SetMeshActive(false);
            RpcSetMeshActive(false);
            m_isDead = true;
            m_generateDropItem?.DropItems();
        }

        if (m_hitTimer > 0f)
        {
            m_hitTimer -= Time.deltaTime;
            if (m_hitTimer <= 0f)
            {
                RpcChangeColor(m_defaultColor);
            }
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        this.ChangeColor(new Color(0.8f, 0f, 0f, 0f));
        m_hitTimer = HITTIME;
        RpcHit();
        if(m_isDropLittleByLittle && Random.Range(0, 4) == 0)
        {
            m_generateDropItem?.DropItems(1);
        }
    }


    [ClientRpc]
    private void RpcHit()
    {
        this.ChangeColor(new Color(0.8f, 0f, 0f, 0f));
        m_hitTimer = HITTIME;
    }

    private void ChangeColor(Color _color)
    {
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].material.color = _color;
        }
    }

    [ClientRpc]
    public void RpcChangeColor(Color _color)
    {
        this.ChangeColor(_color);
    }

    private void SetMeshActive(bool _flag)
    {
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].enabled = _flag;
        }
        m_explosionEffect?.SetActive(!_flag);
    }

    [ClientRpc]
    public void RpcSetMeshActive(bool _flag)
    {
        this.SetMeshActive(_flag);
    }

}
