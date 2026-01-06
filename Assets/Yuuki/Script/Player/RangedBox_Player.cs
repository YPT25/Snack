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

    private bool canAttackLocal = true;

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

        // ====== 射撃入力：ローカルで照準計算 → Cmdでサーバーへ ======
        if (LegacyInputHelper.GetAttackDown())
        {
            TryShootLocal();
        }

        // デバッグ AimLine トグル
        if (Input.GetKeyDown(KeyCode.P))
        {
            debugAimLine = !debugAimLine;
            if (aimLine != null) aimLine.enabled = debugAimLine;
            Debug.Log("AimLine Debug Mode: " + debugAimLine);
        }
    }

    private void TryShootLocal()
    {
        if (!canAttackLocal) return;
        if (projectilePrefab == null || muzzlePoint == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // FPS/TPS問わず、カメラ前方で照準
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, 200f) ? hit.point : ray.GetPoint(200f);

        Vector3 dir = (targetPoint - muzzlePoint.position).normalized;

        // ローカルのクールダウン
        StartCoroutine(LocalCooldownRoutine());

        // サーバーに発射を依頼（弾生成はサーバー）
        CmdShoot(dir);
    }

    private IEnumerator LocalCooldownRoutine()
    {
        canAttackLocal = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttackLocal = true;
    }

    // ====== UI ======
    private void UpdateCrossHair()
    {
        if (crossHair == null) return;
        crossHair.SetActive(GetIsFPS());   // FPS時のみON
    }

    private void UpdateAimLine()
    {
        if (aimLine == null || muzzlePoint == null) return;
        if (!debugAimLine) return;

        if (!GetIsFPS())
        {
            aimLine.enabled = false;
            return;
        }

        aimLine.enabled = true;
        aimLine.SetPosition(0, muzzlePoint.position);

        Camera cam = Camera.main;
        if (cam == null)
        {
            aimLine.SetPosition(1, muzzlePoint.position + transform.forward * 10f);
            aimLine.startColor = normalColor;
            aimLine.endColor = normalColor;
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            aimLine.SetPosition(1, hit.point);

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

    // ====== サーバー：弾生成 ======
    [Command]
    private void CmdShoot(Vector3 dir)
    {
        if (projectilePrefab == null || muzzlePoint == null) return;

        // （任意）簡易チート対策：dirが変な値なら拒否
        if (dir.sqrMagnitude < 0.9f || dir.sqrMagnitude > 1.1f) return;

        StartCoroutine(ServerShootRoutine(dir));
    }

    [Server]
    private IEnumerator ServerShootRoutine(Vector3 dir)
    {
        GameObject proj = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
            projectile.Initialize(this, GetPower()); // ← Projectal.csの定義に合わせる :contentReference[oaicite:4]{index=4}

        if (rb != null)
            rb.velocity = dir * projectileSpeed;

        NetworkServer.Spawn(proj);

        // サーバー側でも最低限の連射抑制をしたいなら、ここで待ってもOK
        yield return null;
    }

    // ★重要：MPlayerBase経由の攻撃は使わない（サーバーでCamera.mainを触らない）
    protected override void OnAttackInput() { }
}