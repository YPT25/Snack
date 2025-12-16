using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NormalBox_NPC : NPCBase
{
    [Header("倒れるモデル（kabeteki）")]
    [SerializeField] private Transform modelRoot;

    [Header("回転の中心（modelRoot ローカル）")]
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;

    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private Collider m_hitCollider;
    [SerializeField] private float attackDuration = 0.5f;

    // 多段ヒット防止
    private HashSet<CharacterBase> hitTargets = new HashSet<CharacterBase>();

    // ★ 攻撃アニメ同期用
    [SyncVar] private bool m_playAttackAnim = false;

    public override void Start()
    {
        base.Start();

        if (isServer)
            m_waypoints = FindWayPoints("NormalWayPoint");

        if (m_attackCollider)
            m_attackCollider.enabled = false;

        if (modelRoot == null)
            Debug.LogError("modelRoot が設定されていません（NormalBox_NPC）");
    }

    // =====================================================
    // 攻撃（Server）
    // =====================================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking)
            yield break;

        m_isAttacking = true;
        m_isMoving = false;
        m_playAttackAnim = true;

        hitTargets.Clear();

        // コライダー ON
        if (m_attackCollider)
        {
            m_attackCollider.enabled = true;
            if (m_hitCollider) m_hitCollider.enabled = false;
        }

        // ★ Client にアニメ開始通知
        RpcPlayAttackAnim();

        yield return new WaitForSeconds(attackDuration * 2f);

        // コライダー OFF
        if (m_attackCollider)
        {
            m_attackCollider.enabled = false;
            if (m_hitCollider) m_hitCollider.enabled = true;
        }

        m_playAttackAnim = false;
        m_isAttacking = false;
    }

    // =====================================================
    // Client：攻撃アニメ再生
    // =====================================================
    [ClientRpc]
    private void RpcPlayAttackAnim()
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(AttackAnimationCoroutine());
    }

    private IEnumerator AttackAnimationCoroutine()
    {
        Vector3 pivotWorld = modelRoot.TransformPoint(pivotOffset);

        Quaternion startRot = modelRoot.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(90f, 0f, 0f);

        float elapsed = 0f;

        // 倒れる
        while (elapsed < attackDuration)
        {
            float t = elapsed / attackDuration;
            modelRoot.rotation = Quaternion.Slerp(startRot, endRot, t);
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        modelRoot.rotation = endRot;
        modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

        // 戻る
        elapsed = 0f;
        while (elapsed < attackDuration)
        {
            float t = elapsed / attackDuration;
            modelRoot.rotation = Quaternion.Slerp(endRot, startRot, t);
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        modelRoot.rotation = startRot;
        modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);
    }

    // =====================================================
    // 攻撃判定（Serverのみ）
    // =====================================================
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking)
            return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null)
            return;

        if (target.GetCharacterType() == CharacterType.ENEMY_TYPE)
            return;

        if (hitTargets.Contains(target))
            return;

        hitTargets.Add(target);
        Attack(target);
    }
}