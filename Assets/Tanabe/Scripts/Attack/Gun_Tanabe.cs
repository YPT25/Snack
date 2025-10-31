using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Mirror;

public class Gun_Tanabe : NetworkBehaviour
{
    private Player_Tanabe m_player;
    [SerializeField] private GameObject m_bulletPrefab;
    [SerializeField] GameObject m_gunHead;
    private float m_interval = 0.0f;
    private float m_maxInterval = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        m_player = GetComponentInParent<Player_Tanabe>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_player.isLocalPlayer) { return; }
        if(m_player.GetHp() <= 0.0f) { return; }

        if (m_player.GetIsAiming())
        {
            // ÉvÉåÉCÉÑÅ[ÇÃYâÒì]ÇÉJÉÅÉâÇÃYâÒì]Ç…çáÇÌÇπÇÈ
            Vector3 camForward = m_player.GetCameraForward();
            if (camForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForward.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10.0f * Time.deltaTime);
            }
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }

        if (m_interval > 0.0f)
        {
            m_interval -= Time.deltaTime;
            return;
        }

        if (Input.GetButtonDown("Attack") || Input.GetAxisRaw("Shot") != 0.0f)
        {
            if (m_player.GetIsThrow() && m_player.GetRightHandsItem() != null)
            {
                CmdChangeState_Item(m_player.GetRightHandsItem(), ItemStateMachine.ItemStateType.THROW);
                m_player.SetRightHandsItem(null);
                m_player.SetIsThrow(false);
                m_interval = 0.2f;
            }
            else
            {
                switch (m_player.GetPartType())
                {
                    case global::SetPart_Tanabe.PartType.NONE_TYPE:
                        {
                            this.CmdShot();
                            m_interval = m_maxInterval;
                            break;
                        }
                    case global::SetPart_Tanabe.PartType.LONGBARREL:
                        {
                            this.CmdShotGun();
                            m_interval = 2.0f;
                            break;
                        }
                    case global::SetPart_Tanabe.PartType.SHARPBULLET:
                        {
                            this.CmdSharpShot();
                            m_interval = 1.0f;
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }

    // í èÌíe
    [Command]
    private void CmdShot()
    {
        //GameObject obj = m_bulletPrefab;
        //Instantiate(obj).GetComponent<Bullet_Tanabe>().Shot(this.GetComponentInParent<Player_Tanabe>().GetPower(), m_gunHead.transform);
        //m_interval = m_maxInterval;

        GameObject obj = Instantiate(m_bulletPrefab);
        obj.GetComponent<Bullet_Tanabe>().Shot(m_player.GetPower(), m_gunHead.transform);
        NetworkServer.Spawn(obj);
    }

    // ÉVÉáÉbÉgÉKÉì
    [Command]
    private void CmdShotGun()
    {
        int bulletCount = 10;
        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 moveVector3 = m_gunHead.transform.forward * 5.0f + new Vector3(GetRandomPoint(), GetRandomPoint(), GetRandomPoint()).normalized;

            GameObject obj = Instantiate(m_bulletPrefab);
            obj.GetComponent<Bullet_Tanabe>().ShotGun(m_player.GetPower(), m_gunHead.transform, moveVector3.normalized);
            NetworkServer.Spawn(obj);
        }
    }

    // êÎíe
    [Command]
    private void CmdSharpShot()
    {
        GameObject obj = Instantiate(m_bulletPrefab);
        obj.GetComponent<Bullet_Tanabe>().SharpShot(m_player.GetPower(), m_gunHead.transform);
        NetworkServer.Spawn(obj);
    }

    private float GetRandomPoint()
    {
        return (float)Random.Range(-10, 11) * 0.1f;
    }

    // èeå˚ÇÃéÊìæ
    public Transform GetGunHead()
    {
        return m_gunHead.transform;
    }

    // ÉAÉCÉeÉÄÇÃèÛë‘ëJà⁄
    [Command]
    public void CmdChangeState_Item(ItemStateMachine _item, ItemStateMachine.ItemStateType _newStateType)
    {
        // ThrowèÛë‘Ç…ëJà⁄Ç∑ÇÈ
        _item.RpcChangeState(_item, _newStateType);
    }
}
