using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PossessionManager_Tanabe : NetworkBehaviour
{
    private Player_Tanabe m_player;

    private BuffManager_Tanabe m_buffManager;

    private ItemStateMachine[] m_items = new ItemStateMachine[2];
    private bool m_isMaxItem = false;

    private LeftHand_Tanabe m_leftHand;

    // Start is called before the first frame update
    void Start()
    {
        m_player = GetComponent<Player_Tanabe>();
        m_buffManager = GetComponent<BuffManager_Tanabe>();
        m_leftHand = GetComponentInChildren<LeftHand_Tanabe>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!this.isLocalPlayer) { return; }

        if (m_items[0] != null && m_items[0].GetItemStateType() != ItemStateMachine.ItemStateType.HANDS)
        {
            CmdSetItemActive(m_items[0].gameObject, true);
            m_items[0] = null;
        }
        if (m_items[1] != null && m_items[1].GetItemStateType() != ItemStateMachine.ItemStateType.HANDS)
        {
            CmdSetItemActive(m_items[1].gameObject, true);
            m_items[1] = null;
        }

        if (!m_player.GetIsDefaultState())
        {
            if (m_items[0] != null) CmdSetItemActive(m_items[0].gameObject, false);
            if (m_items[1] != null) CmdSetItemActive(m_items[1].gameObject, false);
            m_leftHand.SetIsHand(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && m_items[0] != null ||
            Input.GetKeyDown("joystick button 2") && m_items[0] != null)
        {
            if (m_items[0].gameObject.active)
            {
                CmdUsesItem(0);
                return;
            }

            // プレイヤーの左手に持つ
            CmdSetItemActive(m_items[0].gameObject, true);
            if (m_items[1] != null) CmdSetItemActive(m_items[1].gameObject, false);
            m_leftHand.SetIsHand(true);
        }
        else if (Input.GetKeyDown(KeyCode.E) && m_items[1] != null ||
                Input.GetKeyDown("joystick button 3") && m_items[1] != null)
        {
            if (m_items[1].gameObject.active)
            {
                CmdUsesItem(1);
                return;
            }

            // プレイヤーの左手に持つ
            if (m_items[0] != null) CmdSetItemActive(m_items[0].gameObject, false);
            CmdSetItemActive(m_items[1].gameObject, true);

            m_leftHand.SetIsHand(true);
        }

        //// プレイヤーの左手に持つ
        //m_items[0].GetPlayerData().GetPossesionManager().AddItem(m_items[0]);
    }

    [Command]
    private void CmdUsesItem(int _index)
    {
        RpcUsesItem(_index);
    }

    [ClientRpc]
    private void RpcUsesItem(int _index)
    {
        bool prevFlag = m_isMaxItem;
        m_isMaxItem = false;
        if (m_items[_index] != null && m_items[_index].GetItemType() == ItemStateMachine.ItemType.THROW && m_player.GetIsThrow())
        {
            // 10月24日ここだけ同期させる
            m_isMaxItem = prevFlag;
            return;
        }
        Debug.Log("useitem!!!!");
        ItemStateMachine item = m_items[_index];
        m_items[_index] = null;
        if(!this.isLocalPlayer) { return; }
        m_leftHand.SetIsHand(false);

        switch (item.GetItemType())
        {
            case ItemStateMachine.ItemType.BUFF:
                {
                    Debug.Log("useitem!!!!!");

                    m_buffManager.CmdAddBuff(item.GetBuffType());
                    Debug.Log("useitem!!!!!!");

                    m_player.CmdDestroysObject(item.gameObject);
                    //Destroy(item.gameObject);
                    break;
                }
            case ItemStateMachine.ItemType.THROW:
                {
                    CmdChangeState_Item(item, ItemStateMachine.ItemStateType.PREPARINGTHROW);

                    //item.CmdChangeState(item.netIdentity, ItemStateMachine.ItemStateType.PREPARINGTHROW);
                    m_player.SetRightHandsItem(item);
                    m_player.SetIsThrow(true);
                    break;
                }
            case ItemStateMachine.ItemType.TRAP:
                {
                    CmdChangeState_Item(item, ItemStateMachine.ItemStateType.TRAP);
                    break;
                }
            case ItemStateMachine.ItemType.TRAP_BOMB:
                {
                    CmdChangeState_Item(item, ItemStateMachine.ItemStateType.TRAP);
                    break;
                }
            default:
                break;
        }
    }

    // アイテムの追加 ※所持上限に達していたらfalseを返す
    public bool AddItem(ItemStateMachine _item)
    {
        bool isAdd = false;

        if (m_items[0] == null && m_items[1] != _item)
        {
            m_items[0] = _item;
            CmdSetItemActive(m_items[0].gameObject, false);
            isAdd = true;
        }
        else if (m_items[1] == null && m_items[0] != _item)
        {
            m_items[1] = _item;
            CmdSetItemActive(m_items[1].gameObject, false);
            isAdd = true;
        }

        if (m_items[0] == null || m_items[1] == null)
        {
            m_isMaxItem = false;
        }
        else
        {
            m_isMaxItem = true;
        }

        return isAdd;
    }

    // 同じオブジェクトを持っているかチェックする ※持っていたらtrueを返す
    public bool CheckItem(ItemStateMachine _item)
    {
        if(_item == m_items[0] || _item == m_items[1])
        {
            return true;
        }
        return false;
    }

    // アイテム所持数が最大か ※最大ならtrueを返す
    public bool IsMaxPossession()
    {
        return m_isMaxItem;
        //if (m_items[0] == null || m_items[1] == null) { return false; }
        //return true;
    }

    [Command]
    private void CmdSetItemActive(GameObject _item, bool _flag)
    {
        _item.SetActive(_flag);
        RpcSetItemActive(_item, _flag);
    }

    [ClientRpc]
    private void RpcSetItemActive(GameObject _item, bool _flag)
    {
        _item.SetActive(_flag);
    }

    [Command]
    private void CmdChangeState_Item(ItemStateMachine _item, ItemStateMachine.ItemStateType _newStateType)
    {
        _item.RpcChangeState(_item, _newStateType);
    }
}
