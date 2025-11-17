using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTarget_Tanabe : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private float HITTIME;
    private float m_hitTimer = 0.7f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
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

    private void OnTriggerEnter(Collider other)
    {
        m_meshRenderer.material.color = Color.green;
        m_hitTimer = HITTIME;
    }
}
