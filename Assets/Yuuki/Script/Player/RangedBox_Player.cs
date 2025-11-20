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
    }

    protected override void OnAttackInput()
    {
        if (!canAttack) return;

        // ========= 正確な射撃方向の取得 (FPS/TPS両対応) =========
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(200f);
        }

        // 銃口 → 狙っている点 の方向
        Vector3 dir = (targetPoint - muzzlePoint.position).normalized;

        CmdShoot(dir);
    }

    [Command]
    private void CmdShoot(Vector3 dir)
    {
        if (!canAttack || projectilePrefab == null)
            return;

        StartCoroutine(ShootRoutine(dir));
    }

    private IEnumerator ShootRoutine(Vector3 dir)
    {
        canAttack = false;
        RpcSetAttackCooldown(false);

        GameObject proj = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
            projectile.Initialize(this, GetPower());

        // カメラからの射線に一致
        if (rb != null)
            rb.velocity = dir * projectileSpeed;

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