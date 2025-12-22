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
    [Header("弾丸Prefab（NetworkIdentity 必須）")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("発射位置（銃口）")]
    [SerializeField] private Transform muzzlePoint;

    [Header("射線表示用（AimPointにLineRenderer）")]
    [SerializeField] private LineRenderer aimLine;

    [Header("レティクルUI（Canvas内のCrossHair画像）")]
    [SerializeField] private GameObject crossHair;

    [Header("色設定")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hitColor = Color.red;

    [Header("弾速")]
    [SerializeField] private float projectileSpeed = 20f;

    [Header("攻撃クールダウン(秒)")]
    [SerializeField] private float attackCooldown = 1.0f;

    private bool canAttack = true;
    //デバック用切り替え
    private bool debugAimLine = false;

    public override void Start()
    {
        base.Start();

        if (isServer)
            SetEnemyType(EnemyType.TYPE_C);

        // ローカルプレイヤーのみレティクル/ライン表示
        if (isLocalPlayer)
        {
            if (crossHair != null) crossHair.SetActive(true);
            if (aimLine != null) aimLine.enabled = true;
        }
        else
        {
            if (crossHair != null) crossHair.SetActive(false);
            if (aimLine != null) aimLine.enabled = false;
        }
    }

    public override void Update()
    {
        base.Update();
        if (!isLocalPlayer) return;

        // FPS状態に合わせてUI管理
        UpdateCrossHair();
        UpdateAimLine();

        // =================== デバッグ用 AimLine トグル ===================
        if (Input.GetKeyDown(KeyCode.P))
        {
            debugAimLine = !debugAimLine;

            if (aimLine != null)
                aimLine.enabled = debugAimLine;

            Debug.Log("AimLine Debug Mode: " + debugAimLine);
        }
    }

    private void UpdateCrossHair()
    {
        if (crossHair == null) return;
        crossHair.SetActive(GetIsFPS());   // ← FPS時のみ ON
    }

    // ============================
    //   レティクル & バレットライン
    // ============================
    private void UpdateAimLine()
    {
        if (aimLine == null || muzzlePoint == null) return;
        if (!debugAimLine || aimLine == null) return;

        if (!GetIsFPS())  // ← FPS時のみ表示
        {
            aimLine.enabled = false;
            return;
        }

        aimLine.enabled = true;
        aimLine.SetPosition(0, muzzlePoint.position);

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            aimLine.SetPosition(1, hit.point);

            // ★ 当たりが「敵キャラ」なら赤
            if (hit.collider.GetComponent<CharacterBase>() != null)
            {
                aimLine.startColor = hitColor;
                aimLine.endColor = hitColor;
            }
            else
            {
                aimLine.startColor = normalColor;
                aimLine.endColor = normalColor;
            }
        }
        else
        {
            aimLine.SetPosition(1, ray.GetPoint(200f));
            aimLine.startColor = normalColor;
            aimLine.endColor = normalColor;
        }
    }

    // ============================
    //   射撃処理
    // ============================
    protected override void OnAttackInput()
    {
        if (!canAttack) return;

        // FPS/TPS問わず正確に照準方向
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(200f);

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