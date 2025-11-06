using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/// <summary>
/// NormalBox のプレイヤー操作用クラス
/// ・MPlayerBase を継承
/// ・左クリックで「前方に倒れる攻撃」を実行
/// ・攻撃中のみ攻撃判定用コライダーを有効化する
/// </summary>
public class NormalBox_Player : MPlayerBase
{
    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private float m_attackDuration = 0.5f;
    private bool m_isAttacking = false;

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_A);
        if (m_attackCollider != null)
            m_attackCollider.enabled = false;
    }

    protected override void OnAttackInput()
    {
        if (!m_isAttacking)
            StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;
        transform.Rotate(Vector3.right * 30f);

        if (m_attackCollider != null)
            m_attackCollider.enabled = true;

        yield return new WaitForSeconds(m_attackDuration);

        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        transform.Rotate(Vector3.left * 30f);
        m_isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking || !isServer) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
            Attack(target);
    }
}
