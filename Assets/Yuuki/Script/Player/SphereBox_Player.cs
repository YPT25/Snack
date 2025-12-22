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

    [Header("突撃の持続時間（秒）")]
    [SerializeField] private float m_rollDuration = 1.0f;

    [Header("突撃の力")]
    [SerializeField] private float m_rollForce = 15f;

    [Header("突撃モード設定")]
    // 突撃時間
    [SerializeField] private float dashDuration = 1.0f;
    // 移動速度アップ倍率
    [SerializeField] private float dashSpeedMultiplier = 2f;
    // クールタイム
    [SerializeField] private float dashCooldown = 3.0f;

    private bool m_isDashing = false;
    private bool m_canDash = true;

    private bool m_isAttacking = false;
    // ノックバックの威力
    [SerializeField] private float knockbackForce = 20f;

    public override void Start()
    {
        base.Start();

        // 見た目変更（クライアント側のみ）
        if (isClient)
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Color.blue;
        }

        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        // 敵種別を設定
        if (isServer)
            SetEnemyType(EnemyType.TYPE_B);
    }

    /// <summary>
    /// 攻撃入力時の処理（クライアント側で呼ばれ、サーバーに通知）
    /// </summary>
    protected override void OnAttackInput()
    {
        if (!m_isAttacking)
            CmdStartRollAttack();
    }

    /// <summary>
    /// 攻撃入力をサーバーに伝える
    /// </summary>
    [Command]
    private void CmdStartRollAttack()
    {
        if (!m_isAttacking)
            StartCoroutine(DashModeCoroutine());
    }

    /// <summary>
    /// 転がって突撃する攻撃のコルーチン
    /// </summary>
    private IEnumerator DashModeCoroutine()
    {
        m_isDashing = true;
        m_canDash = false;

        // 移動速度アップ
        float originalSpeed = GetMoveSpeed();
        SetMoveSpeed(originalSpeed * dashSpeedMultiplier);

        // 攻撃判定ON
        if (m_attackCollider != null)
            m_attackCollider.enabled = true;

        // 突撃モード持続
        yield return new WaitForSeconds(dashDuration);

        // 元に戻す
        SetMoveSpeed(originalSpeed);
        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        m_isDashing = false;

        // クールタイム
        yield return new WaitForSeconds(dashCooldown);
        m_canDash = true;
    }


    /// <summary>
    /// クライアント側にも攻撃フラグを同期
    /// </summary>
    [ClientRpc]
    private void RpcSetAttackState(bool state)
    {
        m_isAttacking = state;
        if (m_attackCollider != null)
            m_attackCollider.enabled = state;
    }

    /// <summary>
    /// 攻撃判定に相手が入った時の処理（サーバーでのみ有効）
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking || !isServer) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
        {
            // ダメージ処理
            Attack(target);

            // ノックバック
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                // 吹っ飛ばす方向
                Vector3 dir = (other.transform.position - transform.position).normalized;
                dir.y = 1.3f;
                dir.Normalize();

                // 速度リセット
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // 直接速度上書き
                rb.velocity = dir * knockbackForce;

                // AddForceの併用
                rb.AddForce(dir * (knockbackForce * 0.5f), ForceMode.Impulse);
            }
        }
    }
}


