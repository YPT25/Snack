using System.Collections;
using UnityEngine;
using Mirror;
using Mirror.Examples.Tanks;

/// <summary>
/// RangedBox のプレイヤー操作用クラス
/// ・MPlayerBaseを継承
/// ・左クリックで弾を発射する
/// ・サーバーが弾丸を生成し、NetworkServer.Spawnで全クライアントに反映
/// </summary>
public class RangedBox_Player : MPlayerBase
{
    [Header("発射する弾丸のプレハブ（NetworkIdentity付き必須）")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("弾丸の発射位置")]
    [SerializeField] private Transform muzzlePoint;

    [Header("弾速")]
    [SerializeField] private float projectileSpeed = 20f;

    [Header("攻撃クールダウン(秒)")]
    [SerializeField] private float attackCooldown = 1.0f;

    private bool canAttack = true;

    public override void Start()
    {
        base.Start();

        if (isClient)
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Color.cyan;
        }

        if (isServer)
            SetEnemyType(EnemyType.TYPE_C);
    }

    protected override void OnAttackInput()
    {
        if (canAttack)
            CmdShoot();
    }

    [Command]
    private void CmdShoot()
    {
        if (!canAttack || projectilePrefab == null || muzzlePoint == null)
            return;

        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        canAttack = false;
        RpcSetAttackCooldown(false);

        // 弾丸生成（サーバー側）
        GameObject proj = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(this, GetPower());
        }

        if (rb != null)
        {
            rb.velocity = muzzlePoint.forward * projectileSpeed;
        }

        NetworkServer.Spawn(proj);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        RpcSetAttackCooldown(true);
    }

    [ClientRpc]
    private void RpcSetAttackCooldown(bool _canAttack)
    {
        canAttack = _canAttack;
    }
}