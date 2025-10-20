using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl_Tanabe : NetworkBehaviour
{
    [SyncVar] private Color m_color;

    private Rigidbody m_rb;
    private Vector3 m_moveDirection = Vector3.zero;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        m_color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));

        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.isLocalPlayer)
        {
            m_moveDirection = Vector3.zero;
            m_moveDirection.x = Input.GetAxisRaw("Horizontal");
            m_moveDirection.z = Input.GetAxisRaw("Vertical");
        }
    }

    private void FixedUpdate()
    {
        this.gameObject.GetComponent<Renderer>().material.color = m_color;
        if(this.isLocalPlayer)
        {
            CmdMoveSphere();
        }
    }

    [Command]
    private void CmdMoveSphere()
    {
        Vector3 v = m_moveDirection * 5f;
        m_rb.AddForce(v);
    }
}
