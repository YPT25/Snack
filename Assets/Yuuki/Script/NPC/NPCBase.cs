using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class NPCBase : EnemyBase
{
    [SyncVar] protected bool m_isAttacking; // 攻撃中フラグ（クライアントにも同期）
    protected Transform m_target;  // 追跡対象
    protected Rigidbody m_rb;      // 移動用Rigidbody

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();
    }

    // サーバーでのみAI更新
    [ServerCallback]
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 攻撃開始時に呼ばれる（移動停止）
    /// </summary>
    [Server]
    protected virtual void BeginAttack()
    {
        m_isAttacking = true;
        if (m_rb != null)
            m_rb.velocity = Vector3.zero;
    }

    /// <summary>
    /// 攻撃終了時に呼ばれる（移動再開許可）
    /// </summary>
    [Server]
    protected virtual void EndAttack()
    {
        m_isAttacking = false;
    }
}
