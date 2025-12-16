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
    [Header("攻撃判定用コライダー")]
    [SerializeField] private Collider m_attackCollider;

    [Header("突撃設定")]
    [SerializeField] private float dashDuration = 1.0f;
    [SerializeField] private float dashSpeedMultiplier = 2f;
    [SerializeField] private float dashCooldown = 3.0f;

    [Header("ノックバック")]
    [SerializeField] private float knockbackForce = 20f;

    // =========================
    // 同期用
    // =========================
    [SyncVar] private bool m_isAttacking = false;

    private bool m_canDash = true;

    // =========================
    // 初期化
    // =========================
    public override void Start()
    {
        base.Start();

        if (isServer)
            SetEnemyType(EnemyType.TYPE_B);

        if (m_attackCollider != null)
            m_attackCollider.enabled = false;
    }

    // =========================
    // 攻撃入力（Client）
    // =========================
    protected override void OnAttackInput()
    {
        if (!isLocalPlayer || !m_canDash || m_isAttacking)
            return;

        CmdRequestDash();
    }

    // =========================
    // 攻撃開始要求（Server）
    // =========================
    [Command]
    private void CmdRequestDash()
    {
        if (m_isAttacking || !m_canDash)
            return;

        m_isAttacking = true;
        m_canDash = false;

        RpcStartDashVisual();
        StartCoroutine(DashRoutine_Server());
    }

    // =========================
    // 見た目突撃（Client）
    // =========================
    [ClientRpc]
    private void RpcStartDashVisual()
    {
        StartCoroutine(DashVisual_Client());
    }

    private IEnumerator DashVisual_Client()
    {
        float originalSpeed = GetMoveSpeed();
        SetMoveSpeed(originalSpeed * dashSpeedMultiplier);

        if (m_attackCollider != null)
            m_attackCollider.enabled = true;

        yield return new WaitForSeconds(dashDuration);

        SetMoveSpeed(originalSpeed);

        if (m_attackCollider != null)
            m_attackCollider.enabled = false;
    }

    // =========================
    // 攻撃管理（Server）
    // =========================
    [Server]
    private IEnumerator DashRoutine_Server()
    {
        yield return new WaitForSeconds(dashDuration);

        m_isAttacking = false;

        yield return new WaitForSeconds(dashCooldown);
        m_canDash = true;
    }

    // =========================
    // 当たり判定（Server）
    // =========================
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking)
            return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null || target == this)
            return;

        Attack(target);

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;
            dir.y = 1.2f;
            dir.Normalize();

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
        }
    }
}