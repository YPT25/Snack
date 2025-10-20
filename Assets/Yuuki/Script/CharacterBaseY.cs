using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterParameterY
{
    [Header("HP ���f�t�H���g�l:100"), Range(0, 100)] public int hp;
    [Header("�U���� ���f�t�H���g�l:20"), Range(0, 50)] public int power;
    [Header("�ړ����x ���f�t�H���g�l:10.0"), Range(0f, 50f)] public float moveSpeed;
    [Header("�X�^�~�i ���f�t�H���g�l:20.0"), Range(0f, 50f)] public float stamina;
}

public class CharacterBaseY : MonoBehaviour
{
    // ���񋓌^���[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �L�����N�^�[�̃^�C�v(����)
    public enum CharacterType
    {
        NONE_TYPE,
        HERO_TYPE,
        ENEMY_TYPE,
    }

    // ���p�����[�^���[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    [Header("�����p�����[�^"), SerializeField]
    private CharacterParameterY m_initialParameter;
    // ���݂̃p�����[�^
    private int m_hp;
    private int m_power;
    private float m_moveSpeed;
    private float m_stamina;

    // ���g�̃^�C�v(����)
    private CharacterType m_characterType = CharacterType.NONE_TYPE;

    // ���֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �J�n�֐�
    public virtual void Start()
    {
        // ����������
        Initialize();
    }

    // �������֐�
    public virtual void Initialize()
    {
        m_hp = m_initialParameter.hp;
        m_power = m_initialParameter.power;
        m_moveSpeed = m_initialParameter.moveSpeed;
        m_stamina = m_initialParameter.stamina;
    }

    // �X�V�֐�
    public virtual void Update()
    {

    }

    /// <summary>
    /// �_���[�W����
    /// </summary>
    /// <param name="_damage">�U�����Ă����L�����N�^�[�̍U���͂��擾����</param>
    public virtual void Damage(int _damage)
    {
        SetHp(m_hp - _damage);
    }

    // ���Q�b�^�[�֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �L�����N�^�[�^�C�v�̎擾
    public CharacterType GetCharacterType()
    {
        return m_characterType;
    }

    // �����p�����[�^�̎擾
    public CharacterParameterY GetInitialParameter()
    {
        return m_initialParameter;
    }

    // �ő�HP�̎擾
    public virtual int GetMaxHP()
    {
        return m_initialParameter.hp;
    }

    // HP�̎擾
    public virtual int GetHp()
    {
        return m_hp;
    }

    // �U���͂̎擾
    public virtual int GetPower()
    {
        return m_power;
    }

    // �ړ����x�̎擾
    public virtual float GetMoveSpeed()
    {
        return m_moveSpeed;
    }

    // �X�^�~�i�̎擾
    public virtual float GetStamina()
    {
        return m_stamina;
    }

    // ���Z�b�^�[�֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �L�����N�^�[�^�C�v�̐ݒ�
    public void SetCharacterType(CharacterType _characterType)
    {
        m_characterType = _characterType;
    }

    // HP�̐ݒ�
    public virtual void SetHp(int _hp)
    {
        m_hp = Mathf.Min(Mathf.Max(0, _hp), GetMaxHP());
    }

    // �U���͂̐ݒ�
    public virtual void SetPower(int _power)
    {
        m_power = _power;
    }

    // �ړ����x�̐ݒ�
    public virtual void SetMoveSpeed(float _moveSpeed)
    {
        m_moveSpeed = _moveSpeed;
    }

    // �X�^�~�i�̐ݒ�
    public virtual void SetStamina(float _stamina)
    {
        m_stamina = Mathf.Min(Mathf.Max(0.0f, _stamina), m_initialParameter.stamina);
    }
}
