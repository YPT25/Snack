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

    private bool m_isAttacking = false;

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
            StartCoroutine(RollAttackCoroutine());
    }

    /// <summary>
    /// 転がって突撃する攻撃のコルーチン
    /// </summary>
    private IEnumerator RollAttackCoroutine()
    {
        m_isAttacking = true;
        RpcSetAttackState(true);

        Debug.Log($"{name} が転がって突撃！（Server）");

        // 攻撃判定ON
        if (m_attackCollider != null)
            m_attackCollider.enabled = true;

        // Rigidbody に前方へ力を加える（サーバー物理で処理）
        if (m_rb != null)
            m_rb.AddForce(transform.forward * m_rollForce, ForceMode.Impulse);

        yield return new WaitForSeconds(m_rollDuration);

        // 攻撃判定OFF
        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        m_isAttacking = false;
        RpcSetAttackState(false);
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
            Attack(target);
    }
}
