using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class CharacterParameter
{
    [SyncVar, Header("HP ※デフォルト値:100"), Range(0f, 100f)] public float hp;
    [SyncVar, Header("攻撃力 ※デフォルト値:20"), Range(0f, 50f)] public float power;
    [SyncVar, Header("移動速度 ※デフォルト値:8.0"), Range(0f, 50f)] public float moveSpeed;
    [SyncVar, Header("スタミナ ※デフォルト値:5.0"), Range(0f, 50f)] public float stamina;
}

public class CharacterBase : NetworkBehaviour
{
    // ＜列挙型＞ーーーーーーーーーーーーーーーーーーーーーーー

    // キャラクターのタイプ(分類)
    public enum CharacterType
    {
        NONE_TYPE,
        HERO_TYPE,
        ENEMY_TYPE,
    }

    // ＜パラメータ＞ーーーーーーーーーーーーーーーーーーーーー

    [SyncVar, Header("初期パラメータ"), SerializeField]
    private CharacterParameter m_initialParameter;
    // 現在のパラメータ
    private float m_hp;
    [SyncVar] private float m_power;
    [SyncVar] private float m_moveSpeed;
    [SyncVar] private float m_stamina;

    // フラグ
    [SyncVar] private bool m_isMove = true;
    [SyncVar] private bool m_isAttack = true;

    // 自身のタイプ(分類)
    [SyncVar] private CharacterType m_characterType = CharacterType.NONE_TYPE;

    // ＜関数＞ーーーーーーーーーーーーーーーーーーーーーーーー

    // 開始関数
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        // 初期化処理
        Initialize();
        base.OnStartClient();
    }

    // 初期化関数
    public virtual void Initialize()
    {
        m_hp = m_initialParameter.hp;
        m_power = m_initialParameter.power;
        m_moveSpeed = m_initialParameter.moveSpeed;
        m_stamina = m_initialParameter.stamina;
    }

    // 更新関数
    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="_damage">攻撃してきたキャラクターの攻撃力を取得する</param>
    public virtual void Damage(float _damage)
    {
        SetHp(m_hp - _damage);
    }

    [Command]
    public void CmdDamage(float _damage)
    {
        Damage(_damage);
    }

    [ClientRpc]
    public void RpcDamage(float _damage)
    {
        Damage(_damage);
    }

    /// <summary>
    /// 回復処理
    /// </summary>
    /// <param name="_heal">回復する値</param>
    public void Heal(float _heal)
    {
        SetHp(m_hp + _heal);
    }

    [Command]
    public void CmdHeal(float _heal)
    {
        Heal(_heal);
    }

    [ClientRpc]
    public void RpcHeal(float _heal)
    {
        Heal(_heal);
    }

    // ＜ゲッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // キャラクタータイプの取得
    public CharacterType GetCharacterType()
    {
        return m_characterType;
    }

    // 初期パラメータの取得
    public CharacterParameter GetInitialParameter()
    {
        return m_initialParameter;
    }

    // 最大HPの取得
    public virtual float GetMaxHP()
    {
        return m_initialParameter.hp;
    }

    // HPの取得
    public virtual float GetHp()
    {
        return m_hp;
    }

    // 攻撃力の取得
    public virtual float GetPower()
    {
        return m_power;
    }

    // 移動速度の取得
    public virtual float GetMoveSpeed()
    {
        return m_moveSpeed;
    }

    // スタミナの取得
    public virtual float GetStamina()
    {
        return m_stamina;
    }

    // 移動可能フラグの取得
    public bool GetIsMove()
    {
        return m_isMove;
    }

    // 攻撃可能フラグの取得
    public bool GetIsAttack()
    {
        return m_isAttack;
    }

    // ＜セッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // キャラクタータイプの設定
    public void SetCharacterType(CharacterType _characterType)
    {
        m_characterType = _characterType;
    }

    // HPの設定
    public virtual void SetHp(float _hp)
    {
        m_hp = Mathf.Min(Mathf.Max(0.0f, _hp), GetMaxHP());
    }

    // 攻撃力の設定
    public virtual void SetPower(float _power)
    {
        m_power = Mathf.Max(0.0f, _power);
    }

    // 移動速度の設定
    public virtual void SetMoveSpeed(float _moveSpeed)
    {
        m_moveSpeed = Mathf.Max(0.0f, _moveSpeed);
    }

    // スタミナの設定
    public virtual void SetStamina(float _stamina)
    {
        m_stamina = Mathf.Min(Mathf.Max(0.0f, _stamina), m_initialParameter.stamina);
    }

    // 移動可能フラグの設定
    public void SetIsMove(bool _flag)
    {
        m_isMove = _flag;
    }

    // 移動判定フラグの設定
    [ClientRpc]
    public void RpcSetIsMove(bool _flag)
    {
        m_isMove = _flag;
    }

    // 攻撃可能フラグの設定
    public void SetIsAttack(bool _flag)
    {
        m_isAttack = _flag;
    }

    // 攻撃可能フラグの設定
    [ClientRpc]
    public void RpcSetIsAttack(bool _flag)
    {
        m_isAttack = _flag;
    }
}
