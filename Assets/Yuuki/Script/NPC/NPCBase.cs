using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
public class NPCBase : EnemyBase
{
    [SyncVar] protected bool m_isAttacking = false;
    protected Transform m_target;
    protected Rigidbody m_rb;
    protected NavMeshAgent agent;

    // WayPoint 読み込み用（子オブジェクトの Transform を入れる）
    protected Transform[] m_waypoints;
    private int m_currentWaypoint = 0;

    [Header("AI基本設定")]
    [SerializeField] protected float detectRange = 10f;       // 索敵範囲
    [SerializeField] protected float attackRange = 2f;        // 攻撃範囲
    [SerializeField] protected float randomWalkInterval = 2f; // ランダム歩行 切替時間
    [SerializeField] protected float walkSpeedMultiplier = 0.5f;

    private float randomWalkTimer = 0f;
    private Vector3 randomDir;

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();
        TryGetComponent<NavMeshAgent>(out agent);

        if (agent != null)
        {
            // Agent側の速度は GetMoveSpeed() と連動させるために最初の速度を保存
            agent.speed = GetMoveSpeed() * walkSpeedMultiplier;
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
    }

    // ======================================
    // Server Side AI Update
    // ======================================
    [ServerCallback]
    public override void Update()
    {
        base.Update();

        if (m_isAttacking) return;

        // ターゲットがいるなら追跡または攻撃
        if (m_target != null)
        {
            float dist = Vector3.Distance(transform.position, m_target.position);

            // 索敵圏外
            if (dist > detectRange * 1.5f)
            {
                m_target = null;
                // agent を停止
                if (agent != null && agent.isOnNavMesh) agent.ResetPath();
                return;
            }

            if (dist <= attackRange)
            {
                StartCoroutine(DoAttack());
            }
            else
            {
                ChaseTarget();
            }
            return;
        }

        // ターゲットを探す
        FindHeroTarget();
        if (m_target != null) return;

        // 巡回ポイントがあれば巡回
        if (m_waypoints != null && m_waypoints.Length > 0)
        {
            Patrol();
            return;
        }

        // 無ければランダム歩行
        RandomWalk();
    }

    // ======================================
    //  WayPoint自動ロード
    // ======================================
    [Server]
    protected Transform[] FindWayPoints(string rootName)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null) return null;

        List<Transform> pts = new List<Transform>();
        foreach (Transform child in root.transform)
        {
            pts.Add(child);
        }
        return pts.ToArray();
    }

    // ======================================
    //  ランダム歩行
    // ======================================
    [Server]
    private void RandomWalk()
    {
        randomWalkTimer -= Time.deltaTime;
        if (randomWalkTimer <= 0)
        {
            randomWalkTimer = randomWalkInterval;

            randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
        }

        Vector3 move = randomDir * GetMoveSpeed() * walkSpeedMultiplier * Time.deltaTime;

        if (agent != null && agent.isOnNavMesh)
        {
            // NavMesh があるなら目的地を短く指定して移動させる（少しずつ）
            Vector3 dest = transform.position + move;
            agent.speed = GetMoveSpeed() * walkSpeedMultiplier;
            agent.SetDestination(dest);
        }
        else
        {
            transform.position += move;
            if (randomDir.sqrMagnitude > 0.01f) transform.forward = randomDir;
        }
    }

    // ======================================
    //  巡回 WayPoint
    // ======================================
    [Server]
    protected void Patrol()
    {
        if (m_waypoints == null || m_waypoints.Length == 0) return;

        Transform wp = m_waypoints[m_currentWaypoint];
        Vector3 dest = wp.position;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = GetMoveSpeed() * walkSpeedMultiplier;
            agent.SetDestination(dest);

            // 到達判定は NavMeshAgent の残り距離を利用（ただし agent.pathPending を考慮）
            if (!agent.pathPending && agent.remainingDistance <= 1.0f)
            {
                m_currentWaypoint = (m_currentWaypoint + 1) % m_waypoints.Length;
            }
        }
        else
        {
            Vector3 dir = (dest - transform.position).normalized;
            transform.position += dir * GetMoveSpeed() * walkSpeedMultiplier * Time.deltaTime;
            if (dir.sqrMagnitude > 0.01f) transform.forward = dir;

            if (Vector3.Distance(transform.position, dest) < 1f)
            {
                m_currentWaypoint = (m_currentWaypoint + 1) % m_waypoints.Length;
            }
        }
    }

    // ======================================
    // 追跡
    // ======================================
    [Server]
    protected virtual void ChaseTarget()
    {
        if (m_target == null) return;

        Vector3 dest = m_target.position;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = GetMoveSpeed();
            agent.SetDestination(dest);
        }
        else
        {
            Vector3 dir = (dest - transform.position).normalized;
            transform.position += dir * GetMoveSpeed() * Time.deltaTime;

            if (dir.sqrMagnitude > 0.01f)
                transform.forward = dir;
        }
    }

    // ======================================
    // HERO（プレイヤー）探索
    // ======================================
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

        if (result != null)
            m_target = result;
    }

    // ======================================
    // 攻撃（子クラスが実装）
    // ======================================
    protected virtual IEnumerator DoAttack()
    {
        yield break;
    }
}
