using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterBase;

public class EnemyBase : CharacterBaseY
{
    // ���񋓌^���[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �G���Ƃׂ̍�������
    public enum EnemyType
    {
        TYPE_NULL,
        TYPE_A,
        TYPE_B,
        TYPE_C,
        TYPE_D,
    }

    // ���p�����[�^���[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    [Header("�����ւ̍U���������邩�H")]
    [SerializeField] private bool m_canFriendlyFire = false;

    [Header("�G�l�~�[�^�C�v(�e�v���n�u�Őݒ�)")]
    [SerializeField] private EnemyType m_enemyType = EnemyType.TYPE_A; // Inspector�Őݒ�

    protected CharacterType m_enemyCharacterType = CharacterType.ENEMY_TYPE;

    // ���֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // Start is called before the first frame update
    public virtual void Start()
    {
        base.Start();
        SetCharacterType(m_enemyCharacterType);
    }

    /// <summary>
    /// �U�������i�h���N���X�ŏ㏑������z��j
    /// </summary>
    public virtual void Attack(CharacterBaseY target)
    {
        if (target == null) return;

        if (!m_canFriendlyFire && target.GetCharacterType() == m_enemyCharacterType)
        {
            Debug.Log($"{name} �� �����ւ̍U���͋֎~");
            return;
        }
        else
        {
            target.Damage(GetPower());
            Debug.Log($"{name} �� {target.name} �ɍU���I �_���[�W:{GetPower()}");
        }
    }

    /// <summary>
    /// ���ʂ̎��S����
    /// </summary>
    public virtual void Die()
    {
        Debug.Log($"{name} �͓|�ꂽ�I");
        Destroy(gameObject);
    }

    // ���A�N�Z�b�T�\���[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[
    public EnemyType GetEnemyType()
    {
        return m_enemyType;
    }

    public void SetEnemyType(EnemyType _enemyType)
    {
        m_enemyType = _enemyType;
    }

}
