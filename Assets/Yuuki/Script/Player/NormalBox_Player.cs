using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;
/// <summary>
/// NormalBox のプレイヤー操作用クラス
/// ・MPlayerBase を継承
/// ・左クリックで「前方に倒れる攻撃」を実行
/// ・攻撃中のみ攻撃判定用コライダーを有効化する
/// </summary>
public class NormalBox_Player : MPlayerBase
{
    [Header("倒れるモデル（kabeteki）")]
    [SerializeField] private Transform modelRoot;

    [SerializeField] private Collider m_attackCollider;
    [SerializeField] private Collider m_hitCollider;
    [SerializeField] private float m_attackDuration = 0.5f;
    [Header("倒れる中心")]
    [SerializeField] private Transform attackPivot;

    private bool m_isAttacking = false;
    // 多段ヒット防止用
    private HashSet<CharacterBase> hitTargets = new HashSet<CharacterBase>();

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_A);

        if (m_attackCollider != null)
            m_attackCollider.enabled = false;

        if (modelRoot == null)
        {
            Debug.LogError("modelRoot に kabeteki を割り当ててください。");
        }
    }

    protected override void OnAttackInput()
    {
        if (!m_isAttacking)
            StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;
        hitTargets.Clear();

        float elapsed = 0f;
        float duration = m_attackDuration;

        iscanMove = false;

        // pivotが無ければmodelRootをpivotとして扱う
        Transform pivot = attackPivot != null ? attackPivot : modelRoot;

        Quaternion startRot = pivot.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(90f, 0f, 0f);

        m_attackCollider.enabled = true;

        // ===== 倒れるアニメ =====
        while (elapsed < duration)
        {
            pivot.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        pivot.localRotation = targetRot;

        // 攻撃オフ
        m_attackCollider.enabled = false;

        // 元に戻るアニメ
        elapsed = 0f;
        while (elapsed < duration)
        {
            pivot.localRotation = Quaternion.Slerp(targetRot, startRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        pivot.localRotation = startRot;

        iscanMove = true;
        m_isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking || !isServer) return;

        CharacterBase target = other.GetComponent<CharacterBase>();

        // ★多段ヒット防止
        if (target != null && target != this)
        {
            if (!hitTargets.Contains(target))
            {
                hitTargets.Add(target);   // 一度当たった相手は記録
                Attack(target);           // 攻撃実行
            }
        }
    }
}