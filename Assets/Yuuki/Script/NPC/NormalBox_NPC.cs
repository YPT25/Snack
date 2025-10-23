using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBox_NPC : NPCBase
{
    [Header("攻撃判定用コライダー（isTrigger推奨）")]
    [SerializeField] private Collider m_attackCollider; // 攻撃判定に使うコライダー

    [Header("攻撃持続時間（秒）")]
    [SerializeField] private float m_attackDuration = 0.5f; // 攻撃が続く時間
    private bool m_isAttacking = false; // 現在攻撃中かどうか

    public override void Start()
    {
        base.Start();
        //SetEnemyType(EnemyType.TYPE_A);
        // 攻撃判定は通常は無効化
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }
    }

    public override void Update()
    {
        base.Update();

        // 攻撃可能なら攻撃を試みる
        TryAttack();
    }

    /// <summary>
    /// ターゲットが攻撃可能距離にいるか確認し、攻撃を開始する
    /// </summary>
    protected override void TryAttack()
    {
        if (m_isAttacking) return; // 攻撃中ならスキップ
        if (m_target == null) return; // ターゲットがいないならスキップ

        float dist = Vector3.Distance(transform.position, m_target.position);
        if (dist < 2.0f) // 仮の攻撃距離
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    /// <summary>
    /// NPCの攻撃コルーチン
    /// ・前に倒れる動作
    /// ・攻撃判定ON/OFF
    /// ・終了後に状態を戻す
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} が前方に倒れて攻撃！（NPC）");

        // （簡易）倒れる動き
        transform.Rotate(Vector3.right * 90f);

        // 攻撃判定ON
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = true;
        }

        yield return new WaitForSeconds(m_attackDuration);

        // 攻撃判定OFF
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }

        // 元の角度に戻す
        transform.Rotate(Vector3.left * 90f);

        m_isAttacking = false;
    }

    /// <summary>
    /// 攻撃判定に他キャラが入った時の処理
    /// ・攻撃中のみ有効
    /// ・自分自身は対象外
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
        {
            Attack(target);
        }
    }
}
