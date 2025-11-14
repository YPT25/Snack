using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// SphereBox の NPC（AI制御キャラ）用クラス
/// ・NPCBase を継承
/// ・ターゲットが近づいたら「前方に転がって突撃」攻撃を行う
/// </summary>
public class SphereBox_NPC : NPCBase
{
    [Header("突撃攻撃設定")]
    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private float dashDuration = 1.0f;
    [SerializeField] private float dashSpeedMultiplier = 2f;

    private float baseSpeed;

    public override void Start()
    {
        base.Start();

        if (isServer)
            m_waypoints = FindWayPoints("SphereWayPoint");

        baseSpeed = GetMoveSpeed();

        if (m_attackCollider)
            m_attackCollider.enabled = false;
    }

    // ======================================
    // 突撃（dash）
    // ======================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;
        m_isAttacking = true;

        float dashTime = dashDuration;

        float dashSpeed = baseSpeed * dashSpeedMultiplier;

        // ON
        if (m_attackCollider)
            m_attackCollider.enabled = true;

        float t = 0;
        while (t < dashTime)
        {
            t += Time.deltaTime;
            transform.position += transform.forward * dashSpeed * Time.deltaTime;
            yield return null;
        }

        // OFF
        if (m_attackCollider)
            m_attackCollider.enabled = false;

        m_isAttacking = false;
    }

    // 攻撃判定
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() == CharacterType.ENEMY_TYPE) return;

        Attack(target);
    }
}