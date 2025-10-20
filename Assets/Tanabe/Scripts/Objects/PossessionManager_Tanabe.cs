using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PossessionManager_Tanabe : NetworkBehaviour
{
    private Player_Tanabe m_player;

    private BuffManager_Tanabe m_buffManager;

    private ItemStateMachine[] m_items = new ItemStateMachine[2];

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
        if(!m_player.isLocalPlayer) { return; }

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
                UsesItem(0);
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
                UsesItem(1);
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

    private void UsesItem(int _index)
    {
        switch (m_items[_index].GetItemType())
        {
            case ItemStateMachine.ItemType.BUFF:
                {
                    m_buffManager.AddBuff(m_items[_index].GetBuffType());
                    Destroy(m_items[_index].gameObject);
                    m_items[_index] = null;
                    m_leftHand.SetIsHand(false);
                    break;
                }
            case ItemStateMachine.ItemType.THROW:
                {
                    if (!m_player.GetIsThrow())
                    {
                        m_items[_index].CmdChangeState(m_items[_index], ItemStateMachine.ItemStateType.PREPARINGTHROW);
                        m_player.SetIsThrow(true);

                        m_items[_index] = null;
                        m_leftHand.SetIsHand(false);
                    }
                    break;
                }
            case ItemStateMachine.ItemType.TRAP:
                {
                    m_items[_index].CmdChangeState(m_items[_index], ItemStateMachine.ItemStateType.TRAP);
                    m_items[_index] = null;
                    m_leftHand.SetIsHand(false);
                    break;
                }
            case ItemStateMachine.ItemType.TRAP_BOMB:
                {
                    m_items[_index].CmdChangeState(m_items[_index], ItemStateMachine.ItemStateType.TRAP);

                    m_items[_index] = null;
                    m_leftHand.SetIsHand(false);
                    break;
                }
            default:
                break;
        }
    }

    // アイテムの追加 ※所持上限に達していたらfalseを返す
    public bool AddItem(ItemStateMachine _item)
    {
        if (m_items[0] == null)
        {
            m_items[0] = _item;
            CmdSetItemActive(m_items[0].gameObject, false);
            return true;
        }
        else if (m_items[1] == null)
        {
            m_items[1] = _item;
            CmdSetItemActive(m_items[1].gameObject, false);
            return true;
        }

        return false;
    }

    // アイテム所持数が最大か ※最大ならtrueを返す
    public bool IsMaxPossession()
    {
        if (m_items[0] == null || m_items[1] == null) { return false; }
        return true;
    }

    [Command]
    private void CmdSetItemActive(GameObject _item, bool _flag)
    {
        _item.SetActive(_flag);
    }
}
