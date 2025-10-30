using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// CannonBox NPC（AI制御付き）
/// ターゲットに向けて遠距離攻撃を行う
/// </summary>
public class CannonBox_NPC : NPCBase
{
    [Header("弾丸プレハブ（Rigidbody付き）")]
    [SerializeField] private GameObject m_cannonBallPrefab;

    [Header("発射位置（砲口 Transform）")]
    [SerializeField] private Transform m_firePoint;

    [Header("射程距離")]
    [SerializeField] private float m_attackRange = 10f;

    [Header("弾速")]
    [SerializeField] private float m_shotSpeed = 15f;

    [Header("攻撃間隔（秒）")]
    [SerializeField] private float m_attackCooldown = 2.0f;

    private bool m_isOnCooldown = false;

    public override void Update()
    {
        base.Update();

        if (m_target == null) return;

        // 攻撃可能距離に入ったら射撃
        float dist = Vector3.Distance(transform.position, m_target.position);
        if (!m_isOnCooldown && dist <= m_attackRange)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    /// <summary>
    /// 攻撃コルーチン（遠距離射撃）
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;
        m_isOnCooldown = true;

        // ターゲット方向を向く
        Vector3 lookPos = m_target.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // 発射処理
        FireProjectile();

        yield return new WaitForSeconds(m_attackCooldown);

        m_isAttacking = false;
        m_isOnCooldown = false;
    }

    /// <summary>
    /// 実際の弾生成と発射
    /// </summary>
    private void FireProjectile()
    {
        if (m_cannonBallPrefab == null || m_firePoint == null)
        {
            Debug.LogWarning($"{name}：弾丸または発射位置が設定されていません。");
            return;
        }

        GameObject ball = Instantiate(m_cannonBallPrefab, m_firePoint.position, m_firePoint.rotation);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb != null && m_target != null)
        {
            Vector3 dir = (m_target.position - m_firePoint.position).normalized;
            rb.velocity = dir * m_shotSpeed;
        }

        Debug.Log($"{name} が砲弾を発射！");
    }
}
