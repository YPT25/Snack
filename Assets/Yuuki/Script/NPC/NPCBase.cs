using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBase : EnemyBase
{
    protected Transform m_target;  // 追跡対象
    protected Rigidbody m_rb;      // 移動用Rigidbody
    protected bool m_isAttacking;  // 攻撃中フラグ（移動制御用）

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
    /// 攻撃開始時に呼ばれる（移動制御を止める）
    /// </summary>
    protected virtual void BeginAttack()
    {
        m_isAttacking = true;
        m_rb.velocity = Vector3.zero; // 攻撃中は移動停止
    }

    /// <summary>
    /// 攻撃終了時に呼ばれる（移動再開を許可）
    /// </summary>
    protected virtual void EndAttack()
    {
        m_isAttacking = false;
    }
}
