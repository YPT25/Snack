using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NormalBox のプレイヤー操作用クラス
/// ・MPlayerBase を継承
/// ・左クリックで「前方に倒れる攻撃」を実行
/// ・攻撃中のみ攻撃判定用コライダーを有効化する
/// </summary>
public class NormalBox_Player : MPlayerBase
{
    // 攻撃判定に使うコライダー（Inspectorから設定）
    [Header("攻撃判定用コライダー（isTrigger推奨）")]
    [SerializeField] private Collider m_attackCollider;
    // 攻撃判定が有効な時間
    [Header("攻撃持続時間（秒）")]
    [SerializeField] private float m_attackDuration = 0.5f;
    // 攻撃中かどうかを管理
    private bool m_isAttacking = false; 

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_A);
        //オブジェクトの色を赤に変更する
        GetComponent<Renderer>().material.color = Color.red;
        // 攻撃判定は通常は無効化しておく
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }

    }

    /// <summary>
    /// プレイヤーの攻撃入力（左クリック）時に呼ばれる
    /// </summary>
    protected override void OnAttackInput()
    {
        // 攻撃中でなければ攻撃を開始
        if (!m_isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    /// <summary>
    /// 攻撃処理を一定時間行うコルーチン
    /// ・前に倒れる動作
    /// ・攻撃判定ON/OFF
    /// ・終了後に状態を戻す
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} が前方に倒れて攻撃！");

        // 前に倒れるアニメーション（仮）
        transform.Rotate(Vector3.right * 30f);

        // 攻撃判定を有効化
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = true;
        }

        // 攻撃持続時間だけ待つ
        yield return new WaitForSeconds(m_attackDuration);

        // 攻撃判定を無効化
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }

        // 元の角度に戻す
        transform.Rotate(Vector3.left * 30f);

        m_isAttacking = false;
    }

    /// <summary>
    /// 攻撃判定コライダーに誰かが入った時の処理
    /// ・攻撃中のみ有効
    /// ・自分自身は対象外
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
        {
            // EnemyBase の Attack を呼ぶ（ダメージ処理は共通）
            Attack(target);
        }
    }
}
