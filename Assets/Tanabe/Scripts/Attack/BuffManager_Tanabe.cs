using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuffManager_Tanabe.Buff;

public class BuffManager_Tanabe : MonoBehaviour
{
    [System.Serializable]
    public class Buff
    {
        public enum BuffType
        {
            NONE_TYPE,
            HEAL_ONCE,
            HEAL_MULTIPLE,
            POWER_UP,
            SPEED_UP,
        }

        public BuffType buffType;

        public bool isUse = false;
        public float value = 0.0f;
        public float duration = 10.0f;
        public GameObject auraBuff;
    }

    private List<Buff> m_buffs = new List<Buff>();
    Player_Tanabe m_playerData;

    // Start is called before the first frame update
    void Start()
    {
        m_playerData = GetComponent<Player_Tanabe>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < m_buffs.Count; i++)
        {
            if (!m_buffs[i].isUse) { continue; }

            // ���ʎ��Ԃ̃J�E���g
            m_buffs[i].duration -= Time.deltaTime;

            if (m_buffs[i].buffType == BuffType.HEAL_MULTIPLE && (int)m_buffs[i].duration % 3 != 0 && (int)(m_buffs[i].duration + Time.deltaTime) % 3 == 0)
            {
                Heal_Once(m_buffs[i].value);
            }

            // ���ʎ��Ԃ��؂ꂽ��o�t������
            if (m_buffs[i].duration <= 0.0f)
            {
                BuffLost(m_buffs[i]);
            }
        }
    }

    // �o�t����������
    public void BuffLost(Buff _buff)
    {
        switch (_buff.buffType)
        {
            case Buff.BuffType.HEAL_MULTIPLE:
                {

                    break;
                }
            case Buff.BuffType.POWER_UP:
                {
                    m_playerData.SetPower(m_playerData.GetPower() - _buff.value);
                    break;
                }
            case Buff.BuffType.SPEED_UP:
                {
                    m_playerData.SetMoveSpeed(m_playerData.GetDefaultMoveSpeed() - _buff.value);
                    break;
                }
            default:
                break;
        }

        //if (_buff.buffType == Buff.BuffType.POWER_UP)
        //{
        //    m_playerData.SetPower(m_playerData.GetPower() - _buff.value);
        //}
        //else if(_buff.buffType == Buff.BuffType.SPEED_UP)
        //{
        //    m_playerData.SetMoveSpeed(m_playerData.GetDefaultMoveSpeed() - _buff.value);
        //}

        Destroy(_buff.auraBuff.gameObject);
        m_buffs.Remove(_buff);
        Debug.Log("�o�t���P������");
    }

    // �o�t�̒ǉ�
    public void AddBuff(Buff _buff)
    {
        m_buffs.Add(_buff);
    }

    // �o�t�̒ǉ�
    public void AddBuff(Buff.BuffType _buffType)
    {
        switch (_buffType)
        {
            case Buff.BuffType.HEAL_MULTIPLE:
                {
                    Heal_Multiple();
                    break;
                }
            case Buff.BuffType.POWER_UP:
                {
                    PowerUp();
                    break;
                }
            case Buff.BuffType.SPEED_UP:
                {
                    SpeedUp();
                    break;
                }
            default:
                break;
        }
    }

    // HP�̉�
    public void Heal_Once(float _heal)
    {
        m_playerData.SetHp(m_playerData.GetHp() + _heal);

        //Buff buff = new Buff();
        //buff.buffType = Buff.BuffType.HEAL_ONCE;
        //buff.value = _heal;
        //buff.duration = 0.0f;
        //AddBuff(buff);
    }

    // HP�̌p����
    public void Heal_Multiple()
    {
        if (m_playerData == null) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.HEAL_MULTIPLE;

        buff.isUse = true;
        buff.value = 10.0f;
        buff.duration = 15.1f;

        // �v���n�u��GameObject�^�Ŏ擾
        GameObject obj = (GameObject)Resources.Load("Healing");
        buff.auraBuff = Instantiate(obj);

        buff.auraBuff.transform.parent = m_playerData.transform;
        buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);

        AddBuff(buff);
    }

    // HP�̌p����
    public void Heal_Multiple(float _heal, float _duration)
    {
        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.HEAL_MULTIPLE;
        buff.value = _heal;
        buff.duration = _duration;
        AddBuff(buff);
    }

    // �U���̓A�b�v
    public void PowerUp()
    {
        if (m_playerData == null) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.POWER_UP;

        buff.isUse = true;
        buff.value = m_playerData.GetInitialParameter().power * 0.25f;


        // 1�`3�̊ԂŃ����_���Ȓl���擾���A1�Ȃ�ʂ�
        if (Random.Range(1, 4) == 1)
        {
            buff.value *= -1.0f;

            // �v���n�u��GameObject�^�Ŏ擾
            GameObject obj = (GameObject)Resources.Load("AuraBuff_PowerDown");
            buff.auraBuff = Instantiate(obj);

            buff.auraBuff.transform.parent = m_playerData.transform;
            buff.auraBuff.transform.localPosition = new Vector3(0f, 1f, 0f);
        }
        else
        {
            // �v���n�u��GameObject�^�Ŏ擾
            GameObject obj = (GameObject)Resources.Load("AuraBuff_PowerUp");
            buff.auraBuff = Instantiate(obj);

            buff.auraBuff.transform.parent = m_playerData.transform;
            buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);
        }
        buff.duration = 15.0f;


        AddBuff(buff);
        m_playerData.SetPower(m_playerData.GetPower() + buff.value);
    }

    // �U���̓A�b�v
    public void PowerUp(float _power, float _duration)
    {
        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.POWER_UP;
        buff.value = _power;
        buff.duration = _duration;
        AddBuff(buff);

        m_playerData.SetPower(m_playerData.GetPower() + _power);
    }

    // �ړ����x�A�b�v
    public void SpeedUp()
    {
        if (m_playerData == null) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.SPEED_UP;

        buff.isUse = true;
        buff.value = m_playerData.GetInitialParameter().moveSpeed * 0.4f;
        buff.duration = 10.0f;

        // �v���n�u��GameObject�^�Ŏ擾
        GameObject obj = (GameObject)Resources.Load("AuraBuff_SpeedUp");
        buff.auraBuff = Instantiate(obj);

        buff.auraBuff.transform.parent = m_playerData.transform;
        buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);

        AddBuff(buff);
        m_playerData.SetMoveSpeed(m_playerData.GetDefaultMoveSpeed() + buff.value);
    }

    // �ړ����x�A�b�v
    public void SpeedUp(float _speed, float _duration)
    {
        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.SPEED_UP;
        buff.value = _speed;
        buff.duration = _duration;
        AddBuff(buff);

        m_playerData.SetMoveSpeed(m_playerData.GetDefaultMoveSpeed() + _speed);
    }

}
