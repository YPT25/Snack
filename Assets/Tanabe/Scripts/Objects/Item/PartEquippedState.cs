using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartEquippedState : IItemState_Tanabe
{
    private ItemStateMachine item;
    private Vector3 baseScale;

    private float m_removeEquippedTimer = 1.0f;

    public PartEquippedState(ItemStateMachine item)
    {
        this.item = item;
    }

    public void Enter()
    {
        baseScale = item.transform.localScale;
        item.transform.localScale = baseScale * 0.3f;

        item.SetUseGravity(false);
        item.SetIsKinematic(true);

        item.transform.rotation = item.GetPlayerTransform().rotation;

        item.transform.parent = item.GetPlayerTransform();
        item.transform.localPosition = new Vector3(0.6f, 0.0f, -0.4f);

        item.GetPlayerData().SetPart(item.GetComponent<SetPart_Tanabe>());

        Debug.Log("PartEquipped:ŠJŽn");
    }

    public void Update()
    {
        if(Input.GetKeyUp(KeyCode.V) || Input.GetKeyUp("joystick button 4"))
        {
            m_removeEquippedTimer = 1.0f;
        }
        else if(Input.GetKey(KeyCode.V) || Input.GetKey("joystick button 4"))
        {
            m_removeEquippedTimer -= Time.deltaTime;
            if(m_removeEquippedTimer <= 0.0f)
            {
                item.GetPlayerData().SetPart(null);
                item.ChangeState(new DropState(item));

                Vector3 moveVector = new Vector3((float)Random.Range(-10, 11) * 0.1f, 3.0f, (float)Random.Range(-10, 11) * 0.1f);
                item.GetRigidbody().AddForce(moveVector.normalized * 5.0f, ForceMode.Impulse);
            }
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
        Debug.Log("PartEquipped:I—¹");
    }
}
