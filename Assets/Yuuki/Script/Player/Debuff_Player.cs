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

    [Header("取り付き判定（クライアント側で当たったら報告する用）")]
    [SerializeField] private Collider attackTrigger; // isTrigger推奨（未設定なら自身のColliderを使う）

    [Header("飛びつき判定受付時間")]
    [SerializeField] private float attachWindow = 0.35f;

    // ===== ジャンプ制御（追加）=====
    [Header("ジャンプ制御")]
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundMask = ~0; // とりあえず全部。後でGroundだけに絞るのが理想
    [SerializeField] private float fastFallForce = 12f; // 急降下の強さ

    private bool isGroundedLocal = false;
    private bool jumpLockedLocal = false; // ジャンプ中ロック（着地で解除）

    // ローカル：飛びつき中か（当たり報告を受け付ける窓）
    private bool isLeapLocal = false;
    private float leapEndLocalTime = 0f;
    private bool hitSentLocal = false;

    // サーバー：現在取り付き中か
    [SyncVar] private bool isAttachedServer = false;
    [SyncVar] private bool canAttachServer = true;

    // サーバー：今のターゲット
    private CharacterBase attachedTarget = null;
    private Coroutine serverDebuffRoutine;

    private Rigidbody rb;
    private Behaviour networkTransformComp; // NetworkTransformReliable（取り付き中に止める用）

    public override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody>();

        // サーバーだけでタイプ決定
        if (isServer)
            SetEnemyType(EnemyType.TYPE_D);

        if (attackTrigger == null)
            attackTrigger = GetComponent<Collider>();

        // Mirror最新版：NetworkTransformReliableのみ想定
        networkTransformComp = GetComponent<Mirror.NetworkTransformReliable>();
    }

    // MPlayerBase経由のOnAttackInputはサーバーで呼ばれるので、ここでは使わない
    protected override void OnAttackInput() { }

    public override void Update()
    {

        if (isLocalPlayer && LegacyInputHelper.GetAttackDown())
        {
            Debug.Log($"[Debuff] grounded={isGroundedLocal} jumpLocked={jumpLockedLocal} kin={rb.isKinematic} grav={rb.useGravity} vY={rb.velocity.y}");
        }

        base.Update();

        if (!isLocalPlayer) return;

        // 地面チェック更新
        isGroundedLocal = CheckGrounded();

        // 着地したらジャンプロック解除
        if (isGroundedLocal)
            jumpLockedLocal = false;

        // 攻撃ボタン：地上ならジャンプ、空中なら急降下
        if (LegacyInputHelper.GetAttackDown())
        {
            if (!isGroundedLocal)
            {
                DoFastFallLocal();
            }
            else if (!jumpLockedLocal)
            {
                StartLeapLocal();
            }
        }

        // 飛びつき受付の時間切れ
        if (isLeapLocal && Time.time >= leapEndLocalTime)
            isLeapLocal = false;
    }

    // ===== 地面判定（SphereCastで安定）=====
    private bool CheckGrounded()
    {
        if (rb == null) return false;

        // ちょい上から広めの球でチェック（段差や原点ズレに強い）
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        float radius = 0.45f;
        float dist = 0.8f; // ← groundCheckDistanceより強制で広めにして原因切り分け

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out _,
            dist,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

    }

    // ===== ジャンプ（飛びつき）=====
    private void StartLeapLocal()
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        if (rb == null) return;

        // 取り付き中はジャンプしない（見た目破綻しやすいので）
        if (isAttachedServer) return;

        // ジャンプ開始 = ロック
        jumpLockedLocal = true;

        isLeapLocal = true;
        hitSentLocal = false;
        leapEndLocalTime = Time.time + attachWindow;

        // 上向き速度が残ってると連打っぽく見えるので、上方向だけ整える
        Vector3 v = rb.velocity;
        if (v.y > 0f) v.y = 0f;
        rb.velocity = v;

        rb.AddForce(Vector3.up * jumpForce + transform.forward * forwardForce, ForceMode.Impulse);
    }

    // ===== 急降下（空中で攻撃）=====
    private void DoFastFallLocal()
    {
        if (rb == null) return;

        // 上昇中なら上向きを潰す
        Vector3 v = rb.velocity;
        if (v.y > 0f) v.y = 0f;
        rb.velocity = v;

        rb.AddForce(Vector3.down * fastFallForce, ForceMode.Impulse);
    }

    // =========================================================
    // 取り付き（現時点で動かなくてもOK：相手設定揃ったら効く）
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        // ローカルで当たり検出 → サーバーへ申請（A方式）
        if (!isLocalPlayer) return;
        if (!isLeapLocal) return;
        if (hitSentLocal) return;

        NetworkIdentity ni = other.GetComponentInParent<NetworkIdentity>();
        if (ni == null) return;
        if (ni == netIdentity) return;

        CharacterBase target = ni.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target == this) return;

        // 必要ならここで対象フィルタ（例：ヒーローだけ）
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        hitSentLocal = true;
        CmdRequestAttach(ni);
    }

    [Command]
    private void CmdRequestAttach(NetworkIdentity targetNi)
    {
        if (targetNi == null) return;
        if (!canAttachServer) return;
        if (isAttachedServer) return;

        CharacterBase target = targetNi.GetComponent<CharacterBase>();
        if (target == null) return;

        // ゆる距離検証
        float dist = Vector3.Distance(transform.position, target.transform.position);
        if (dist > 2.0f) return;

        if (serverDebuffRoutine != null)
            StopCoroutine(serverDebuffRoutine);

        serverDebuffRoutine = StartCoroutine(ServerAttachAndDebuff(target));
    }

    [Server]
    private IEnumerator ServerAttachAndDebuff(CharacterBase target)
    {
        canAttachServer = false;
        isAttachedServer = true;
        attachedTarget = target;

        // 相手の行動不可（サーバー確定）
        target.SetIsMove(false);
        target.SetIsAttack(false);
        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        // 見た目の取り付き（全員）
        RpcAttachVisual(target.netIdentity);

        float timer = 0f;
        while (timer < debuffDuration && attachedTarget != null)
        {
            // DoT（サーバー）
            target.Damage(damageAmount);

            timer += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // 解除
        if (attachedTarget != null)
        {
            attachedTarget.SetIsMove(true);
            attachedTarget.SetIsAttack(true);
            attachedTarget.RpcSetIsMove(true);
            attachedTarget.RpcSetIsAttack(true);
        }

        RpcDetachVisual(target.netIdentity);

        attachedTarget = null;
        isAttachedServer = false;

        // 小さめクールタイム
        yield return new WaitForSeconds(1f);
        canAttachServer = true;

        serverDebuffRoutine = null;
    }

    [ClientRpc]
    private void RpcAttachVisual(NetworkIdentity targetNi)
    {
        if (targetNi == null) return;

        // NetworkTransform補正と喧嘩しやすいので取り付き中は止める（存在するなら）
        if (networkTransformComp != null)
            networkTransformComp.enabled = false;

        // 自分の物理停止
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Transform head = targetNi.transform.Find("HeadPoint");
        if (head == null) head = targetNi.transform;

        transform.SetParent(head, worldPositionStays: false);
        transform.localPosition = attachOffset;
        transform.localRotation = Quaternion.identity;

        // 本人の移動入力止め
        if (isLocalPlayer) iscanMove = false;
    }

    [ClientRpc]
    private void RpcDetachVisual(NetworkIdentity targetNi)
    {
        transform.SetParent(null);

        // 物理復帰
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // NetworkTransform復帰
        if (networkTransformComp != null)
            networkTransformComp.enabled = true;

        // 本人の移動復帰
        if (isLocalPlayer) iscanMove = true;

        // 離脱の吹き飛ばし（見た目用：後で必要ならサーバー確定に寄せる）
        if (rb != null && targetNi != null)
        {
            Vector3 pushDir = (-targetNi.transform.forward * 15f) + (Vector3.up * 13f);
            rb.AddForce(pushDir, ForceMode.Impulse);
        }
    }
}