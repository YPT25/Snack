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

    // =========================
    // 同期用
    // =========================
    [SyncVar] private bool m_isAttacking = false;

    // 多段ヒット防止（Server）
    private HashSet<CharacterBase> hitTargets = new HashSet<CharacterBase>();

    // =========================
    // 初期化
    // =========================
    public override void Start()
    {
        base.Start();

        if (isServer)
            SetEnemyType(EnemyType.TYPE_A);

        if (m_attackCollider)
            m_attackCollider.enabled = false;

        if (modelRoot == null)
            Debug.LogError("modelRoot に kabeteki を割り当ててください。");
    }

    // =========================
    // 攻撃入力（Client）
    // =========================
    protected override void OnAttackInput()
    {
        if (!isLocalPlayer || m_isAttacking)
            return;

        CmdRequestAttack();
    }

    // =========================
    // 攻撃開始要求（Server）
    // =========================
    [Command]
    private void CmdRequestAttack()
    {
        if (m_isAttacking) return;

        m_isAttacking = true;
        hitTargets.Clear();

        RpcPlayAttackAnimation();
        StartCoroutine(AttackDurationRoutine());
    }

    // =========================
    // 攻撃アニメーション（Client）
    // =========================
    [ClientRpc]
    private void RpcPlayAttackAnimation()
    {
        StartCoroutine(AttackAnimation());
    }

    private IEnumerator AttackAnimation()
    {
        iscanMove = false;

        Transform pivot = attackPivot != null ? attackPivot : modelRoot;

        Quaternion startRot = pivot.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(90f, 0f, 0f);

        if (m_attackCollider)
            m_attackCollider.enabled = true;

        float elapsed = 0f;
        while (elapsed < m_attackDuration)
        {
            pivot.localRotation =
                Quaternion.Slerp(startRot, targetRot, elapsed / m_attackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        pivot.localRotation = targetRot;

        if (m_attackCollider)
            m_attackCollider.enabled = false;

        elapsed = 0f;
        while (elapsed < m_attackDuration)
        {
            pivot.localRotation =
                Quaternion.Slerp(targetRot, startRot, elapsed / m_attackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        pivot.localRotation = startRot;

        iscanMove = true;
    }

    // =========================
    // 攻撃終了（Server）
    // =========================
    [Server]
    private IEnumerator AttackDurationRoutine()
    {
        yield return new WaitForSeconds(m_attackDuration * 2f);
        m_isAttacking = false;
    }

    // =========================
    // 当たり判定（Server）
    // =========================
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking)
            return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        if (target == this) return;

        if (hitTargets.Contains(target))
            return;

        hitTargets.Add(target);
        Attack(target);
    }
}