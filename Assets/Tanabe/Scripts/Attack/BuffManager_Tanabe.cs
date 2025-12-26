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
            STAMINAN,
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
    public override void OnStartServer()
    {
        base.OnStartServer();
        m_playerData = GetComponent<Player_Tanabe>();
        m_effectGenerator = GameObject.Find("BuffEffectGenerator").GetComponent<BuffEffectGenerator_Tanabe>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        m_playerData = GetComponent<Player_Tanabe>();
        m_effectGenerator = GameObject.Find("BuffEffectGenerator").GetComponent<BuffEffectGenerator_Tanabe>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_effectGenerator == null)
        {
            m_effectGenerator = GameObject.Find("BuffEffectGenerator").GetComponent<BuffEffectGenerator_Tanabe>();
            if (m_effectGenerator == null)
            {
                return;
            }
        }

        if (!this.isLocalPlayer) { return; }

        Vector3 playerPosition = m_playerData.transform.position;

        for (int i = 0; i < m_buffs.Count; i++)
        {
            if (!m_buffs[i].isUse) { continue; }

            // 効果時間のカウント
            m_buffs[i].duration -= Time.deltaTime;

            if (m_buffs[i].buffType == BuffType.HEAL_MULTIPLE && (int)m_buffs[i].duration % 3 != 0 && (int)(m_buffs[i].duration + Time.deltaTime) % 3 == 0)
            {
                CmdHeal_Once(m_buffs[i].value);
            }

            // 効果時間が切れたらバフを失う
            if (m_buffs[i].duration <= 0.0f)
            {
                m_buffs[i].isUse = false;
                BuffLost(m_buffs[i]);
            }
            else
            {
                Vector3 posAdjustment = new Vector3(0f, -1f, 0f);
                if (m_buffs[i].value < 0f)
                {
                    posAdjustment = new Vector3(0f, 1f, 0f);
                }
                this.CmdSetBuffPosition(m_buffs[i].auraBuff, playerPosition + posAdjustment);
            }
        }

        for (int i = 0; i < m_buffs.Count; i++)
        {
            if (!m_buffs[i].isUse) { continue; }
            if (m_buffs[i].buffType == BuffType.STAMINAN)
            {
                m_playerData.SetIsStaminan(true);
                break;
            }
        }
    }

    // バフを失う処理
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
            case Buff.BuffType.STAMINAN:
                {
                    m_playerData.SetIsStaminan(false);
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
        Debug.Log("バフを１つ失った");
    }

    // バフの追加
    public void AddBuff(Buff _buff)
    {
        m_buffs.Add(_buff);
    }

    // バフの追加
    [Command]
    public void CmdAddBuff(Buff.BuffType _buffType)
    {
        if (m_effectGenerator == null)
        {
            m_effectGenerator = GameObject.Find("BuffEffectGenerator").GetComponent<BuffEffectGenerator_Tanabe>();
            if (m_effectGenerator == null)
            {
                return;
            }
        }


        switch (_buffType)
        {
            case Buff.BuffType.HEAL_MULTIPLE:
                {
                    GameObject obj = Instantiate(m_effectGenerator.GetEffect_Healing(), m_playerData.transform.position + new Vector3(0f, -1f, 0f), m_effectGenerator.GetEffect_Healing().transform.rotation, m_playerData.transform);
                    NetworkServer.Spawn(obj);
                    //obj.transform.parent = m_playerData.transform;
                    //obj.transform.localPosition = new Vector3(0f, -1f, 0f);
                    RpcHeal_Multiple(obj);
                    break;
                }
            case Buff.BuffType.POWER_UP:
                {
                    int randNum = Random.Range(1, 10);
                    GameObject obj = null;
                    // 1～3の間でランダムな値を取得し、1なら通す
                    if (randNum == 1)
                    {
                        obj = Instantiate(m_effectGenerator.GetEffect_PowerDown(), m_playerData.transform.position + new Vector3(0f, 1f, 0f), m_effectGenerator.GetEffect_PowerDown().transform.rotation, m_playerData.transform);
                        NetworkServer.Spawn(obj);
                        //obj.transform.parent = m_playerData.transform;
                        //obj.transform.localPosition = new Vector3(0f, 1f, 0f);
                    }
                    else
                    {
                        obj = Instantiate(m_effectGenerator.GetEffect_PowerUp(), m_playerData.transform.position + new Vector3(0f, -1f, 0f), m_effectGenerator.GetEffect_PowerUp().transform.rotation, m_playerData.transform);
                        NetworkServer.Spawn(obj);
                        //obj.transform.parent = m_playerData.transform;
                        //obj.transform.localPosition = new Vector3(0f, -1f, 0f);
                    }
                    RpcPowerUp(obj, randNum);
                    break;
                }
            case Buff.BuffType.SPEED_UP:
                {
                    GameObject obj = Instantiate(m_effectGenerator.GetEffect_SpeedUp(), m_playerData.transform.position + new Vector3(0f, -1f, 0f), m_effectGenerator.GetEffect_SpeedUp().transform.rotation, m_playerData.transform);
                    NetworkServer.Spawn(obj);
                    //obj.transform.parent = m_playerData.transform;
                    //obj.transform.localPosition = new Vector3(0f, -1f, 0f);
                    RpcSpeedUp(obj);
                    break;
                }
            case Buff.BuffType.STAMINAN:
                {
                    GameObject obj = Instantiate(m_effectGenerator.GetEffect_Staminan(), m_playerData.transform.position + new Vector3(0f, -1f, 0f), m_effectGenerator.GetEffect_Staminan().transform.rotation, m_playerData.transform);
                    NetworkServer.Spawn(obj);
                    //obj.transform.parent = m_playerData.transform;
                    //obj.transform.localPosition = new Vector3(0f, -1f, 0f);
                    RpcStaminan(obj);
                    break;
                }
            default:
                break;
        }
    }

    // HPの回復
    [Command]
    public void CmdHeal_Once(float _heal)
    {
        m_playerData.RpcHeal(_heal);
        //m_playerData.SetHp(m_playerData.GetHp() + _heal);
    }

    // HPの継続回復
    [ClientRpc]
    public void RpcHeal_Multiple(GameObject _effect)
    {
        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
            if(m_playerData == null) { return; }
        }
        if (_effect == null) { return; }

        //GameObject obj = Instantiate(m_effectGenerator.GetEffect_Healing());
        //NetworkServer.Spawn(obj);
        _effect.transform.parent = m_playerData.transform;
        _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        if (!this.isLocalPlayer) { return; }

        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
        }

        if (_effect.transform.parent == null)
        {
            _effect.transform.parent = m_playerData.transform;
        }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.HEAL_MULTIPLE;

        buff.isUse = true;
        buff.value = 10.0f;
        buff.duration = 15.1f;

        // プレハブをGameObject型で取得
        //GameObject obj = (GameObject)Resources.Load("Healing");
        //buff.auraBuff = Instantiate(obj);
        buff.auraBuff = _effect;

        //buff.auraBuff.transform.parent = m_playerData.transform;
        //buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);
        //this.CmdAddEffect(buff.auraBuff, new Vector3(0f, -1f, 0f));

        AddBuff(buff);
    }

    // 攻撃力アップ
    [ClientRpc]
    public void RpcPowerUp(GameObject _effect, int randNum)
    {
        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
            if (m_playerData == null) { return; }
        }
        if (_effect == null) { return; }

        _effect.transform.parent = m_playerData.transform;
        _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        if (randNum == 1)
        {
            _effect.transform.localPosition = new Vector3(0f, 1f, 0f);
        }

        //// 1～3の間でランダムな値を取得し、1なら通す
        //if (randNum == 1)
        //{
        //    _effect.transform.localPosition = new Vector3(0f, 1f, 0f);
        //}
        //else
        //{
        //    _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        //}

        if (!this.isLocalPlayer) { return; }

        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
        }

        if (_effect.transform.parent == null)
        {
            _effect.transform.parent = m_playerData.transform;
        }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.POWER_UP;

        buff.isUse = true;
        buff.value = m_playerData.GetInitialParameter().power * 0.25f;

        buff.auraBuff = _effect;

        // 1～3の間でランダムな値を取得し、1なら通す
        if (randNum == 1)
        {
            buff.value *= -1.0f;
        }

        buff.duration = 15.0f;


        AddBuff(buff);
        m_playerData.SetPower(m_playerData.GetPower() + buff.value);
    }

    // 移動速度アップ
    [ClientRpc]
    public void RpcSpeedUp(GameObject _effect)
    {
        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
            if (m_playerData == null) { return; }
        }
        if (_effect == null) { return; }

        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
        }

        if (_effect.transform.parent == null)
        {
            _effect.transform.parent = m_playerData.transform;
        }

        _effect.transform.parent = m_playerData.transform;
        _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        if (!this.isLocalPlayer) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.SPEED_UP;

        buff.isUse = true;
        buff.value = m_playerData.GetInitialParameter().moveSpeed * 0.4f;
        buff.duration = 10.0f;

        // プレハブをGameObject型で取得
        //GameObject obj = (GameObject)Resources.Load("AuraBuff_SpeedUp");
        buff.auraBuff = _effect;

        //buff.auraBuff.transform.parent = m_playerData.transform;
        //buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);

        AddBuff(buff);
        m_playerData.SetMoveSpeed(m_playerData.GetDefaultMoveSpeed() + buff.value);
    }

    // スタミナ無限
    [ClientRpc]
    public void RpcStaminan(GameObject _effect)
    {
        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
            if (m_playerData == null) { return; }
        }
        if (_effect == null) { return; }

        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
        }

        if (_effect.transform.parent == null)
        {
            _effect.transform.parent = m_playerData.transform;
        }

        _effect.transform.parent = m_playerData.transform;
        _effect.transform.localPosition = new Vector3(0f, -1f, 0f);
        if (!this.isLocalPlayer) { return; }

        Buff buff = new Buff();
        buff.buffType = Buff.BuffType.STAMINAN;

        buff.isUse = true;
        //buff.value = m_playerData.GetInitialParameter().moveSpeed * 0.4f;
        buff.duration = 3.0f;

        // プレハブをGameObject型で取得
        //GameObject obj = (GameObject)Resources.Load("AuraBuff_SpeedUp");
        buff.auraBuff = _effect;

        //buff.auraBuff.transform.parent = m_playerData.transform;
        //buff.auraBuff.transform.localPosition = new Vector3(0f, -1f, 0f);

        AddBuff(buff);
        m_playerData.SetIsStaminan(true);
    }

    public void SetBuffPosition(GameObject _effect, Vector3 _pos)
    {
        if(_effect.transform.parent != null) { return; }
        if (m_playerData == null)
        {
            m_playerData = GetComponent<Player_Tanabe>();
        }

        if(_effect.transform.parent == null)
        {
            _effect.transform.parent = m_playerData.transform;
        }

        _effect.transform.position = _pos;
    }

    [Command]
    public void CmdSetBuffPosition(GameObject _effect, Vector3 _pos)
    {
        this.RpcSetBuffPosition(_effect, _pos);
    }

    [ClientRpc]
    public void RpcSetBuffPosition(GameObject _effect, Vector3 _pos)
    {
        if(!_effect) { return; }
        this.SetBuffPosition(_effect, _pos);
    }
}
