using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest2D_Tanabe : MonoBehaviour
{
    [SerializeField, Range(0f, 500f)] private float m_power;
    private Rigidbody2D m_rb;
    [SerializeField] private float COUNTTIME = 2f;
    [SerializeField] private float m_timer = 1f;
    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        m_timer -= Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.Space) || m_timer <= 0f)
        {
            m_timer = COUNTTIME;
            m_rb.AddForce(Vector3.up * m_power, ForceMode2D.Impulse);
            m_rb.angularVelocity = Random.Range(-15f, 15f);
        }
    }
}
