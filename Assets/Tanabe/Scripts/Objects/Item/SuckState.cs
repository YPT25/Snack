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
        Debug.Log("Suck:�J�n");
    }

    public void Update()
    {
        if (item.GetItemType() != ItemStateMachine.ItemType.POINT && item.GetPlayerData() == null ||
            item.GetItemType() != ItemStateMachine.ItemType.POINT && item.GetPlayerData().GetPossesionManager().IsMaxPossession())
        {
            // �h���b�v��ԂɑJ�ڂ���
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
            // ���ꂪ�|�C���g�A�C�e���Ȃ�ʂ�
            if(item.GetItemType() == ItemStateMachine.ItemType.POINT)
            {
                item.ServerAddPoint();
                //// �|�C���g�ɕϊ�����
                //item.GetPlayerData().AddPoint(item.GetPoint());
                //// ���̃I�u�W�F�N�g��j������
                //item.DestroysGameObject();
            }
            else
            {
                // �v���C���[�ɏ���������
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
            // �h���b�v��ԂɑJ�ڂ�����
            item.ServerChangeState(item, ItemStateMachine.ItemStateType.DROP);
        }
    }

    public void OnCollisionExit(Collider other)
    {
    }

    public void Exit()
    {
        item.transform.localScale = baseScale;
        Debug.Log("Suck:�I��");
    }
}
