using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsState : IItemState_Tanabe
{
    private ItemStateMachine item;
    private Vector3 baseScale;

    public HandsState(ItemStateMachine item)
    {
        this.item = item;
    }

    public void Enter()
    {
        baseScale = item.transform.localScale;
        if (item.GetItemType() != ItemStateMachine.ItemType.TRAP && item.GetItemType() != ItemStateMachine.ItemType.THROW)
        {
            item.transform.localScale = baseScale * 0.3f;
        }

        item.transform.rotation = item.GetPlayerTransform().rotation;

        item.transform.parent = item.GetPlayerTransform();
        item.transform.localPosition = new Vector3(-0.6f, 0.0f, 0.8f);

        item.GetPlayerData().GetPossesionManager().AddItem(item);

        Debug.Log("Hands:äJén");
    }

    public void Update()
    {
        item.transform.localPosition = new Vector3(-0.6f, 0.0f, 0.8f);
    }

    public void OnTriggerEnter(GameObject other)
    {
    }

    public void OnTriggerExit(Collider other)
    {
    }

    public void OnCollisionExit(Collider other)
    {
    }

    public void Exit()
    {
        item.SetUseGravity(true);
        item.transform.localScale = baseScale;
        item.transform.parent = null;
        Debug.Log("Hands:èIóπ");
    }
}
