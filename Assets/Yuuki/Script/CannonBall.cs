using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CannonBox の弾丸
/// ・衝突した相手にダメージ
/// ・味方にも当たる（EnemyBase の m_canFriendlyFire に依存）
/// </summary>
public class CannonBall : MonoBehaviour
{
    [Header("ダメージ量")]
    public int damage = 20;

    private void OnCollisionEnter(Collision collision)
    {
        CharacterBase target = collision.gameObject.GetComponent<CharacterBase>();
        if (target != null)
        {
            target.Damage(damage);
            Debug.Log($"{target.name} が砲弾を受けた！ ダメージ:{damage}");
        }

        Destroy(gameObject); // 使い捨て
    }
}
