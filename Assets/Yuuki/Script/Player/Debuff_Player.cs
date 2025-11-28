using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Debuff_Player : MPlayerBase
{
    [Header("デバフ設定")]
    [SerializeField] private float debuffDuration = 3f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageAmount = 5f;

    private bool isDebuffAttacking = false;
    protected override void OnAttackInput()
    {
        if (!isDebuffAttacking)
            StartCoroutine(DebuffAttack());
    }

    private IEnumerator DebuffAttack()
    {
        isDebuffAttacking = true;

        // ジャンプ
        if (m_rb != null)
            m_rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        // 着地前に当たり判定有効化
        yield return new WaitForSeconds(0.5f); 

        isDebuffAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        CharacterBase target = other.GetComponent<CharacterBase>();

        if (target == null) return;
        if (target == this) return;
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        // デバフ実行
        ApplyDebuff(target);
    }

    // -----------------------------
    // デバフ実行（ターゲットのみ）
    // -----------------------------
    private void ApplyDebuff(CharacterBase target)
    {
        // 行動停止
        target.SetIsMove(false);
        target.SetIsAttack(false);
        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        // DOT
        StartCoroutine(DoDamageOverTime(target));
        // 復帰
        StartCoroutine(RecoverAfterDelay(target));
    }

    private IEnumerator DoDamageOverTime(CharacterBase target)
    {
        float timer = 0f;
        while (timer < debuffDuration)
        {
            target.Damage(damageAmount);
            target.RpcDamage(damageAmount);
            yield return new WaitForSeconds(damageInterval);
            timer += damageInterval;
        }
    }

    private IEnumerator RecoverAfterDelay(CharacterBase target)
    {
        yield return new WaitForSeconds(debuffDuration);

        target.SetIsMove(true);
        target.SetIsAttack(true);
        target.RpcSetIsMove(true);
        target.RpcSetIsAttack(true);
    }
}

