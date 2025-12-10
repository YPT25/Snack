using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Debuff_Player : MPlayerBase
{
    [Header("デバフ設定")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float forwardForce = 3f;

    [SerializeField] private float debuffDuration = 3f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageAmount = 5f;

    [Header("取り付き位置（相手のHeadPointへ追従）")]
    [SerializeField] private Vector3 attachOffset = new Vector3(0, 0.5f, 0);

    private bool isDebuffAttacking = false;
    private CharacterBase attachedTarget = null;

    // 再取り付きクールタイム用フラグ
    private bool canAttach = true;

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_D);
    }

    // ============================
    //   攻撃入力 → 飛びつき開始
    // ============================
    protected override void OnAttackInput()
    {
        if (isDebuffAttacking) return;
        StartCoroutine(StartDebuffAttack());
    }

    private IEnumerator StartDebuffAttack()
    {
        isDebuffAttacking = true;

        // ジャンプ＋前方突進
        if (m_rb != null)
        {
            m_rb.AddForce(Vector3.up * jumpForce + transform.forward * forwardForce, ForceMode.Impulse);
        }

        // 少し待って衝突受付
        yield return new WaitForSeconds(0.2f);

        isDebuffAttacking = false;
    }

    // ============================
    //     衝突 → とりつき開始
    // ============================
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        // クールタイム中は取り付かない
        if (!canAttach) return;

        // すでに取り付き済み
        if (attachedTarget != null) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target == this) return;
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        StartCoroutine(AttachAndDebuff(target));
    }

    // ============================
    //    頭に吸着 → デバフ実施
    // ============================
    private IEnumerator AttachAndDebuff(CharacterBase target)
    {
        attachedTarget = target;

        // 相手の行動を停止
        target.SetIsMove(false);
        target.SetIsAttack(false);
        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        // 自分の物理を停止してめり込み防止
        m_rb.isKinematic = true;
        m_rb.useGravity = false;

        // 相手の頭に親子付け
        Transform head = target.transform.Find("HeadPoint");
        if (head == null) head = target.transform;

        transform.SetParent(head, worldPositionStays: false);
        transform.localPosition = attachOffset;
        transform.localRotation = Quaternion.identity;

        // DOT開始
        float timer = 0;
        while (timer < debuffDuration)
        {
            target.Damage(damageAmount);
            target.RpcDamage(damageAmount);

            timer += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // デバフ解除
        target.SetIsMove(true);
        target.SetIsAttack(true);
        target.RpcSetIsMove(true);
        target.RpcSetIsAttack(true);

        // 離脱処理
        transform.SetParent(null);
        m_rb.isKinematic = false;
        m_rb.useGravity = true;

        // 離脱時に大きめに後方へ吹き飛ばす（押し出し強化）
        Vector3 pushDir = (-target.transform.forward * 15f) + (Vector3.up * 13f);
        m_rb.AddForce(pushDir, ForceMode.Impulse);

        // 取り付き解除
        attachedTarget = null;

        // 1秒の取り付き禁止クールタイム発動
        StartCoroutine(AttachCooldown());
    }

    // ============================
    //     再取り付きを禁止する
    // ============================
    private IEnumerator AttachCooldown()
    {
        canAttach = false;
        yield return new WaitForSeconds(1f); // ← 必要に応じて調整可能
        canAttach = true;
    }
}