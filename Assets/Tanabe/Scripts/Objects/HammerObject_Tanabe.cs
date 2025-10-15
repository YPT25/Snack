using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerObject_Tanabe : MonoBehaviour
{
    [SerializeField] private Player_Tanabe m_player;
    [SerializeField] private GameObject m_hammerHead;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private MeshRenderer m_meshRenderer2;

    // Start is called before the first frame update
    void Start()
    {
        //m_meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player == null) { return; }
        m_meshRenderer.enabled = !m_player.GetIsThrow();
        m_meshRenderer2.enabled = !m_player.GetIsThrow();
        if (m_hammerHead != null)
        {
            m_hammerHead.GetComponent<MeshRenderer>().enabled = !m_player.GetIsThrow();
        }
    }

    public Vector3 GetPosition_HammerHead()
    {
        return m_hammerHead.transform.position;
    }
}
