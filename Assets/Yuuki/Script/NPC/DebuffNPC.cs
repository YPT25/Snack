using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebuffNPC : NPCBase
{
    [Header("デバフ設定")]
    // 行動不能時間
    [SerializeField] private float debuffDuration = 2f;
    // 1秒ごとのDoT
    [SerializeField] private float damagePerSecond = 5f;
    // 接触しやすくする用
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody rb;

    public override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    // 攻撃対象にヒットしたときサーバー側で実行
    [Server]
    void ApplyDebuff(CharacterBase target)
    {
        if (target == null)
        {
            return;
        }

        // HEROにのみ効果
        if (target.GetCharacterType() != CharacterType.HERO_TYPE)
        {
            return;
        }


        // 既存のSetIsMove / SetIsAttack を使って行動不能
        target.SetIsMove(false);
        target.SetIsAttack(false);

        target.RpcSetIsMove(false);
        target.RpcSetIsAttack(false);

        // 行動不能+持続ダメージのコルーチン
        StartCoroutine(DebuffCoroutine(target));
    }

    [Server]
    private IEnumerator DebuffCoroutine(CharacterBase target)
    {
        float timer = 0f;

        while (timer < debuffDuration && target != null)
        {
            target.Damage(damagePerSecond * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // デバフ終了 → 行動可能に戻す
        if (target != null)
        {
            target.SetIsMove(true);
            target.SetIsAttack(true);

            target.RpcSetIsMove(true);
            target.RpcSetIsAttack(true);
        }
    }

    [Server]
    protected override IEnumerator DoAttack()
    {
        if (m_isAttacking) yield break;
        m_isAttacking = true;

        // 簡易ジャンプ攻撃
        if (rb != null)
            rb.velocity = new Vector3(0, jumpForce, 0);

        yield return new WaitForSeconds(0.2f);

        m_isAttacking = false;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;

        // デバフ実行
        ApplyDebuff(target);
    }
}