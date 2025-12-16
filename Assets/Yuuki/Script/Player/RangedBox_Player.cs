using System.Collections;
using UnityEngine;
using Mirror;
using Mirror.Examples.Tanks;
using Mirror.Examples.Common;

/// <summary>
/// RangedBox のプレイヤー操作用クラス
/// ・MPlayerBaseを継承
/// ・左クリックで弾を発射する
/// ・サーバーが弾丸を生成し、NetworkServer.Spawnで全クライアントに反映
/// </summary>
public class RangedBox_Player : MPlayerBase
{
    //[Header("弾丸Prefab（NetworkIdentity 必須）")]
    //[SerializeField] private GameObject projectilePrefab;

    //[Header("発射位置（銃口）")]
    //[SerializeField] private Transform muzzlePoint;

    //[Header("射線表示用（AimPointにLineRenderer）")]
    //[SerializeField] private LineRenderer aimLine;

    //[Header("レティクルUI（Canvas内のCrossHair画像）")]
    //[SerializeField] private GameObject crossHair;

    //[Header("色設定")]
    //[SerializeField] private Color normalColor = Color.white;
    //[SerializeField] private Color hitColor = Color.red;

    //[Header("弾速")]
    //[SerializeField] private float projectileSpeed = 20f;

    //[Header("攻撃クールダウン(秒)")]
    //[SerializeField] private float attackCooldown = 1.0f;

    //private bool canAttack = true;
    ////デバック用切り替え
    //private bool debugAimLine = false;

    [Header("射撃設定")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootForce = 20f;
    [SerializeField] private float attackInterval = 0.6f;

    private bool canAttack = true;

    public override void Start()
    {
        base.Start();

        if (isServer)
            SetEnemyType(EnemyType.TYPE_C);
    }

    // =========================
    // 攻撃入力（MPlayerBaseから呼ばれる）
    // =========================
    protected override void OnAttackInput()
    {
        if (!canAttack)
            return;

        canAttack = false;
        CmdShoot();
    }

    // =========================
    // 弾発射（Server）
    // =========================
    [Command]
    private void CmdShoot()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        GameObject proj = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * shootForce;
        }

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(this, GetPower());
        }

        NetworkServer.Spawn(proj);

        RpcResetAttackCooldown();
    }

    // =========================
    // 攻撃クールタイム（Client）
    // =========================
    [ClientRpc]
    private void RpcResetAttackCooldown()
    {
        Invoke(nameof(ResetAttack), attackInterval);
    }

    private void ResetAttack()
    {
        canAttack = true;
    }
}