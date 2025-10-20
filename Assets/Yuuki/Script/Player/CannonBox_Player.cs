using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CannonBox プレイヤー（砲撃攻撃持ち）
/// 攻撃時に弾丸Prefabを生成して前方に飛ばす
/// </summary
public class CannonBox_Player : MPlayerBase
{
    [Header("弾丸プレハブ")]
    public GameObject cannonBallPrefab;

    [Header("弾丸発射位置")]
    public Transform firePoint;
    [Header("弾丸発射速度")]
    public float fireSpeed;

    protected override void OnAttackInput()
    {
        Attack(null); // 近接対象は不要
    }

    public override void Attack(CharacterBaseY target)
    {
        if (cannonBallPrefab == null || firePoint == null) return;

        GameObject ball = Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * fireSpeed; // 発射速度
        }

        Debug.Log($"{name} が砲撃を発射！");
    }
}
