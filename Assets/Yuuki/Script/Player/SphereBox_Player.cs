using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// SphereBox のプレイヤー操作用クラス
/// ・MPlayerBase を継承
/// ・左クリックで「前方に転がって突撃する」攻撃を実行
/// ・攻撃中は Rigidbody に力を加えて移動し、攻撃判定コライダーをONにする
/// </summary>
public class SphereBox_Player : MPlayerBase
{
    [Header("攻撃判定用コライダー（isTrigger推奨）")]
    [SerializeField] private Collider m_attackCollider;

    [Header("突撃モード設定")]
    [SerializeField] private float dashDuration = 1.0f;
    [SerializeField] private float dashSpeedMultiplier = 2f;
    [SerializeField] private float dashCooldown = 3.0f;

    [Header("ノックバックの威力")]
    [SerializeField] private float knockbackForce = 20f;

    // ===== クライアント側（見た目・操作） =====
    private bool m_isDashingLocal = false;
    private bool m_localHitSent = false;
    private float m_dashEndLocalTime = 0f;
    private float m_savedSpeedLocal = 0f;

    // ===== サーバー側（確定・検証） =====
    [SyncVar] private bool m_isDashingServer = false;
    [SyncVar] private bool m_canDashServer = true;
    private double m_dashEndServerTime = 0;

    // サーバー検証用（緩めでOK）
    private const float HIT_DISTANCE_EPS = 1.8f; // 体当たり許容距離（環境で調整）

    public override void Start()
    {
        base.Start();

        // 初期はOFF（ローカルダッシュ中だけONにする）
        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        if (isServer)
            SetEnemyType(EnemyType.TYPE_B);
    }

    /// <summary>
    /// 攻撃入力（MPlayerBaseのCmdAttackInputから「サーバーで」呼ばれるが、
    /// A方式では “突進の見た目” はクライアントでやりたいので、
    /// ここではサーバー側の「ダッシュ開始許可・状態記録」だけ行う。
    /// </summary>
    protected override void OnAttackInput()
    {
        // ここはサーバーで呼ばれる
        if (!isServer) return;

        if (!m_canDashServer || m_isDashingServer) return;

        // ダッシュ開始を全体の正当性として記録
        m_isDashingServer = true;
        m_canDashServer = false;
        m_dashEndServerTime = NetworkTime.time + dashDuration;

        // ローカル側に「見た目のダッシュ開始」を通知
        // TargetRpcで本人だけに送る（他人の見た目はClientToServer同期で自然に見える）
        TargetStartDash(connectionToClient, dashDuration, dashSpeedMultiplier);

        // ダッシュ終了・クールダウン
        StartCoroutine(ServerDashEndRoutine());
    }

    [Server]
    private IEnumerator ServerDashEndRoutine()
    {
        // ダッシュ時間待ち
        yield return new WaitForSeconds(dashDuration);

        m_isDashingServer = false;

        // クールダウン
        yield return new WaitForSeconds(dashCooldown);

        m_canDashServer = true;
    }

    [TargetRpc]
    private void TargetStartDash(NetworkConnection target, float duration, float speedMul)
    {
        // 既にダッシュ中なら無視
        if (m_isDashingLocal) return;

        m_isDashingLocal = true;
        m_localHitSent = false;

        m_savedSpeedLocal = GetMoveSpeed();
        SetMoveSpeed(m_savedSpeedLocal * speedMul);

        m_dashEndLocalTime = Time.time + duration;

        if (m_attackCollider != null)
            m_attackCollider.enabled = true;

        // 通常移動は止めないと「プルプル」になりやすいので、ダッシュ中は止める
        // （これで “方向に一瞬行って戻される” が消える）
        iscanMove = false;
    }

    private void Update()
    {
        // ローカルダッシュ終了処理（時間切れ）
        if (isLocalPlayer && m_isDashingLocal && Time.time >= m_dashEndLocalTime)
        {
            EndDashLocal();
        }

        base.Update();
    }

    public override void FixedUpdate()
    {
        // ローカルダッシュ中だけ「前進突進」を上書き
        if (isLocalPlayer && m_isDashingLocal && m_rb != null)
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            float speed = GetMoveSpeed();
            Vector3 v = m_rb.velocity;
            v.x = forward.x * speed;
            v.z = forward.z * speed;
            m_rb.velocity = v;

            return; 
        }

        base.FixedUpdate();
    }

    private void EndDashLocal()
    {
        m_isDashingLocal = false;

        // 速度戻す
        SetMoveSpeed(m_savedSpeedLocal);

        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        // 通常移動復帰
        iscanMove = true;

        // 水平成分だけ止めたい場合はここで止めてもOK
        // if (m_rb != null) m_rb.velocity = new Vector3(0, m_rb.velocity.y, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        // A方式：当たり検出は「ローカル」だけでOK（命中確定はサーバー）
        if (!isLocalPlayer) return;
        if (!m_isDashingLocal) return;
        if (m_localHitSent) return;

        // 自分自身は除外
        if (other.attachedRigidbody == m_rb) return;

        // ネットワーク対象だけ報告（サーバーで特定できるように）
        NetworkIdentity ni = other.GetComponentInParent<NetworkIdentity>();
        if (ni == null) return;
        if (ni == netIdentity) return;

        // 送信（多重送信防止）
        m_localHitSent = true;
        CmdReportDashHit(ni);
    }

    [Command]
    private void CmdReportDashHit(NetworkIdentity targetNi)
    {
        if (targetNi == null) return;
        if (!m_isDashingServer) return;                 // サーバー的にダッシュ中じゃなければ無効
        if (NetworkTime.time > m_dashEndServerTime + 0.15) return; // 少しだけ猶予、基本は無効

        // 距離検証（緩め）
        float dist = Vector3.Distance(transform.position, targetNi.transform.position);
        if (dist > HIT_DISTANCE_EPS) return;

        // 対象取得
        CharacterBase target = targetNi.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target == this) return;

        // ダメージ確定（サーバー）
        Attack(target);

        // ノックバック（サーバー）
        Rigidbody rb = targetNi.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (targetNi.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
            dir.Normalize();

            Vector3 knock = dir;
            knock.y = 1.3f;
            knock.Normalize();

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.velocity = knock * knockbackForce;
            rb.AddForce(knock * (knockbackForce * 0.5f), ForceMode.Impulse);
        }
    }
}