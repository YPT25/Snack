using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
public class NPCBase : EnemyBase
{
    // =========================
    // 同期用（Server → Client）
    // =========================
    [SyncVar] protected bool m_isAttacking = false;
    [SyncVar] protected Vector3 m_syncDestination;
    [SyncVar] protected bool m_isMoving = false;

    // =========================
    // 内部参照
    // =========================
    protected Transform m_target;
    protected Rigidbody m_rb;
    protected NavMeshAgent agent;

    // =========================
    // WayPoint
    // =========================
    protected Transform[] m_waypoints;
    protected int m_currentWaypoint = 0;

    // =========================
    // AI設定
    // =========================
    [Header("AI基本設定")]
    [SerializeField] protected float detectRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float randomWalkInterval = 2f;
    [SerializeField] protected float walkSpeedMultiplier = 0.5f;

    private float randomWalkTimer = 0f;
    private Vector3 randomDir;
    private float fallDeathY = -5f;

    // =========================
    // 初期化
    // =========================
    public override void Start()
    {
        base.Start();

        m_rb = GetComponent<Rigidbody>();
        TryGetComponent(out agent);

        // Server：AI判断のみ（NavMeshは使わない）
        if (isServer && agent != null)
        {
            agent.enabled = false;
        }

        // Client：見た目の移動をNavMeshで再生
        if (!isServer && agent != null)
        {
            agent.speed = GetMoveSpeed() * walkSpeedMultiplier;
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
    }

    // =========================
    // Server：AI判断
    // =========================
    [ServerCallback]
    public override void Update()
    {
        if (!GetIsMove())
            return;

        // 落下死
        if (transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        if (GetHp() <= 0)
        {
            Die();
            return;
        }

        base.Update();

        if (m_isAttacking)
            return;

        // ===== ターゲットあり =====
        if (m_target != null)
        {
            float dist = Vector3.Distance(transform.position, m_target.position);

            // 見失い
            if (dist > detectRange * 1.5f)
            {
                m_target = null;
                m_isMoving = false;
                return;
            }

            // 攻撃範囲
            if (dist <= attackRange)
            {
                Vector3 dir = m_target.position - transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);

                StartCoroutine(DoAttack());
                return;
            }

            // 追跡
            ChaseTarget();
            return;
        }

        // ===== 索敵 =====
        FindHeroTarget();
        if (m_target != null)
            return;

        // ===== 巡回 =====
        if (m_waypoints != null && m_waypoints.Length > 0)
        {
            Patrol();
            return;
        }

        // ===== ランダム歩行 =====
        RandomWalk();
    }

    // =========================
    // Client：移動の再生
    // =========================
    private void LateUpdate()
    {
        if (isServer) return;
        if (agent == null) return;

        if (!m_isMoving || m_isAttacking)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(m_syncDestination);
    }

    // =========================
    // WayPoint 自動取得（派生クラス用）
    // =========================
    [Server]
    protected Transform[] FindWayPoints(string rootName)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null)
        {
            Debug.LogWarning($"[NPCBase] WayPoint Root '{rootName}' が見つかりません");
            return null;
        }

        List<Transform> points = new List<Transform>();
        foreach (Transform child in root.transform)
        {
            points.Add(child);
        }

        return points.ToArray();
    }

    // =========================
    // ランダム歩行（Server）
    // =========================
    [Server]
    private void RandomWalk()
    {
        randomWalkTimer -= Time.deltaTime;
        if (randomWalkTimer <= 0f)
        {
            randomWalkTimer = randomWalkInterval;
            randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
        }

        Vector3 move =
            randomDir * GetMoveSpeed() * walkSpeedMultiplier;

        m_syncDestination = transform.position + move;
        m_isMoving = true;
    }

    // =========================
    // 巡回（Server）
    // =========================
    [Server]
    protected void Patrol()
    {
        if (m_waypoints == null || m_waypoints.Length == 0)
            return;

        Transform wp = m_waypoints[m_currentWaypoint];
        m_syncDestination = wp.position;
        m_isMoving = true;

        if (Vector3.Distance(transform.position, wp.position) <= 1f)
        {
            m_currentWaypoint =
                (m_currentWaypoint + 1) % m_waypoints.Length;
        }
    }

    // =========================
    // 追跡（Server）
    // =========================
    [Server]
    protected virtual void ChaseTarget()
    {
        if (m_target == null)
            return;

        m_syncDestination = m_target.position;
        m_isMoving = true;
    }

    // =========================
    // HERO探索（Server）
    // =========================
    [Server]
    protected void FindHeroTarget()
    {
        CharacterBase[] chars = FindObjectsOfType<CharacterBase>();
        float nearest = Mathf.Infinity;
        Transform result = null;

        foreach (var c in chars)
        {
            if (c.GetCharacterType() != CharacterType.HERO_TYPE)
                continue;

            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < detectRange && d < nearest)
            {
                nearest = d;
                result = c.transform;
            }
        }

        m_target = result;
    }

    // =========================
    // 攻撃（派生クラス実装）
    // =========================
    protected virtual IEnumerator DoAttack()
    {
        m_isAttacking = true;
        m_isMoving = false;
        yield break;
    }
}