using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NormalBox_NPC : NPCBase
{
    [Header("倒れるモデル（kabeteki を指定）")]
    [SerializeField] private Transform modelRoot;

    [Header("回転の中心（modelRoot のローカル座標）")]
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;

    [Header("倒れる角度（度） ※逆なら符号を反転")]
    [SerializeField] private float fallAngle = 90f; 

    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private Collider m_hitCollider;
    [SerializeField] private float attackDuration = 0.5f;

    // 多段ヒット対策
    private readonly HashSet<CharacterBase> hitTargets = new HashSet<CharacterBase>();

    public override void Start()
    {
        base.Start();

        if (isServer)
            m_waypoints = FindWayPoints("NormalWayPoint");

        // 攻撃判定だけON/OFFする
        if (m_attackCollider)
            m_attackCollider.enabled = false;

        // 本体コライダーは常に有効のまま（落下/消える対策）
        if (m_hitCollider)
            m_hitCollider.enabled = true;

        if (modelRoot == null)
            Debug.LogError("modelRoot に kabeteki を設定してください。NPC");
    }

    // =====================================================
    // 倒れる攻撃（ターゲット方向へ向いてから倒れる）
    // =====================================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;
        if (modelRoot == null) yield break;

        m_isAttacking = true;

        // 多段ヒット防止
        hitTargets.Clear();

        // ★攻撃開始時にターゲット方向へ向ける（Y軸のみ）
        if (m_target != null)
        {
            Vector3 to = (m_target.position - transform.position);
            to.y = 0f;

            if (to.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
        }

        // 攻撃中は移動不能（あなたの既存設計）
        float originalSpeed = GetMoveSpeed();
        SetMoveSpeed(0);

        float duration = Mathf.Max(0.01f, attackDuration);

        // 回転開始前の状態（ワールド）
        Quaternion startRot = modelRoot.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(fallAngle, 0f, 0f);

        // コライダーON（攻撃判定だけ）
        if (m_attackCollider)
            m_attackCollider.enabled = true;

        // ============================
        // 倒れるアニメーション
        // ============================
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Quaternion rot = Quaternion.Slerp(startRot, endRot, t);

            // ★pivotWorldは毎回計算（ズレ蓄積を抑える）
            Vector3 pivotWorld = modelRoot.TransformPoint(pivotOffset);

            modelRoot.rotation = rot;
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 最終位置を確定
        {
            Vector3 pivotWorld = modelRoot.TransformPoint(pivotOffset);
            modelRoot.rotation = endRot;
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);
        }

        // コライダーOFF
        if (m_attackCollider)
            m_attackCollider.enabled = false;

        // ============================
        // 元の姿勢に戻るアニメーション
        // ============================
        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Quaternion rot = Quaternion.Slerp(endRot, startRot, t);

            Vector3 pivotWorld = modelRoot.TransformPoint(pivotOffset);

            modelRoot.rotation = rot;
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 最終位置を確定
        {
            Vector3 pivotWorld = modelRoot.TransformPoint(pivotOffset);
            modelRoot.rotation = startRot;
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);
        }

        // 移動復帰
        SetMoveSpeed(originalSpeed);
        m_isAttacking = false;
    }

    // =====================================================
    // 多段ヒットなし攻撃（攻撃判定Triggerで当たったら1回だけ）
    // =====================================================
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() == CharacterType.ENEMY_TYPE) return;

        if (hitTargets.Contains(target)) return;
        hitTargets.Add(target);

        Attack(target);
    }
}