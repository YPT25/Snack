using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyBase : CharacterBase
{
    // ＜列挙型＞ーーーーーーーーーーーーーーーーーーーーーーー
    public enum EnemyType
    {
        TYPE_NULL,
        TYPE_A,
        TYPE_B,
        TYPE_C,
        TYPE_D,
    }

    // ＜パラメータ＞ーーーーーーーーーーーーーーーーーーーーー
    [Header("味方への攻撃を許可するか？")]
    [SerializeField] private bool m_canFriendlyFire = false;

    [Header("エネミータイプ(各プレハブで設定)")]
    [SyncVar][SerializeField] private EnemyType m_enemyType = EnemyType.TYPE_A;

    protected CharacterType m_enemyCharacterType = CharacterType.ENEMY_TYPE;

    //ダメージ演出用
    private DamegeEffect_Mokurin m_damagePerformance;



    public virtual void Start()
    {
        if (isServer)
            SetCharacterType(m_enemyCharacterType);
        m_damagePerformance = GetComponent<DamegeEffect_Mokurin>();

    }

    /// <summary>
    /// 攻撃処理（サーバーで実行）
    /// </summary>
    [Server]
    public virtual void Attack(CharacterBase target)
    {
        if (target == null) return;

        if (!m_canFriendlyFire && target.GetCharacterType() == m_enemyCharacterType)
        {
            Debug.Log($"{name} → 味方への攻撃は禁止");
            return;
        }

        target.RpcDamage(GetPower());
        Debug.Log($"{name} が {target.name} に攻撃！ ダメージ:{GetPower()}");
    }

    public override void Damage(float _damage)
    {
        base.Damage(_damage);

        // 全員に赤点滅を通知
        RpcPlayDamageEffect();
    }

    [ClientRpc]
    private void RpcPlayDamageEffect()
    {
        m_damagePerformance?.Damage();
    }

    /// <summary>
    /// 共通の死亡処理
    /// </summary>
    [Server]
    public virtual void Die()
    {
        Debug.Log($"{name} は倒れた！");

        NetworkServer.Destroy(gameObject);
    }



    public EnemyType GetEnemyType() => m_enemyType;
    public void SetEnemyType(EnemyType _enemyType) => m_enemyType = _enemyType;
}