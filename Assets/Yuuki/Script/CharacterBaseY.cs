using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterParameterY
{
    [Header("HP ※デフォルト値:100"), Range(0, 100)] public int hp;
    [Header("攻撃力 ※デフォルト値:20"), Range(0, 50)] public int power;
    [Header("移動速度 ※デフォルト値:10.0"), Range(0f, 50f)] public float moveSpeed;
    [Header("スタミナ ※デフォルト値:20.0"), Range(0f, 50f)] public float stamina;
}

public class CharacterBaseY : MonoBehaviour
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

    [Header("初期パラメータ"), SerializeField]
    private CharacterParameterY m_initialParameter;
    // 現在のパラメータ
    private int m_hp;
    private int m_power;
    private float m_moveSpeed;
    private float m_stamina;

    // 自身のタイプ(分類)
    private CharacterType m_characterType = CharacterType.NONE_TYPE;

    // ＜関数＞ーーーーーーーーーーーーーーーーーーーーーーーー

    // 開始関数
    public virtual void Start()
    {
        // 初期化処理
        Initialize();
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

    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="_damage">攻撃してきたキャラクターの攻撃力を取得する</param>
    public virtual void Damage(int _damage)
    {
        SetHp(m_hp - _damage);
    }

    // ＜ゲッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // キャラクタータイプの取得
    public CharacterType GetCharacterType()
    {
        return m_characterType;
    }

    // 初期パラメータの取得
    public CharacterParameterY GetInitialParameter()
    {
        return m_initialParameter;
    }

    // 最大HPの取得
    public virtual int GetMaxHP()
    {
        return m_initialParameter.hp;
    }

    // HPの取得
    public virtual int GetHp()
    {
        return m_hp;
    }

    // 攻撃力の取得
    public virtual int GetPower()
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

    // ＜セッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // キャラクタータイプの設定
    public void SetCharacterType(CharacterType _characterType)
    {
        m_characterType = _characterType;
    }

    // HPの設定
    public virtual void SetHp(int _hp)
    {
        m_hp = Mathf.Min(Mathf.Max(0, _hp), GetMaxHP());
    }

    // 攻撃力の設定
    public virtual void SetPower(int _power)
    {
        m_power = _power;
    }

    // 移動速度の設定
    public virtual void SetMoveSpeed(float _moveSpeed)
    {
        m_moveSpeed = _moveSpeed;
    }

    // スタミナの設定
    public virtual void SetStamina(float _stamina)
    {
        m_stamina = Mathf.Min(Mathf.Max(0.0f, _stamina), m_initialParameter.stamina);
    }
}
