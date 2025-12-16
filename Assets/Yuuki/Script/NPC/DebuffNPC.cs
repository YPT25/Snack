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

    // =========================
    // 同期用
    // =========================
    [SyncVar] private bool m_isAttached = false;
    [SyncVar] private NetworkIdentity m_attachedTargetNetId;

    private bool canAttach = true;

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_D);
    }

    // ======================================
    // 攻撃（Server）
    // ======================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking || m_target == null)
            yield break;

        m_isAttacking = true;

        // NavMesh 停止
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();

        // 飛びつき（Serverのみ物理）
        if (m_rb != null)
        {
            m_rb.AddForce(
                Vector3.up * jumpForce + transform.forward * forwardForce,
                ForceMode.Impulse
            );
        }

        yield return new WaitForSeconds(0.2f);
        m_isAttacking = false;
    }

    // ======================================
    // 衝突 → 取り付き（Server）
    // ======================================
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!canAttach || m_isAttached)
            return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        StartCoroutine(AttachAndDebuff(target));
    }

    // ======================================
    // 取り付き + デバフ（Server）
    // ======================================
    [Server]
    private IEnumerator AttachAndDebuff(CharacterBase target)
    {
        canAttach = false;
        m_isAttached = true;
        m_attachedTargetNetId = target.netIdentity;

        // ターゲット行動停止
        target.SetIsMove(false);
        target.SetIsAttack(false);
        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        // 自身物理停止
        m_rb.isKinematic = true;
        m_rb.useGravity = false;

        // Client に見た目同期
        RpcAttach(target.netIdentity);

        float timer = 0f;
        while (timer < debuffDuration)
        {
            target.Damage(damageAmount);
            timer += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // デバフ解除
        target.SetIsMove(true);
        target.SetIsAttack(true);
        target.RpcSetIsMove(true);
        target.RpcSetIsAttack(true);

        // 離脱
        RpcDetach();

        m_rb.isKinematic = false;
        m_rb.useGravity = true;

        Vector3 pushDir =
            (-target.transform.forward * 15f) +
            (Vector3.up * 13f);

        m_rb.AddForce(pushDir, ForceMode.Impulse);

        m_isAttached = false;
        m_attachedTargetNetId = null;

        yield return new WaitForSeconds(1f);
        canAttach = true;
    }

    // ======================================
    // Client：取り付き再生
    // ======================================
    [ClientRpc]
    private void RpcAttach(NetworkIdentity targetNetId)
    {
        if (targetNetId == null) return;

        Transform head = targetNetId.transform.Find("HeadPoint");
        if (head == null) head = targetNetId.transform;

        transform.SetParent(head, false);
        transform.localPosition = attachOffset;
        transform.localRotation = Quaternion.identity;
    }

    // ======================================
    // Client：離脱再生
    // ======================================
    [ClientRpc]
    private void RpcDetach()
    {
        transform.SetParent(null);
    }
}