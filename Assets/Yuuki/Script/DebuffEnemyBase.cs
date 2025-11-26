using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebuffEnemyBase : EnemyBase
{
    [Header("デバフ設定")]
    // 行動不能時間
    [SerializeField] protected float debuffDuration = 2f;
    // 1秒ごとのDoT
    [SerializeField] protected float damagePerSecond = 5f;
    // 接触しやすくする用
    [SerializeField] protected float jumpForce = 5f;          

    protected Rigidbody rb;

    public override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    // 攻撃対象にヒットしたときサーバー側で実行
    [Server]
    protected void ApplyDebuff(CharacterBase target)
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
}
