using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedBox_NPC : NPCBase
{
    [Header("弾丸Prefab（NetworkIdentity 必須）")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("発射位置（銃口）")]
    [SerializeField] private Transform muzzlePoint;

    [Header("弾速")]
    [SerializeField] private float projectileSpeed = 20f;

    [Header("攻撃クールダウン(秒)")]
    [SerializeField] private float attackCooldown = 1.0f;

    private bool canAttack = true;

    public override void Start()
    {
        base.Start();

        if (isServer)
            SetEnemyType(EnemyType.TYPE_C);

        if (muzzlePoint == null)
            Debug.LogError("[RangedBox_NPC] muzzlePoint を設定してください。");
    }

    // ======================================
    // 攻撃（NPCBase → 攻撃距離以内で呼ばれる）
    // ======================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking || !canAttack) yield break;

        m_isAttacking = true;
        canAttack = false;

        // --------------- 攻撃方向の計算 ----------------
        Vector3 dir = GetShootDirection();

        // --------------- 発射 ----------------
        SpawnProjectile(dir);

        // --------------- クールダウン ----------------
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        m_isAttacking = false;
    }

    // ======================================
    // 射撃方向（銃口 → ターゲット）
    // ======================================
    private Vector3 GetShootDirection()
    {
        if (m_target == null || muzzlePoint == null)
            return transform.forward;

        // ターゲットの中心（少し上）を狙う
        Vector3 targetPos = m_target.position + Vector3.up * 1.0f;

        return (targetPos - muzzlePoint.position).normalized;
    }

    // ======================================
    // サーバー側で弾生成
    // ======================================
    [Server]
    private void SpawnProjectile(Vector3 dir)
    {
        if (projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(dir));

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
            projectile.Initialize(this, GetPower());

        if (rb != null)
            rb.velocity = dir * projectileSpeed;

        NetworkServer.Spawn(proj);
    }
}