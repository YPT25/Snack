using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowState : IItemState_Tanabe
{
    private ItemStateMachine item;
    private Vector3 baseScale;

    private float m_deleteTimer = 2.0f;
    private float m_exitTimer = 10.0f;

    public ThrowState(ItemStateMachine item)
    {
        this.item = item;
    }

    public void Enter()
    {
        baseScale = item.transform.localScale;
        //item.transform.localScale = baseScale * 0.3f;

        item.transform.rotation = item.GetPlayerTransform().rotation;

        if(item.GetPlayerData().GetWeaponID() == Player_Tanabe.WeaponID.GUN && item.GetPlayerData().GetIsAiming())
        {
            Vector3 playerVec = item.GetPlayerData().GetGunHead().forward;
            item.GetRigidbody().AddForce(playerVec * 30.0f, ForceMode.Impulse);
            item.GetRigidbody().useGravity = false;
        }
        else
        {
            Vector3 playerVec = item.GetPlayerData().GetCameraForward();
            item.GetRigidbody().AddForce(playerVec * 15.0f, ForceMode.Impulse);
        }

        Debug.Log("Throw:äJén");
    }

    public void Update()
    {
        if(!item.GetRigidbody().isKinematic)
        {
            m_exitTimer -= Time.deltaTime;
            if(m_exitTimer <= 0.0f)
            {
                this.Explode();
            }
            return;
        }
        m_deleteTimer -= Time.deltaTime;
        if(m_deleteTimer <= 0)
        {
            item.DestroysGameObject();
            item.DestroysGameObject(item.GetEffectObject());
        }
    }

    public void OnTriggerEnter(GameObject other)
    {
        if (other.GetComponentInParent<Player_Tanabe>() != null || other.tag == "Player" || item.GetRigidbody().isKinematic) { return; }
        this.Explode();
    }

    private void Explode()
    {
        item.SetIsKinematic(true);
        item.GetEffectObject().SetActive(true);
        item.GetEffectObject().transform.parent = null;
        item.transform.localScale = Vector3.zero;
        item.GetColiider().enabled = false;
        //item.GetComponent<MeshRenderer>().enabled = false;

        BombExplosion_Tanabe test = item.GetComponent<BombExplosion_Tanabe>();
        if (test != null)
        {
            test.Explode(false, 2f, true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
    }

    public void OnCollisionExit(Collider other)
    {
    }

    public void Exit()
    {
        item.transform.localScale = baseScale;
        Debug.Log("Throw:èIóπ");
    }
}
