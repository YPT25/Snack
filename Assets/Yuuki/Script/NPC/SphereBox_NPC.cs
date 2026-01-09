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

        // ★突撃中：Agentを止めて位置補正の喧嘩を止める
        bool hadAgent = (agent != null && agent.isOnNavMesh);
        if (hadAgent)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        // 攻撃判定ON
        if (m_attackCollider) m_attackCollider.enabled = true;

        float dashSpeed = baseSpeed * dashSpeedMultiplier;

        float t = 0f;
        while (t < dashDuration)
        {
            // RigidbodyがあるならFixedでMovePosition（衝突に強い）
            if (m_rb != null)
            {
                Vector3 next = m_rb.position + transform.forward * dashSpeed * Time.fixedDeltaTime;
                m_rb.MovePosition(next);
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }
            else
            {
                // 最低限：Rigidbodyが無い場合は従来通り
                t += Time.deltaTime;
                transform.position += transform.forward * dashSpeed * Time.deltaTime;
                yield return null;
            }
        }

        // 攻撃判定OFF
        if (m_attackCollider) m_attackCollider.enabled = false;

        // 突撃後：Agentを復帰＆位置を同期
        if (hadAgent)
        {
            agent.nextPosition = transform.position; // ワープ差分を消す
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
        }

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