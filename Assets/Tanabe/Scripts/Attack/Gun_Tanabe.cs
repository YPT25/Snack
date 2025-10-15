using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gun_Tanabe : MonoBehaviour
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
            if (m_player.GetIsThrow())
            {
                m_player.SetIsThrow(false);
                m_interval = 0.2f;
            }
            else
            {
                switch (m_player.GetPartType())
                {
                    case global::SetPart_Tanabe.PartType.NONE_TYPE:
                        {
                            this.Shot();
                            break;
                        }
                    case global::SetPart_Tanabe.PartType.LONGBARREL:
                        {
                            this.ShotGun();
                            break;
                        }
                    case global::SetPart_Tanabe.PartType.SHARPBULLET:
                        {
                            this.SharpShot();
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }

    // í èÌíe
    private void Shot()
    {
        GameObject obj = m_bulletPrefab;
        Instantiate(obj).GetComponent<Bullet_Tanabe>().Shot(this.GetComponentInParent<Player_Tanabe>().GetPower(), m_gunHead.transform);
        m_interval = m_maxInterval;
    }

    // ÉVÉáÉbÉgÉKÉì
    private void ShotGun()
    {
        int bulletCount = 10;
        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 moveVector3 = m_gunHead.transform.forward * 5.0f + new Vector3(GetRandomPoint(), GetRandomPoint(), GetRandomPoint()).normalized;

            GameObject obj = m_bulletPrefab;
            Instantiate(obj).GetComponent<Bullet_Tanabe>().ShotGun(this.GetComponentInParent<Player_Tanabe>().GetPower(), m_gunHead.transform, moveVector3.normalized);
        }
        m_interval = 2.0f;
    }

    // êÎíe
    private void SharpShot()
    {
        GameObject obj = m_bulletPrefab;
        Instantiate(obj).GetComponent<Bullet_Tanabe>().SharpShot(this.GetComponentInParent<Player_Tanabe>().GetPower(), m_gunHead.transform);
        m_interval = 1.0f;
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
}
