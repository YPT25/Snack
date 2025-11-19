using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Check_dorp_idle2_Tanabe : NetworkBehaviour
{
    [Header("オブジェクトの移動関連")]
    [Tooltip("オブジェクトのスピード")]
    public float WaveSpeed = 1.0f;

    [Tooltip("オブジェクトの高さ")]
    public float WaveHeight = 0.1f;

    // 基準の高さ
    [Tooltip("基準位置")]
    float baseY = 0.5f;

    float m_time = 0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ItemStateMachine itemStateMachine = GetComponent<ItemStateMachine>();
        if (itemStateMachine.GetItemStateType() == ItemStateMachine.ItemStateType.DROP)
        {
            m_time += Time.deltaTime;
            float newY = transform.position.y + Mathf.Sin(m_time/*Time.time*/ * WaveSpeed) * WaveHeight;

            //transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            Rigidbody rb = this.GetComponent<Rigidbody>();
            rb.AddForce(transform.up * (Mathf.Sin(m_time/*Time.time*/ * WaveSpeed) * WaveHeight));
        }
    }
}
