using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump_Tanabe : MonoBehaviour
{
    [SerializeField, Range(0f, 500f)] private float m_power;
    private Rigidbody m_rb;
    [SerializeField] private float COUNTTIME = 2f;
    [SerializeField] private float m_timer = 1f;
    private Player_Tanabe m_player;
    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_player == null)
        {
            m_player = FindFirstObjectByType<Player_Tanabe>();
            return;
        }
        m_timer -= Time.deltaTime;
        if (m_timer <= 0f)
        {
            m_timer = COUNTTIME;
            Vector3 pos = m_player.transform.position - this.transform.position;
            Vector3 dir = pos.normalized;
            pos.y = 0f;
            this.transform.rotation = Quaternion.LookRotation(pos.normalized);
            m_rb.velocity = Vector3.zero;
            m_rb.AddForce(dir * m_power, ForceMode.Impulse);
            m_rb.AddForce(Vector3.up * m_power, ForceMode.Impulse);
            //m_rb.angularVelocity = Random.Range(-15f, 15f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Bullet_Tanabe bullet = other.GetComponent<Bullet_Tanabe>();
        if(bullet == null) { return; }
        
        Vector3 pos = this.transform.position - m_player.transform.position;
        pos.y = 0f;
        Vector3 dir = pos.normalized;
        //m_rb.velocity = Vector3.zero;

        m_rb.AddForce(dir * 5f, ForceMode.Impulse);
    }
}
