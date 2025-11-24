using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/// <summary>
/// NormalBox のプレイヤー操作用クラス
/// ・MPlayerBase を継承
/// ・左クリックで「前方に倒れる攻撃」を実行
/// ・攻撃中のみ攻撃判定用コライダーを有効化する
/// </summary>
public class Check_NormalBox_Player_Tanabe : Check_MPlayerBase_Tanabe
{
    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private float m_attackDuration = 0.5f;
    private bool m_isAttacking = false;

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_A);
        if (m_attackCollider != null)
            m_attackCollider.enabled = false;
    }

    public override void Update()
    {
        base.Update();
        if(m_isAttacking) { return; }
    }

    protected override void OnAttackInput()
    {
        if (!m_isAttacking)
            StartCoroutine(AttackCoroutine());
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

        float elapsed = 0f;
        float duration = m_attackDuration;
        float rotationAngle = 90f; // 倒れる角度を設定
        Vector3 _eulerAngle = transform.rotation.eulerAngles;
        Quaternion startRot = Quaternion.Euler(0f, _eulerAngle.y, 0f);
        Quaternion targetRot = Quaternion.Euler(rotationAngle, _eulerAngle.y, 0f);
        //Quaternion targetRot = startRot * Quaternion.Euler(rotationAngle, 0f, 0f);

        if (m_attackCollider != null)
            m_attackCollider.enabled = true;

        // 倒れる動作
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRot;

        // 攻撃判定をオフ
        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        // 元に戻る動作（同じ時間で戻す）
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(targetRot, startRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = startRot;

        m_isAttacking = false;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking || !isServer) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
            Attack(target);
    }
}
