using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class NPCBase : EnemyBase
{
    [SyncVar] protected bool m_isAttacking = false;
    protected Transform m_target;
    protected Rigidbody m_rb;

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
    }

    // ======================================
    // Server Side AI Update
    // ======================================
    [ServerCallback]
    public override void Update()
    {
        base.Update();

        if (m_isAttacking) return;

        // ① ターゲットがいる → 追跡 or 攻撃
        if (m_target != null)
        {
            float dist = Vector3.Distance(transform.position, m_target.position);

            // 索敵圏外
            if (dist > detectRange * 1.5f)
            {
                m_target = null;
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

        // ② ターゲットを探す
        FindHeroTarget();
        if (m_target != null) return;

        // ③ 巡回ポイントがあれば巡回
        if (m_waypoints != null && m_waypoints.Length > 0)
        {
            Patrol();
            return;
        }

        // ④ 無ければランダム歩行
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

        transform.position += randomDir * GetMoveSpeed() * walkSpeedMultiplier * Time.deltaTime;

        if (randomDir.sqrMagnitude > 0.01f)
            transform.forward = randomDir;
    }

    // ======================================
    //  巡回 WayPoint
    // ======================================
    [Server]
    protected void Patrol()
    {
        if (m_waypoints == null || m_waypoints.Length == 0) return;

        Transform wp = m_waypoints[m_currentWaypoint];

        Vector3 dir = (wp.position - transform.position).normalized;
        transform.position += dir * GetMoveSpeed() * walkSpeedMultiplier * Time.deltaTime;

        if (dir.sqrMagnitude > 0.01f)
            transform.forward = dir;

        // 到達判定
        if (Vector3.Distance(transform.position, wp.position) < 1f)
        {
            m_currentWaypoint = (m_currentWaypoint + 1) % m_waypoints.Length;
        }
    }

    // ======================================
    // 追跡
    // ======================================
    [Server]
    protected virtual void ChaseTarget()
    {
        Vector3 dir = (m_target.position - transform.position).normalized;

        transform.position += dir * GetMoveSpeed() * Time.deltaTime;

        if (dir.sqrMagnitude > 0.01f)
            transform.forward = dir;
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
