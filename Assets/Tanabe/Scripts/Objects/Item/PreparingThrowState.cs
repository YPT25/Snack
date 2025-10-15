using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreparingThrowState : IItemState_Tanabe
{
    private ItemStateMachine item;
    private Vector3 baseScale;
    private Transform m_gunHead = null;

    public PreparingThrowState(ItemStateMachine item)
    {
        this.item = item;
    }

    public void Enter()
    {
        baseScale = item.transform.localScale;
        //item.transform.localScale = baseScale * 0.3f;

        item.transform.rotation = item.GetPlayerTransform().rotation;

        item.transform.parent = item.GetPlayerTransform();
        item.transform.localPosition = new Vector3(0.6f, 0.0f, 0.8f);

        //item.GetPlayerData().GetPossesionManager().AddItem(item);

        Debug.Log("PreparingThrow:�J�n");
    }

    public void Update()
    {
        if(m_gunHead == null && item.GetPlayerData().GetWeaponID() == Player_Tanabe.WeaponID.GUN && item.GetPlayerData().GetIsAiming())
        {
            m_gunHead = item.GetPlayerData().GetGunHead();
        }

        if(m_gunHead != null)
        {
            item.transform.position = m_gunHead.position + m_gunHead.forward * 0.5f;
        }

        // ���N���b�N�����m������U���X�e�[�g�ɑJ�ڂ���
        if (Input.GetButtonDown("Attack") || Input.GetAxisRaw("Shot") != 0.0f)
        {
            // Throw��ԂɑJ�ڂ���
            item.ChangeState(new ThrowState(item));
        }
    }

    public void OnTriggerEnter(Collider other)
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
        item.SetIsKinematic(false);
        item.transform.localScale = baseScale;
        item.transform.parent = null;
        Debug.Log("PreparingThrow:�I��");
    }
}
