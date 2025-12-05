using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebuffNPC : NPCBase
{
    [Header("デバフ設定")]
    [SerializeField] private float debuffDuration = 3f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageAmount = 5f;

    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;

        m_isAttacking = true;

        // ジャンプ
        if (m_rb != null)
            m_rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);

        yield return new WaitForSeconds(0.4f);

        m_isAttacking = false;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();

        if (target == null) return;
        if (target.GetCharacterType() != CharacterType.HERO_TYPE) return;

        ApplyDebuff(target);
    }

    // -----------------------------
    // デバフ本体
    // -----------------------------
    private void ApplyDebuff(CharacterBase target)
    {
        target.SetIsMove(false);
        target.SetIsAttack(false);
        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        StartCoroutine(DoDamageOverTime(target));
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