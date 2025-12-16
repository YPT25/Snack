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
        {
            m_waypoints = FindWayPoints("SphereWayPoint");
        }

        baseSpeed = GetMoveSpeed();

        if (m_attackCollider)
            m_attackCollider.enabled = false;
    }

    // ======================================
    // 突撃攻撃（Server）
    // ======================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking)
            yield break;

        m_isAttacking = true;
        m_isMoving = true;

        float elapsed = 0f;
        float dashSpeed = baseSpeed * dashSpeedMultiplier;

        // ★ 突撃開始時の基準位置を保持
        Vector3 dashOrigin = transform.position;
        Vector3 dashDir = transform.forward;

        if (m_attackCollider)
            m_attackCollider.enabled = true;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            // ★ 経過時間ベースで目的地を更新
            float moveDist = dashSpeed * elapsed;
            m_syncDestination = dashOrigin + dashDir * moveDist;

            yield return null;
        }

        if (m_attackCollider)
            m_attackCollider.enabled = false;

        m_isMoving = false;
        m_isAttacking = false;
    }

    // ======================================
    // 攻撃ヒット判定（Serverのみ）
    // ======================================
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking)
            return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null)
            return;

        if (target.GetCharacterType() == CharacterType.ENEMY_TYPE)
            return;

        Attack(target);
    }
}