using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Debuff_Player : MPlayerBase
{
    [Header("ジャンプ設定")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float forwardForce = 3f;

    [Header("デバフ設定")]
    [SerializeField] private float debuffDuration = 3f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageAmount = 5f;

    [Header("取り付き位置")]
    [SerializeField] private Vector3 attachOffset = new Vector3(0, 0.5f, 0);

    // =========================
    // 状態
    // =========================
    [SyncVar] private bool isAttached = false;

    private CharacterBase attachedTarget;
    private bool canAttach = true;

    // =========================
    // 初期化
    // =========================
    public override void Start()
    {
        base.Start();

        if (isServer)
            SetEnemyType(EnemyType.TYPE_D);
    }

    // =========================
    // 攻撃入力（Client）
    // =========================
    protected override void OnAttackInput()
    {
        if (!isLocalPlayer) return;
        CmdStartDebuffJump();
    }

    // =========================
    // ジャンプ開始（Server）
    // =========================
    [Command]
    private void CmdStartDebuffJump()
    {
        StartJump();
    }

    [Server]
    private void StartJump()
    {
        if (m_rb == null) return;

        m_rb.AddForce(
            Vector3.up * jumpForce + transform.forward * forwardForce,
            ForceMode.Impulse
        );
    }

    // =========================
    // 接触検出（Client）
    // =========================
    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;
        if (!canAttach || isAttached) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target == this) return;

        CmdTryAttach(target.netIdentity);
    }

    // =========================
    // 取り付き判定（Server）
    // =========================
    [Command]
    private void CmdTryAttach(NetworkIdentity targetNet)
    {
        if (!canAttach || isAttached) return;

        CharacterBase target = targetNet.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        StartCoroutine(AttachAndDebuff(target));
    }

    // =========================
    // デバフ処理（Server）
    // =========================
    [Server]
    private IEnumerator AttachAndDebuff(CharacterBase target)
    {
        isAttached = true;
        attachedTarget = target;

        // 相手拘束
        target.SetIsMove(false);
        target.SetIsAttack(false);
        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        RpcAttach(target.netIdentity);

        float timer = 0f;
        while (timer < debuffDuration)
        {
            target.Damage(damageAmount);
            timer += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // 解放
        target.SetIsMove(true);
        target.SetIsAttack(true);
        target.RpcSetIsMove(true);
        target.RpcSetIsAttack(true);

        RpcDetach(target.transform.forward);

        attachedTarget = null;
        isAttached = false;

        StartCoroutine(AttachCooldown());
    }

    // =========================
    // 見た目：取り付き（全Client）
    // =========================
    [ClientRpc]
    private void RpcAttach(NetworkIdentity targetNet)
    {
        Transform head = targetNet.transform.Find("HeadPoint");
        if (head == null)
            head = targetNet.transform;

        transform.SetParent(head, false);
        transform.localPosition = attachOffset;
        transform.localRotation = Quaternion.identity;

        if (m_rb != null)
        {
            m_rb.isKinematic = true;
            m_rb.useGravity = false;
        }
    }

    // =========================
    // 見た目：離脱（全Client）
    // =========================
    [ClientRpc]
    private void RpcDetach(Vector3 targetForward)
    {
        transform.SetParent(null);

        if (m_rb != null)
        {
            m_rb.isKinematic = false;
            m_rb.useGravity = true;

            Vector3 pushDir =
                (-targetForward * 15f) + Vector3.up * 13f;

            m_rb.AddForce(pushDir, ForceMode.Impulse);
        }
    }

    // =========================
    // 再取り付き制限
    // =========================
    private IEnumerator AttachCooldown()
    {
        canAttach = false;
        yield return new WaitForSeconds(1f);
        canAttach = true;
    }
}