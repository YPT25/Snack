using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SphereBox のプレイヤー操作用クラス
/// ・MPlayerBase を継承
/// ・左クリックで「前方に転がって突撃する」攻撃を実行
/// ・攻撃中は Rigidbody に力を加えて移動し、攻撃判定コライダーをONにする
/// </summary>
public class SphereBox_Player : MPlayerBase
{
    // 攻撃判定コライダ
    [Header("攻撃判定用コライダー（isTrigger推奨）")]
    [SerializeField] private Collider m_attackCollider;
    // 突撃する時間
    [Header("突撃の持続時間（秒）")]
    [SerializeField] private float m_rollDuration = 1.0f;
    // 突撃中に与える前方への力
    [Header("突撃の力")]
    [SerializeField] private float m_rollForce = 15f;
    // 攻撃中フラグ
    private bool m_isAttacking = false; 

    public override void Start()
    {
        base.Start();
        //オブジェクトの色を赤に変更する
        GetComponent<Renderer>().material.color = Color.blue;
        // 攻撃判定は初期状態でOFF
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }
    }

    /// <summary>
    /// プレイヤー攻撃入力（左クリック）時の処理
    /// </summary>
    protected override void OnAttackInput()
    {
        if (!m_isAttacking)
        {
            StartCoroutine(RollAttackCoroutine());
        }
    }

    /// <summary>
    /// 転がって突撃する攻撃のコルーチン
    /// ・前方に力を加える
    /// ・攻撃判定ON/OFF
    /// </summary>
    private IEnumerator RollAttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} が転がって突撃！（Player）");

        // 攻撃判定ON
        if (m_attackCollider != null) m_attackCollider.enabled = true;

        // Rigidbody に前方へ力を加える
        if (m_rb != null)
        {
            m_rb.AddForce(transform.forward * m_rollForce, ForceMode.Impulse);
        }

        // 一定時間突撃
        yield return new WaitForSeconds(m_rollDuration);

        // 攻撃判定OFF
        if (m_attackCollider != null) m_attackCollider.enabled = false;

        m_isAttacking = false;
    }

    /// <summary>
    /// 攻撃判定に相手が入った時の処理
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
        {
            // EnemyBaseのAttackを利用
            Attack(target); 
        }
    }
}
