using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterBase;

public class EnemyBase : CharacterBase
{
    // ＜列挙型＞ーーーーーーーーーーーーーーーーーーーーーーー

    // 敵ごとの細かい分類
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
    [SerializeField] private EnemyType m_enemyType = EnemyType.TYPE_A; // Inspectorで設定

    protected CharacterType m_enemyCharacterType = CharacterType.ENEMY_TYPE;

    // ＜関数＞ーーーーーーーーーーーーーーーーーーーーーーーー

    // Start is called before the first frame update
    public virtual void Start()
    {
        base.Initialize();
        SetCharacterType(m_enemyCharacterType);
    }

    /// <summary>
    /// 攻撃処理（派生クラスで上書きする想定）
    /// </summary>
    public virtual void Attack(CharacterBase target)
    {
        if (target == null) return;

        if (!m_canFriendlyFire && target.GetCharacterType() == m_enemyCharacterType)
        {
            Debug.Log($"{name} → 味方への攻撃は禁止");
            return;
        }
        else
        {
            target.RpcDamage(GetPower());
            Debug.Log($"{name} が {target.name} に攻撃！ ダメージ:{GetPower()}");
        }
    }

    /// <summary>
    /// 共通の死亡処理
    /// </summary>
    public virtual void Die()
    {
        Debug.Log($"{name} は倒れた！");
        // ネットワークオブジェクトなのでMirror経由で削除
        if (isServer)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    // ＜アクセッサ―＞ーーーーーーーーーーーーーーーーーーーーーーーー
    public EnemyType GetEnemyType()
    {
        return m_enemyType;
    }

    public void SetEnemyType(EnemyType _enemyType)
    {
        m_enemyType = _enemyType;
    }

}
