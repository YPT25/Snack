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

    [Header("取り付き中の追従（サーバー）")]
    [SerializeField] private bool followHeadRotation = true;

    [Header("クールタイム")]
    [SerializeField] private float attachCooldown = 1f;

    private bool canAttach = true;

    private CharacterBase attachedTarget;

    // ★親子付けしない方式：追従対象Transformを保持してサーバーで追従
    private Transform attachedHead;
    private bool isAttached;

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

        // 飛びつき（サーバーの Rigidbody 物理）
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
    // 取り付き中の追従（サーバーで毎FixedUpdate）
    // ======================================
    [ServerCallback]
    private void FixedUpdate()
    {
        if (!isAttached) return;
        if (attachedHead == null) return;

        // サーバーで位置を更新 → NetworkTransform に同期させる想定
        transform.position = attachedHead.position + attachedHead.rotation * attachOffset;
        if (followHeadRotation)
            transform.rotation = attachedHead.rotation;
    }

    // ======================================
    // 衝突 → 取り付き開始（サーバーのみ）
    // ======================================
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!canAttach) return;
        if (attachedTarget != null) return;

        // 子Collider対策：InParentで取る
        CharacterBase target = other.GetComponentInParent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        // 1人1体ロック（受ける側にDebuffLockを付ける）
        DebuffLock lockComp = target.GetComponent<DebuffLock>();
        if (lockComp == null)
            lockComp = target.gameObject.AddComponent<DebuffLock>();

        // 既に誰かが取り付いてたら中止
        if (!lockComp.TryLock(netId))
            return;

        // ここで無効化（クールタイムを効かせる）
        canAttach = false;

        StartCoroutine(AttachAndDebuff(target, lockComp));
    }

    // ======================================
    // 取り付き + デバフ（親子付けしない方式）
    // ======================================
    [Server]
    private IEnumerator AttachAndDebuff(CharacterBase target, DebuffLock lockComp)
    {
        attachedTarget = target;

        try
        {
            // ターゲット行動停止（あなたの既存設計を維持）
            target.SetIsMove(false);
            target.SetIsAttack(false);
            target.RpcSetIsMove(false);
            target.RpcSetIsAttack(false);

            // 自身物理停止（null安全）
            if (m_rb != null)
            {
                m_rb.isKinematic = true;
                m_rb.useGravity = false;
                m_rb.velocity = Vector3.zero;
                m_rb.angularVelocity = Vector3.zero;
            }

            // 吸着先（HeadPointが無ければ本体）
            Transform head = target.transform.Find("HeadPoint");
            if (head == null) head = target.transform;

            // 親子付けしない：追従対象を保持してFixedUpdateで追従
            attachedHead = head;
            isAttached = true;

            // 初期位置を即合わせ
            transform.position = head.position + head.rotation * attachOffset;
            if (followHeadRotation)
                transform.rotation = head.rotation;

            // デバフ時間中、一定間隔でダメージ
            float timer = 0f;
            while (timer < debuffDuration)
            {
                // ターゲットが消えた/死んだ等の保険
                if (attachedTarget == null) break;

                // ※基本はサーバーDamageだけでOK（演出が必要なら別途Rpcで）
                target.RpcDamage(damageAmount);

                timer += damageInterval;
                yield return new WaitForSeconds(damageInterval);
            }

            // デバフ解除（相手が存在する場合のみ）
            if (target != null)
            {
                target.SetIsMove(true);
                target.SetIsAttack(true);
                target.RpcSetIsMove(true);
                target.RpcSetIsAttack(true);
            }

            // 追従停止
            isAttached = false;
            attachedHead = null;

            // 自身の物理復帰 & 離脱吹っ飛ばし
            if (m_rb != null)
            {
                m_rb.isKinematic = false;
                m_rb.useGravity = true;

                // できればターゲットの向き基準で後ろに飛ばす
                Vector3 baseForward = (target != null) ? target.transform.forward : transform.forward;
                Vector3 pushDir = (-baseForward * 15f) + (Vector3.up * 13f);

                m_rb.AddForce(pushDir, ForceMode.Impulse);
            }
        }
        finally
        {
            // ロック解除(永ロック防止）
            if (lockComp != null)
                lockComp.Unlock(netId);

            attachedTarget = null;

            // 念のため追従情報もクリア
            isAttached = false;
            attachedHead = null;
        }

        // クールタイム
        yield return new WaitForSeconds(attachCooldown);
        canAttach = true;
    }
}