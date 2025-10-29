using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBase : EnemyBase
{
    protected Transform m_target;
    protected Rigidbody m_rb;

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();

        // 仮：シーン内のPlayerBaseを探す
        MPlayerBase player = FindObjectOfType<MPlayerBase>();
        if (GetHp() <= 0)
        {
            Die();
        }
        if (player != null)
        {
            m_target = player.transform;
        }
    }

    public override void Update()
    {
        base.Update();

        if (m_target != null)
        {
            MoveTowardsTarget();
        }
    }

    /// <summary>
    /// ターゲットに向かって移動する
    /// </summary>
    protected virtual void MoveTowardsTarget()
    {
        Vector3 dir = (m_target.position - transform.position).normalized;
        m_rb.velocity = new Vector3(dir.x * GetMoveSpeed(), m_rb.velocity.y, dir.z * GetMoveSpeed());
    }

    /// <summary>
    /// 攻撃処理（ターゲットが近いとき）
    /// </summary>
    protected virtual void TryAttack()
    {
        if (m_target == null) return;

        float dist = Vector3.Distance(transform.position, m_target.position);
        if (dist < 2.0f) // 仮の攻撃範囲
        {
            Attack(m_target.GetComponent<CharacterBaseY>());
        }
    }

}
