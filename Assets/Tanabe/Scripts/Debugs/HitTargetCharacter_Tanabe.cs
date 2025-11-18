using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTargetCharacter_Tanabe : CharacterBase
{
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private float HITTIME;
    [SerializeField] private int m_characterTypeNum;
    private float m_hitTimer = 0.7f;

    // Start is called before the first frame update
    public override void OnStartClient()
    {
        base.OnStartClient();
        base.SetCharacterType((CharacterType)m_characterTypeNum);
    }

    // Update is called once per frame
    public override void Update()
    {
        if(m_hitTimer > 0f)
        {
            m_hitTimer -= Time.deltaTime;
            if(m_hitTimer <= 0f)
            {
                m_meshRenderer.material.color = Color.white;
            }
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        m_meshRenderer.material.color = Color.green;
        m_hitTimer = HITTIME;
        RpcHit();
    }

    [ClientRpc]
    private void RpcHit()
    {
        m_meshRenderer.material.color = Color.green;
        m_hitTimer = HITTIME;
    }
}
