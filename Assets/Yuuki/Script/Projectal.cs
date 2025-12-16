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
    [SyncVar] private NetworkIdentity ownerNetId;

    [SerializeField] private float lifeTime = 3f;

    [SerializeField] private float damage = 10.0f;

    // ======================================
    // 初期化（Server）
    // ======================================
    [Server]
    public void Initialize(EnemyBase shooter, float power)
    {
        ownerNetId = shooter.netIdentity;
        damage = power;

        Invoke(nameof(DestroySelf), lifeTime);
    }

    // ======================================
    // 当たり判定（Server）
    // ======================================
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        EnemyBase target = other.GetComponent<EnemyBase>();
        if (target == null) return;

        // 自己ヒット防止
        if (target.netIdentity == ownerNetId) return;

        target.Damage(damage);

        DestroySelf();
    }

    // ======================================
    // 破棄
    // ======================================
    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}