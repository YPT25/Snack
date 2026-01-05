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

    [Header("攻撃設定")]
    [SerializeField] private Collider attackCollider;
    [SerializeField] private float attackDuration = 0.4f;

    [Header("倒れる中心（省略可）")]
    [SerializeField] private Transform attackPivot;
    private Quaternion baseLocalRotation;
    // ===== 状態 =====
    [SyncVar] private bool isAttacking = false;

    // 多段ヒット防止
    private readonly HashSet<CharacterBase> hitTargets = new HashSet<CharacterBase>();

    public override void Start()
    {
        base.Start();

        SetEnemyType(EnemyType.TYPE_A);

        Transform pivot = attackPivot != null ? attackPivot : modelRoot;
        if (pivot != null)
            baseLocalRotation = pivot.localRotation;

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (modelRoot == null)
            Debug.LogError("NormalBox_Player: modelRoot が設定されていません");
    }

    // =============================
    // 攻撃入力（Serverに届く）
    // =============================
    protected override void OnAttackInput()
    {
        if (!isServer) return;
        if (isAttacking) return;

        isAttacking = true;
        hitTargets.Clear();

        // 自分（操作プレイヤー）用
        TargetStartAttack(connectionToClient);

        // 他人に見せる用
        RpcPlayAttackVisual();

        StartCoroutine(ServerAttackRoutine());
    }

    // =============================
    // Server 側の攻撃管理
    // =============================
    private IEnumerator ServerAttackRoutine()
    {
        // 判定ON
        if (attackCollider != null)
            attackCollider.enabled = true;

        yield return new WaitForSeconds(attackDuration);

        if (attackCollider != null)
            attackCollider.enabled = false;

        isAttacking = false;
    }

    // =============================
    // 自分用（移動停止あり）
    // =============================
    [TargetRpc]
    private void TargetStartAttack(NetworkConnection target)
    {
        if (!isLocalPlayer) return;
        StartCoroutine(AttackVisual_Local());
    }

    private IEnumerator AttackVisual_Local()
    {
        iscanMove = false;

        yield return AttackVisualCore();

        iscanMove = true;
    }

    // =============================
    // 他人用（見るだけ）
    // =============================
    [ClientRpc]
    private void RpcPlayAttackVisual()
    {
        if (isLocalPlayer) return;
        StartCoroutine(AttackVisual_Remote());
    }

    private IEnumerator AttackVisual_Remote()
    {
        yield return AttackVisualCore();
    }

    // =============================
    // 共通演出コア
    // =============================
    private IEnumerator AttackVisualCore()
    {
        Transform pivot = attackPivot != null ? attackPivot : modelRoot;
        if (pivot == null) yield break;

        Quaternion startRot = baseLocalRotation;
        Quaternion endRot = baseLocalRotation * Quaternion.Euler(90f, 0f, 0f);

        // ★ 念のため強制リセット
        pivot.localRotation = startRot;

        float t = 0f;

        // 倒れる
        while (t < attackDuration)
        {
            pivot.localRotation = Quaternion.Slerp(startRot, endRot, t / attackDuration);
            t += Time.deltaTime;
            yield return null;
        }

        pivot.localRotation = endRot;

        // 戻る
        t = 0f;
        while (t < attackDuration)
        {
            pivot.localRotation = Quaternion.Slerp(endRot, startRot, t / attackDuration);
            t += Time.deltaTime;
            yield return null;
        }

        pivot.localRotation = startRot;
    }

    // =============================
    // 攻撃判定（Server only）
    // =============================
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null || target == this) return;

        if (hitTargets.Contains(target)) return;

        hitTargets.Add(target);
        Attack(target);
    }
}