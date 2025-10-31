using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuffManager_Tanabe.Buff;
using Mirror;

public class BuffManager_Tanabe : NetworkBehaviour
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
    BuffEffectGenerator_Tanabe m_effectGenerator;

    // Start is called before the first frame update
    void Start()
    {
        m_playerData = GetComponent<Player_Tanabe>();
        m_effectGenerator = GameObject.Find("BuffEffectGenerator").GetComponent<BuffEffectGenerator_Tanabe>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!this.isLocalPlayer) { return; }

        for (int i = 0; i < m_buffs.Count; i++)
        {
            if (!m_buffs[i].isUse) { continue; }

            // ���ʎ��Ԃ̃J�E���g
            m_buffs[i].duration -= Time.deltaTime;

            if (m_buffs[i].buffType == BuffType.HEAL_MULTIPLE && (int)m_buffs[i].duration % 3 != 0 && (int)(m_buffs[i].duration + Time.deltaTime) % 3 == 0)
            {
                CmdHeal_Once(m_buffs[i].value);
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
        if(!this.isLocalPlayer) { return; }

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

        //Destroy(_buff.auraBuff.gameObject);
        m_playerData.CmdDestroysObject(_buff.auraBuff.gameObject);
        m_buffs.Remove(_buff);
        Debug.Log("�o�t���P������");
    }

    // �o�t�̒ǉ�
    public void AddBuff(Buff _buff)
    {
        m_buffs.Add(_buff);
    }

    // �o�t�̒ǉ�
    [Command]
    public void CmdAddBuff(Buff.BuffType _buffType)
    {
        switch (_buffType)
        {
            case Buff.BuffType.HEAL_MULTIPLE:
                {
                    GameObject obj = Instantiate(m_effectGenerator.GetEffect_Healing());
                    NetworkServer.Spawn(obj);
                    obj.transform.parent = m_playerData.transform;
                    obj.transform.localPosition = new Vector3(0f, -1f, 0f);
                    RpcHeal_Multiple(obj);
                    break;
                }
            case Buff.BuffType.POWER_UP:
                {
                    int randNum = Random.Range(1, 4);
                    GameObject obj = null;
                    // 1�`3�̊ԂŃ����_���Ȓl���擾���A1�Ȃ�ʂ�
                    if (randNum == 1)
                    {
                        obj = Instantiate(m_effectGenerator.GetEffect_PowerDown());
                        NetworkServer.Spawn(obj);
                        obj.transform.parent = m_playerData.transform;
                        obj.transform.localPosition = new Vector3(0f, 1f, 0f);
                    }
                    else
                    {
                        obj = Instantiate(m_effectGenerator.GetEffect_PowerUp());
                        NetworkServer.Spawn(obj);
                        obj.transform.parent = m_playerData.transform;
                        obj.transform.localPosition = new Vector3(0f, -1f, 0f);
                    }
                    RpcPowerUp(obj, randNum);
                    break;
                }
            case Buff.BuffType.SPEED_UP:
                {
                    GameObject obj = Instantiate(m_effectGenerator.GetEffect_SpeedUp());
                    NetworkServer.Spawn(obj);
                    obj.transform.parent = m_playerData.transform;
                    obj.transform.localPosition = new Vector3(0f, -1f, 0f);
                    RpcSpeedUp(obj);
                    break;
                }
            default:
                break;
        }
    }

    // HP�̉�
    [Command]
    public void CmdHeal_Once(float _heal)
    {
        m_playerData.RpcHeal(_heal);
        //m_playerData.SetHp(m_playerData.GetHp() + _heal);
    }

    // HP�̌p����
    [ClientRpc]
    public void RpcHeal_Multiple(GameObject _effect)
    {
        if (m_playerData == null) { return; }

        //GameObject obj = Instantiate(m_effectGenerator.GetEffect_Healing());
        //NetworkServer.Spawn(obj);
        _effect.transform.parent = m_playerData.transform;
        _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        if(!this.isLocalPlayer) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.HEAL_MULTIPLE;

        buff.isUse = true;
        buff.value = 10.0f;
        buff.duration = 15.1f;

        // �v���n�u��GameObject�^�Ŏ擾
        //GameObject obj = (GameObject)Resources.Load("Healing");
        //buff.auraBuff = Instantiate(obj);
        buff.auraBuff = _effect;

        //buff.auraBuff.transform.parent = m_playerData.transform;
        //buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);
        //this.CmdAddEffect(buff.auraBuff, new Vector3(0f, -1f, 0f));

        AddBuff(buff);
    }

    // �U���̓A�b�v
    [ClientRpc]
    public void RpcPowerUp(GameObject _effect, int randNum)
    {
        if (m_playerData == null) { return; }

        _effect.transform.parent = m_playerData.transform;

        // 1�`3�̊ԂŃ����_���Ȓl���擾���A1�Ȃ�ʂ�
        if (randNum == 1)
        {
            _effect.transform.localPosition = new Vector3(0f, 1f, 0f);
        }
        else
        {
            _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        }

        if(!this.isLocalPlayer) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.POWER_UP;

        buff.isUse = true;
        buff.value = m_playerData.GetInitialParameter().power * 0.25f;

        buff.auraBuff = _effect;

        // 1�`3�̊ԂŃ����_���Ȓl���擾���A1�Ȃ�ʂ�
        if (randNum == 1)
        {
            buff.value *= -1.0f;
        }

        buff.duration = 15.0f;


        AddBuff(buff);
        m_playerData.SetPower(m_playerData.GetPower() + buff.value);
    }

    // �ړ����x�A�b�v
    [ClientRpc]
    public void RpcSpeedUp(GameObject _effect)
    {
        if (m_playerData == null) { return; }

        _effect.transform.parent = m_playerData.transform;
        _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        if (!this.isLocalPlayer) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.SPEED_UP;

        buff.isUse = true;
        buff.value = m_playerData.GetInitialParameter().moveSpeed * 0.4f;
        buff.duration = 10.0f;

        // �v���n�u��GameObject�^�Ŏ擾
        //GameObject obj = (GameObject)Resources.Load("AuraBuff_SpeedUp");
        buff.auraBuff = _effect;

        //buff.auraBuff.transform.parent = m_playerData.transform;
        //buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);

        AddBuff(buff);
        m_playerData.SetMoveSpeed(m_playerData.GetDefaultMoveSpeed() + buff.value);
    }
}
