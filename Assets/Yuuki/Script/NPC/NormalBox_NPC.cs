using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    // 索敵範囲
    [SerializeField] private float m_attackRange = 2.0f;
    // 攻撃距離
    [Header("巡回ポイント設定")]
    [SerializeField] private Transform[] m_waypoints;
    [SerializeField] private float m_patrolSpeed = 2.0f;

    [SerializeField] private float m_chaseSpeed = 3.5f;
    private int m_currentWaypoint = 0;
    // 現在追跡しているプレイヤー
    private bool m_isOnCooldown = false;
    // 攻撃クールタイム中か
    public override void Start()
    {
        base.Start(); if (m_attackCollider != null) m_attackCollider.enabled = false;
    }
    public override void Update()
    {
        base.Update();
        if (m_isAttacking) return;
        // 攻撃中は移動停止
        if (m_target != null)
        {
            float dist = Vector3.Distance(transform.position, m_target.position);
            // 索敵範囲外ならターゲット解除
            if (dist > m_detectRange * 1.5f) { m_target = null; return; }
            // 攻撃可能距離内なら移動せず攻撃
            if (dist <= m_attackRange)
            {
                if (!m_isOnCooldown) StartCoroutine(AttackCoroutine());
            }
            else
            {
                // 攻撃範囲外の時のみ追跡
                MoveTowards(m_target.position, m_chaseSpeed);
            }
        }
        else
        {
            // 索敵してターゲット探す
            FindTarget();
            // ターゲットいなければ巡回
            if (m_target == null) Patrol();
        }
    }
    /// <summary> 
    /// /// ターゲット探索 
    /// /// </summary>
    private void FindTarget()
    {
        // シーン内のすべての CharacterBase を取得
        CharacterBase[] characters = FindObjectsOfType<CharacterBase>();
        float nearest = Mathf.Infinity;
        Transform nearestHero = null;
        foreach (var c in characters)
        {
            // 同じENEMY_TYPE はスキップ
            if (c == this || c.GetCharacterType() == CharacterBase.CharacterType.ENEMY_TYPE) continue;
            // Heroだけ狙う
            if (c.GetCharacterType() == CharacterBase.CharacterType.HERO_TYPE)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < m_detectRange && dist < nearest)
                {
                    nearest = dist; nearestHero = c.transform;
                }
            }
        }
        if (nearestHero != null)
        {
            m_target = nearestHero; Debug.Log($"{name} が {m_target.name}（HERO）を発見！");
        }
    }
    /// <summary> 
    /// /// 巡回処理 
    /// /// </summary>
    private void Patrol()
    {
        if (m_waypoints == null || m_waypoints.Length == 0) return;
        Transform wp = m_waypoints[m_currentWaypoint];
        MoveTowards(wp.position, m_patrolSpeed);
        float dist = Vector3.Distance(transform.position, wp.position);
        if (dist < 1.0f)
        {
            // 次の巡回ポイントへ
            m_currentWaypoint = (m_currentWaypoint + 1) % m_waypoints.Length;
        }
    }
    /// <summary> 
    /// /// 指定方向へ向かって移動
    /// /// </summary>
    private void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        // 向き補正
        if (dir.sqrMagnitude > 0.01f) transform.forward = dir;
    }
    /// <summary> 
    /// /// 攻撃アクション
    /// /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true; m_isOnCooldown = true;
        Debug.Log($"{name} が攻撃動作を開始！");
        if (m_attackCollider != null) m_attackCollider.enabled = true;
        transform.Rotate(Vector3.right * 45f);
        yield return new WaitForSeconds(m_attackDuration);
        if (m_attackCollider != null) m_attackCollider.enabled = false;
        transform.Rotate(Vector3.left * 45f); m_isAttacking = false;
        // 攻撃クールタイム
        yield return new WaitForSeconds(m_attackCooldown);
        m_isOnCooldown = false;
    }
    /// <summary>
    /// 攻撃判定に他キャラが入ったとき 
    /// /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;
        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        // 同陣営は攻撃しない
        if (target.GetCharacterType() == CharacterBase.CharacterType.ENEMY_TYPE) return;
        target.Damage(GetPower());
        Debug.Log($"{name} が {other.name} に {GetPower()} ダメージ！ 残HP:{target.GetHp()}");
    }
}