using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NormalBox_NPC : NPCBase
{
    [Header("攻撃判定用コライダー（isTrigger推奨）")]
    [SerializeField] private Collider m_attackCollider;
    [Header("攻撃持続時間（秒）")]
    [SerializeField] private float m_attackDuration = 0.5f;
    [Header("攻撃間隔（秒）")]
    [SerializeField] private float m_attackCooldown = 1.0f;
    [Header("索敵設定")]
    [SerializeField] private float m_detectRange = 10.0f;
    [SerializeField] private float m_attackRange = 2.0f;
    [Header("巡回ポイント設定")]
    [SerializeField] private Transform[] m_waypoints;
    [SerializeField] private float m_patrolSpeed = 2.0f;
    [SerializeField] private float m_chaseSpeed = 3.5f;

    private int m_currentWaypoint = 0;
    private bool m_isOnCooldown = false;

    public override void Start()
    {
        base.Start();
        if (m_attackCollider != null)
            m_attackCollider.enabled = false;
    }

    [ServerCallback]
    public override void Update()
    {
        base.Update();
        if (m_isAttacking) return;

        if (m_target != null)
        {
            float dist = Vector3.Distance(transform.position, m_target.position);

            // 索敵範囲外ならターゲット解除
            if (dist > m_detectRange * 1.5f)
            {
                m_target = null;
                return;
            }

            if (dist <= m_attackRange)
            {
                if (!m_isOnCooldown)
                    StartCoroutine(AttackCoroutine());
            }
            else
            {
                MoveTowards(m_target.position, m_chaseSpeed);
            }
        }
        else
        {
            FindTarget();
            if (m_target == null)
                Patrol();
        }
    }

    [Server]
    private void FindTarget()
    {
        CharacterBase[] characters = FindObjectsOfType<CharacterBase>();
        float nearest = Mathf.Infinity;
        Transform nearestHero = null;

        foreach (var c in characters)
        {
            if (c == this || c.GetCharacterType() == CharacterType.ENEMY_TYPE)
                continue;

            if (c.GetCharacterType() == CharacterType.HERO_TYPE)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < m_detectRange && dist < nearest)
                {
                    nearest = dist;
                    nearestHero = c.transform;
                }
            }
        }

        if (nearestHero != null)
        {
            m_target = nearestHero;
            Debug.Log($"{name} が {m_target.name}（HERO）を発見！");
        }
    }

    [Server]
    private void Patrol()
    {
        if (m_waypoints == null || m_waypoints.Length == 0) return;

        Transform wp = m_waypoints[m_currentWaypoint];
        MoveTowards(wp.position, m_patrolSpeed);

        float dist = Vector3.Distance(transform.position, wp.position);
        if (dist < 1.0f)
            m_currentWaypoint = (m_currentWaypoint + 1) % m_waypoints.Length;
    }

    [Server]
    private void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (dir.sqrMagnitude > 0.01f)
            transform.forward = dir;
    }

    [Server]
    private IEnumerator AttackCoroutine()
    {
        BeginAttack();
        m_isOnCooldown = true;
        Debug.Log($"{name} が攻撃動作を開始！");

        if (m_attackCollider != null)
            m_attackCollider.enabled = true;

        transform.Rotate(Vector3.right * 45f);
        yield return new WaitForSeconds(m_attackDuration);

        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        transform.Rotate(Vector3.left * 45f);
        EndAttack();

        yield return new WaitForSeconds(m_attackCooldown);
        m_isOnCooldown = false;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() == CharacterType.ENEMY_TYPE) return;

        target.Damage(GetPower());
        Debug.Log($"{name} が {other.name} に {GetPower()} ダメージ！ 残HP:{target.GetHp()}");
    }
}