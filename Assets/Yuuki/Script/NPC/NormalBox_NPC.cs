using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NormalBox_NPC : NPCBase
{
    [Header("倒れるモデル（kabeteki を指定）")]
    [SerializeField] private Transform modelRoot;

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

    // ======================================
    // ● プレイヤー版の前倒し攻撃を移植（transformは回転させない）
    // ======================================
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;
        m_isAttacking = true;

        // 多段ヒットを防ぐ
        hitTargets.Clear();

        // ↓ 攻撃中は移動しない
        Vector3 stopVelocity = Vector3.zero;
        float originalSpeed = GetMoveSpeed();
        SetMoveSpeed(0);

        float elapsed = 0f;
        float duration = attackDuration;

        // モデルだけ倒す（Playerと同じ）
        Quaternion startRot = modelRoot.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, -90f);

        // コライダーON
        if (m_attackCollider)
        {
            m_attackCollider.enabled = true;
            m_hitCollider.enabled = false;
        }

        // 倒れるアニメ
        while (elapsed < duration)
        {
            modelRoot.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        modelRoot.localRotation = targetRot;

        // OFF
        if (m_attackCollider)
        {
            m_attackCollider.enabled = false;
            m_hitCollider.enabled = true;
        }

        // 元に戻す
        elapsed = 0f;
        while (elapsed < duration)
        {
            modelRoot.localRotation = Quaternion.Slerp(targetRot, startRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        modelRoot.localRotation = startRot;

        // 移動速度を戻す
        SetMoveSpeed(originalSpeed);

        m_isAttacking = false;
    }

    // ======================================
    // ● 多段ヒットなし攻撃
    // ======================================
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