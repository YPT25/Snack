using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBase : EnemyBase
{
    protected Transform m_target;  // �ǐՑΏ�
    protected Rigidbody m_rb;      // �ړ��pRigidbody
    protected bool m_isAttacking;  // �U�����t���O�i�ړ�����p�j

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();


    }

    public override void Update()
    {
        base.Update();
    }


    /// <summary>
    /// �U���J�n���ɌĂ΂��i�ړ�������~�߂�j
    /// </summary>
    protected virtual void BeginAttack()
    {
        m_isAttacking = true;
        m_rb.velocity = Vector3.zero; // �U�����͈ړ���~
    }

    /// <summary>
    /// �U���I�����ɌĂ΂��i�ړ��ĊJ�����j
    /// </summary>
    protected virtual void EndAttack()
    {
        m_isAttacking = false;
    }
}
