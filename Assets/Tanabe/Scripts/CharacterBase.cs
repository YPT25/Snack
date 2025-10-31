using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class CharacterParameter
{
    [SyncVar, Header("HP ���f�t�H���g�l:100"), Range(0f, 100f)] public float hp;
    [SyncVar, Header("�U���� ���f�t�H���g�l:20"), Range(0f, 50f)] public float power;
    [SyncVar, Header("�ړ����x ���f�t�H���g�l:8.0"), Range(0f, 50f)] public float moveSpeed;
    [SyncVar, Header("�X�^�~�i ���f�t�H���g�l:5.0"), Range(0f, 50f)] public float stamina;
}

public class CharacterBase : NetworkBehaviour
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

    [SyncVar, Header("�����p�����[�^"), SerializeField]
    private CharacterParameter m_initialParameter;
    // ���݂̃p�����[�^
    private float m_hp;
    [SyncVar] private float m_power;
    [SyncVar] private float m_moveSpeed;
    [SyncVar] private float m_stamina;

    // �t���O
    [SyncVar] private bool m_isMove = true;
    [SyncVar] private bool m_isAttack = true;

    // ���g�̃^�C�v(����)
    [SyncVar] private CharacterType m_characterType = CharacterType.NONE_TYPE;

    // ���֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �J�n�֐�
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        // ����������
        Initialize();
        base.OnStartClient();
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

    public virtual void FixedUpdate()
    {

    }

    /// <summary>
    /// �_���[�W����
    /// </summary>
    /// <param name="_damage">�U�����Ă����L�����N�^�[�̍U���͂��擾����</param>
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
    /// �񕜏���
    /// </summary>
    /// <param name="_heal">�񕜂���l</param>
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

    // ���Q�b�^�[�֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �L�����N�^�[�^�C�v�̎擾
    public CharacterType GetCharacterType()
    {
        return m_characterType;
    }

    // �����p�����[�^�̎擾
    public CharacterParameter GetInitialParameter()
    {
        return m_initialParameter;
    }

    // �ő�HP�̎擾
    public virtual float GetMaxHP()
    {
        return m_initialParameter.hp;
    }

    // HP�̎擾
    public virtual float GetHp()
    {
        return m_hp;
    }

    // �U���͂̎擾
    public virtual float GetPower()
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

    // �ړ��\�t���O�̎擾
    public bool GetIsMove()
    {
        return m_isMove;
    }

    // �U���\�t���O�̎擾
    public bool GetIsAttack()
    {
        return m_isAttack;
    }

    // ���Z�b�^�[�֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �L�����N�^�[�^�C�v�̐ݒ�
    public void SetCharacterType(CharacterType _characterType)
    {
        m_characterType = _characterType;
    }

    // HP�̐ݒ�
    public virtual void SetHp(float _hp)
    {
        m_hp = Mathf.Min(Mathf.Max(0.0f, _hp), GetMaxHP());
    }

    // �U���͂̐ݒ�
    public virtual void SetPower(float _power)
    {
        m_power = Mathf.Max(0.0f, _power);
    }

    // �ړ����x�̐ݒ�
    public virtual void SetMoveSpeed(float _moveSpeed)
    {
        m_moveSpeed = Mathf.Max(0.0f, _moveSpeed);
    }

    // �X�^�~�i�̐ݒ�
    public virtual void SetStamina(float _stamina)
    {
        m_stamina = Mathf.Min(Mathf.Max(0.0f, _stamina), m_initialParameter.stamina);
    }

    // �ړ��\�t���O�̐ݒ�
    public void SetIsMove(bool _flag)
    {
        m_isMove = _flag;
    }

    // �ړ�����t���O�̐ݒ�
    [ClientRpc]
    public void RpcSetIsMove(bool _flag)
    {
        m_isMove = _flag;
    }

    // �U���\�t���O�̐ݒ�
    public void SetIsAttack(bool _flag)
    {
        m_isAttack = _flag;
    }

    // �U���\�t���O�̐ݒ�
    [ClientRpc]
    public void RpcSetIsAttack(bool _flag)
    {
        m_isAttack = _flag;
    }
}
