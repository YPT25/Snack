using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebuffNPC : NPCBase
{
    [Header("デバフ設定")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float forwardForce = 3f;

    [SerializeField] private float debuffDuration = 3f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageAmount = 5f;

    [Header("取り付き位置")]
    [SerializeField] private Vector3 attachOffset = new Vector3(0, 0.5f, 0);

    private bool canAttach = true;
    private CharacterBase attachedTarget;

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_D);
    }

    // ======================================
    // 攻撃処理（NPCBase から呼ばれる）
    // ======================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;
        if (m_target == null) yield break;

        m_isAttacking = true;

        // NavMesh 停止
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();

        // 飛びつき
        if (m_rb != null)
        {
            m_rb.AddForce(
                Vector3.up * jumpForce + transform.forward * forwardForce,
                ForceMode.Impulse
            );
        }

        // 衝突受付まで少し待つ
        yield return new WaitForSeconds(0.2f);

        m_isAttacking = false;
    }

    // ======================================
    // 衝突 → 取り付き
    // ======================================
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!canAttach) return;
        if (attachedTarget != null) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        StartCoroutine(AttachAndDebuff(target));
    }

    // ======================================
    // 取り付き + デバフ
    // ======================================
    private IEnumerator AttachAndDebuff(CharacterBase target)
    {
        attachedTarget = target;

        // ターゲット行動停止
        target.SetIsMove(false);
        target.SetIsAttack(false);
        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        // 自身物理停止
        m_rb.isKinematic = true;
        m_rb.useGravity = false;

        // 頭へ吸着
        Transform head = target.transform.Find("HeadPoint");
        if (head == null) head = target.transform;

        transform.SetParent(head, false);
        transform.localPosition = attachOffset;
        transform.localRotation = Quaternion.identity;

        float timer = 0f;
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

        // 離脱
        transform.SetParent(null);
        m_rb.isKinematic = false;
        m_rb.useGravity = true;

        Vector3 pushDir =
            (-target.transform.forward * 15f) +
            (Vector3.up * 13f);

        m_rb.AddForce(pushDir, ForceMode.Impulse);

        attachedTarget = null;

        // クールタイム
        yield return new WaitForSeconds(1f);
        canAttach = true;
    }
}