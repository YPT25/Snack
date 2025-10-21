using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SuckState : IItemState_Tanabe
{
    private ItemStateMachine item;
    private Vector3 baseScale;

    public SuckState(ItemStateMachine item)
    {
        this.item = item;
    }

    public void Enter()
    {
        item.SetUseGravity(false);
        item.SetIsKinematic(true);
        baseScale = item.transform.localScale;
        if (item.GetItemType() != ItemStateMachine.ItemType.TRAP && item.GetItemType() != ItemStateMachine.ItemType.THROW)
        {
            item.transform.localScale = baseScale * 0.3f;
        }
        //item.GetComponent<BoxCollider>().isTrigger = true;
        item.GetColiider().isTrigger = true;
        Debug.Log("Suck:開始");
    }

    public void Update()
    {
        if (item.GetItemType() != ItemStateMachine.ItemType.POINT && item.GetPlayerData() == null ||
            item.GetItemType() != ItemStateMachine.ItemType.POINT && item.GetPlayerData().GetPossesionManager().IsMaxPossession())
        {
            // ドロップ状態に遷移する
            item.ServerChangeState(item, ItemStateMachine.ItemStateType.DROP);
            return;
        }

        Transform playerTransform = item.GetPlayerTransform();
        if(playerTransform == null) { return; }


        Vector3 localEulerAngles = item.transform.eulerAngles;
        localEulerAngles.y += item.rotateSpeed * Time.deltaTime;
        item.transform.rotation = Quaternion.Euler(localEulerAngles);

        Vector3 localPosition = item.transform.position;
        Vector3 vec = playerTransform.localPosition - localPosition;
        localPosition += vec.normalized * item.moveSpeed * Time.deltaTime;
        item.transform.position = localPosition;

        if(Vector3.Distance(playerTransform.localPosition, localPosition) < 0.2f)
        {
            // これがポイントアイテムなら通す
            if(item.GetItemType() == ItemStateMachine.ItemType.POINT)
            {
                item.ServerAddPoint();
                //// ポイントに変換する
                //item.GetPlayerData().AddPoint(item.GetPoint());
                //// このオブジェクトを破棄する
                //item.DestroysGameObject();
            }
            else
            {
                // プレイヤーに所持させる
                item.ServerChangeState(item, ItemStateMachine.ItemStateType.HANDS);
            }
        }
    }

    public void OnTriggerEnter(GameObject other)
    {
    }

    public void OnTriggerExit(Collider other)
    {
        if(item.GetPlayerData() == other.gameObject.GetComponent<Player_Tanabe>())
        {
            item.RpcSetPlayerData(null);
            // ドロップ状態に遷移させる
            item.ServerChangeState(item, ItemStateMachine.ItemStateType.DROP);
        }
    }

    public void OnCollisionExit(Collider other)
    {
    }

    public void Exit()
    {
        item.transform.localScale = baseScale;
        Debug.Log("Suck:終了");
    }
}
