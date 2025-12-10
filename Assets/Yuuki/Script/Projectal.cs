using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 弾丸の基本処理
/// ・サーバーでのみ当たり判定処理を行う
/// ・発射元プレイヤー情報を保持し、自己ヒット防止
/// </summary>
public class Projectile : NetworkBehaviour
{
    [SerializeField]
    private float damage = 10f;

    private EnemyBase owner;   // ← EnemyBase に変更！

    [SerializeField] private float lifeTime = 3f;

    public void Initialize(EnemyBase shooter, float power)
    {
        owner = shooter;
        damage = power;

        Invoke(nameof(DestroySelf), lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        EnemyBase target = other.GetComponent<EnemyBase>();
        if (target == null) return;

        // 自己ヒット防止
        if (target == owner) return;

        // 攻撃
        owner.Attack(target);

        DestroySelf();
    }

    private void DestroySelf()
    {
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }
}
