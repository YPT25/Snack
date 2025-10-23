using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DropState : IItemState_Tanabe
{
    private ItemStateMachine item;
    private Vector3 baseScale;
    private bool m_isDestroy = false;

    public DropState(ItemStateMachine item)
    {
        this.item = item;
    }

    public void Enter()
    {
        item.SetUseGravity(true);
        item.SetIsKinematic(false);
        baseScale = item.transform.localScale;
        if(item.GetItemType() != ItemStateMachine.ItemType.TRAP && item.GetItemType() != ItemStateMachine.ItemType.THROW)
        {
            item.transform.localScale = baseScale * 0.3f;
        }
        //item.GetComponent<BoxCollider>().isTrigger = false;
        item.GetColiider().isTrigger = false;
        Debug.Log("Drop:äJén");
    }

    public void Update()
    {
        if (m_isDestroy) { return; }
        else if (item.gameObject.transform.position.y <= -500.0f)
        {
            item.DestroysGameObject();
            m_isDestroy = true;
            return;
        }


        Vector3 localEulerAngles = item.transform.eulerAngles;
        localEulerAngles.y += item.rotateSpeed * Time.deltaTime;
        item.transform.rotation = Quaternion.Euler(localEulerAngles);

        if (item.GetItemType() == ItemStateMachine.ItemType.SETPART)
        {
            if (Input.GetButtonDown("Attack") && item.GetPlayerData() != null && item.GetPlayerData().GetPart() == null ||
                item.GetPlayerData() != null && item.GetPlayerData().GetPrevShotButton() == 0.0f && Input.GetAxisRaw("Shot") != 0.0f && item.GetPlayerData().GetPart() == null)
            {
                item.GetPlayerData().SetPrevShotButton(Input.GetAxisRaw("Shot"));
                item.CmdChangeState(item, ItemStateMachine.ItemStateType.PARTEQUIPPED);
            }
        }
    }

    public void OnTriggerEnter(GameObject other)
    {
        Debug.Log("hit");
        if(other.tag != "Player") { return; }



        Player_Tanabe player = other.GetComponent<Player_Tanabe>();

        if (player == null ||
            item.GetItemType() != ItemStateMachine.ItemType.POINT && item.GetItemType() != ItemStateMachine.ItemType.SETPART && player.GetPossesionManager().IsMaxPossession())
        {
            return;
        }

        item.SetPlayerData(player);

        if(item.GetItemType() != ItemStateMachine.ItemType.SETPART)
        {
            item.CmdChangeState(item, ItemStateMachine.ItemStateType.SUCK);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (item.GetItemType() != ItemStateMachine.ItemType.SETPART) { return; }

        item.SetPlayerData(null);
    }

    public void OnCollisionExit(Collider other)
    {
    }

    public void Exit()
    {
        item.transform.localScale = baseScale;
        Debug.Log("Drop:èIóπ");
    }
}
