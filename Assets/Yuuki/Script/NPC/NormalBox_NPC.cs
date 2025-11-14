using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NormalBox_NPC : NPCBase
{
    [Header("çUåÇê›íË")]
    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private float attackDuration = 0.5f;

    public override void Start()
    {
        base.Start();

        if (isServer)
            m_waypoints = FindWayPoints("NormalWayPoint");

        if (m_attackCollider)
            m_attackCollider.enabled = false;
    }

    // ======================================
    // ì|ÇÍÇÈ Å® ãNÇ´ÇÈçUåÇ
    // ======================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;

        m_isAttacking = true;

        float elapsed = 0f;
        float duration = attackDuration;
        float angle = 90f;

        Quaternion start = transform.rotation;
        Quaternion down = start * Quaternion.Euler(angle, 0, 0);

        // ÉRÉâÉCÉ_Å[ON
        if (m_attackCollider)
            m_attackCollider.enabled = true;

        // ì|ÇÍÇÈ
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(start, down, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = down;

        // OFF
        if (m_attackCollider)
            m_attackCollider.enabled = false;

        // ãNÇ´è„Ç™ÇÈ
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(down, start, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = start;

        m_isAttacking = false;
    }

    // çUåÇîªíË
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase t = other.GetComponent<CharacterBase>();
        if (t == null) return;
        if (t.GetCharacterType() == CharacterType.ENEMY_TYPE) return;

        Attack(t);
    }
}