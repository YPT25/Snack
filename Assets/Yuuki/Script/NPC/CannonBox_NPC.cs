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
    [Header("弾丸プレハブ")]
    public GameObject cannonBallPrefab;

    [Header("弾丸発射位置")]
    public Transform firePoint;

    protected override void TryAttack()
    {
        if (m_target == null) return;

        float dist = Vector3.Distance(transform.position, m_target.position);
        if (dist < 10.0f) // 射程
        {
            Attack(m_target.GetComponent<CharacterBase>());
        }
    }

    public override void Attack(CharacterBase target)
    {
        if (cannonBallPrefab == null || firePoint == null) return;

        GameObject ball = Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = (m_target.position - firePoint.position).normalized * 15f;
        }

        Debug.Log($"{name} (NPC) が砲撃を発射！");
    }
}
