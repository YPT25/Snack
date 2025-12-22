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

    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private Collider m_hitCollider;
    [SerializeField] private float attackDuration = 0.5f;

    // 多段ヒット対策
    private HashSet<CharacterBase> hitTargets = new HashSet<CharacterBase>();

    public override void Start()
    {
        base.Start();

        if (isServer)
            m_waypoints = FindWayPoints("NormalWayPoint");

        if (m_attackCollider)
            m_attackCollider.enabled = false;

        if (modelRoot == null)
            Debug.LogError("modelRoot に kabeteki を設定してください。NPC");
    }


    // =====================================================
    // 倒れる攻撃
    // =====================================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;
        m_isAttacking = true;

        // 多段ヒット防止
        hitTargets.Clear();

        // 攻撃中は移動不能
        float originalSpeed = GetMoveSpeed();
        SetMoveSpeed(0);

        float elapsed = 0f;
        float duration = attackDuration;

        // Pivot 計算
        Vector3 pivotWorld = modelRoot.TransformPoint(pivotOffset);

        // 回転開始前の状態
        Quaternion startRot = modelRoot.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(90, 0, 0f);

        // コライダー ON
        if (m_attackCollider)
        {
            m_attackCollider.enabled = true;
            if (m_hitCollider) m_hitCollider.enabled = false;
        }

        // ============================
        // 前倒しの回転
        // ============================
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Quaternion rot = Quaternion.Slerp(startRot, endRot, t);

            // Pivot を中心に回転させる
            modelRoot.rotation = rot;
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        modelRoot.rotation = endRot;
        modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

        // コライダー OFF
        if (m_attackCollider)
        {
            m_attackCollider.enabled = false;
            if (m_hitCollider) m_hitCollider.enabled = true;
        }

        // ============================
        // 元の姿勢に戻るアニメーション
        // ============================
        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Quaternion rot = Quaternion.Slerp(endRot, startRot, t);

            modelRoot.rotation = rot;
            modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        modelRoot.rotation = startRot;
        modelRoot.position = pivotWorld - (modelRoot.rotation * pivotOffset);

        // 移動復帰
        SetMoveSpeed(originalSpeed);
        m_isAttacking = false;
    }


    // =====================================================
    // 多段ヒットなし攻撃
    // =====================================================
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target.GetCharacterType() == CharacterType.ENEMY_TYPE) return;

        // 多段ヒット防止
        if (hitTargets.Contains(target)) return;
        hitTargets.Add(target);

        Attack(target);
    }
}