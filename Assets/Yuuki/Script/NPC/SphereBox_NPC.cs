using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SphereBox の NPC（AI制御キャラ）用クラス
/// ・NPCBase を継承
/// ・ターゲットが近づいたら「前方に転がって突撃」攻撃を行う
/// </summary>
public class SphereBox_NPC : NPCBase
{
    // 攻撃判定用コライダー
    [Header("攻撃判定用コライダー（isTrigger推奨）")]
    [SerializeField] private Collider m_attackCollider;

    [Header("突撃の持続時間（秒）")]
    [SerializeField] private float m_rollDuration = 1.0f;

    [Header("突撃の力")]
    [SerializeField] private float m_rollForce = 15f;

    private bool m_isAttacking = false;

    public override void Start()
    {
        base.Start();

        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }
    }

    public override void Update()
    {
        base.Update();

        // 攻撃を試みる（NPCBaseのTryAttackをオーバーライド）
        TryAttack();
    }

    /// <summary>
    /// NPCの攻撃判定処理
    /// ターゲットが一定距離以内なら突撃開始
    /// </summary>
    protected override void TryAttack()
    {
        if (m_isAttacking) return;
        if (m_target == null) return;

        float dist = Vector3.Distance(transform.position, m_target.position);
        // SphereBoxはやや広めの距離から突撃
        if (dist < 3.0f)
        {
            StartCoroutine(RollAttackCoroutine());
        }
    }

    /// <summary>
    /// 突撃攻撃のコルーチン
    /// ・前方に力を加える
    /// ・攻撃判定ON/OFF
    /// </summary>
    private IEnumerator RollAttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} が転がって突撃！（NPC）");

        if (m_attackCollider != null) m_attackCollider.enabled = true;

        if (m_rb != null)
        {
            m_rb.AddForce(transform.forward * m_rollForce, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(m_rollDuration);

        if (m_attackCollider != null) m_attackCollider.enabled = false;

        m_isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBaseY target = other.GetComponent<CharacterBaseY>();
        if (target != null && target != this)
        {
            Attack(target);
        }
    }
}
