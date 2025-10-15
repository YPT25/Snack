using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TrapState : IItemState_Tanabe
{
    private ItemStateMachine item;
    private Vector3 baseScale;

    private float m_trapInterval = 3.0f;
    private float m_deleteTimer = 2.0f;

    private List<CharacterBase> m_characters = new List<CharacterBase>();

    public TrapState(ItemStateMachine item)
    {
        this.item = item;
    }

    public void Enter()
    {
        baseScale = item.transform.localScale;
        if(item.GetItemType() == ItemStateMachine.ItemType.TRAP)
        {
            Vector3 playerEuler = item.GetPlayerData().transform.eulerAngles;
            item.transform.rotation = Quaternion.Euler(playerEuler.x, playerEuler.y, playerEuler.z + 90f);
            //item.transform.localScale = new Vector3(baseScale.x * 6.0f, 2.0f, baseScale.z * 6.0f);
            item.transform.localScale = new Vector3(6.0f, baseScale.y * 9.0f, baseScale.z * 9.0f);
            Vector3 extents = item.GetComponent<BoxCollider>().size;
            extents.x = 0.001f;
            item.GetComponent<BoxCollider>().size = extents;
            item.GetComponent<BoxCollider>().center = new Vector3(0f, 0.17f, 0f);
            item.transform.transform.position = item.transform.position + new Vector3(0f, -1f, 0f);
            m_deleteTimer = 15.0f;
        }
        else
        {
            m_deleteTimer = 12.0f;
            item.transform.localScale = new Vector3(baseScale.x, baseScale.y * 0.05f, baseScale.z);
        }

        item.SetIsKinematic(false);
        item.SetUseGravity(true);
        item.GetRigidbody().freezeRotation = true;
        item.GetComponent<BoxCollider>().isTrigger = false;

        item.gameObject.layer = 0;

        Debug.Log("Trap:開始");
    }

    public void Update()
    {
        if(item.GetItemType()== ItemStateMachine.ItemType.TRAP)
        {
            Update_Normal();
        }
        else
        {
            Update_Bomb();
        }
    }

    private void Update_Normal()
    {
        Vector3 velocity = item.GetRigidbody().velocity;
        velocity.x = 0.0f;
        velocity.z = 0.0f;
        if (velocity.y > 0.0f)
        {
            velocity.y = 0.0f;
        }
        item.GetRigidbody().velocity = velocity;

        if (m_trapInterval > 0.0f)
        {
            m_trapInterval -= Time.deltaTime;
            return;
        }

        m_deleteTimer -= Time.deltaTime;
        if (m_deleteTimer <= 0)
        {
            item.DestroysGameObject();
        }
    }

    private void Update_Bomb()
    {
        if (m_trapInterval > 0.0f)
        {
            m_trapInterval -= Time.deltaTime;
            if (m_trapInterval <= 0.0f)
            {
                item.GetRigidbody().mass = 999f;
            }
            return;
        }

        Vector3 scale = item.transform.localScale;
        scale += Vector3.one * 1.3f * Time.deltaTime;
        item.transform.localScale = scale;
        m_deleteTimer -= Time.deltaTime;
        if (m_deleteTimer <= 0)
        {
            item.DestroysGameObject();
        }
        else if(m_deleteTimer <= 2.0f && m_deleteTimer + Time.deltaTime >= 2.0f && !item.GetRigidbody().isKinematic)
        {
            this.Explode();
        }

        //if (!item.GetRigidbody().isKinematic) { return; }
        //m_deleteTimer -= Time.deltaTime;
        //if (m_deleteTimer <= 0)
        //{
        //    item.DestroysGameObject();
        //}
    }

    public void OnTriggerEnter(Collider other)
    {
        if(m_trapInterval > 0.0f || item.GetRigidbody().isKinematic) { return; }

        if(item.GetItemType() != ItemStateMachine.ItemType.TRAP_BOMB)
        {
            CharacterBase character = other.gameObject.GetComponent<CharacterBase>();
            if (character != null)
            {
                if(!m_characters.Contains(character))
                {
                    m_characters.Add(character);
                    character.SetIsMove(false);
                }
            }
            else if(other.gameObject.GetComponent<Rigidbody>() != null)
            {
                Vector3 velocity = other.gameObject.GetComponent<Rigidbody>().velocity;
                velocity = velocity * 0.3f;
                other.gameObject.GetComponent<Rigidbody>().velocity = velocity;
            }
        }
        //一時停止
        else if (other.gameObject.GetComponent<Bullet_Tanabe>() != null/* || other.GetComponent<DebugAttacker>() != null*/ || other.GetComponent<ShockWave_Tanabe>() != null)
        {
            this.Explode();
        }
    }

    // 爆発処理
    private void Explode()
    {
        Vector3 scale = item.transform.localScale;

        item.SetIsKinematic(true);
        item.GetEffectObject().SetActive(true);
        item.transform.localScale = Vector3.one * scale.x * 0.1f;
        item.gameObject.GetComponent<MeshRenderer>().enabled = false;
        item.GetComponent<BoxCollider>().enabled = false;

        BombExplosion_Tanabe test = item.GetComponent<BombExplosion_Tanabe>();
        if (test != null)
        {
            test.Explode(3f + scale.x * 0.5f, 5f * scale.x * 0.3f, scale.x * 0.00001f);
            m_deleteTimer = 1.9f * ((10f - (m_deleteTimer - 2f)) / 10f);
        }
    }

    public void OnTriggerExit(Collider other)
    {
    }

    public void OnCollisionExit(Collider other)
    {
        if(m_trapInterval > 0.0f || item.GetRigidbody().isKinematic) { return; }

        if(item.GetItemType() != ItemStateMachine.ItemType.TRAP_BOMB)
        {
            CharacterBase character = other.gameObject.GetComponent<CharacterBase>();
            if (character != null)
            {
                if (m_characters.Contains(character))
                {
                    character.SetIsMove(true);
                    Rigidbody charcterRB = character.gameObject.GetComponent<Rigidbody>();
                    if (charcterRB != null)
                    {
                        Vector3 characterVelocity = charcterRB.velocity;
                        characterVelocity = characterVelocity * 0.3f;
                        charcterRB.velocity = characterVelocity;
                    }
                    m_characters.Remove(character);
                }
            }
        }
    }

    public void Exit()
    {
        if(item.GetItemType() == ItemStateMachine.ItemType.TRAP)
        {
            for(int i = 0; i < m_characters.Count; i++)
            {
                if (m_characters[i] == null) { continue; }
                m_characters[i].SetIsMove(true);
            }
        }
        item.transform.localScale = baseScale;
        Debug.Log("Trap:終了");
    }
}
