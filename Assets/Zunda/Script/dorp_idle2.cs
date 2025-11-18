using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class dorp_idle2 : NetworkBehaviour
{
    [Header("オブジェクトの移動関連")]
    [Tooltip("オブジェクトのスピード")]
    public float WaveSpeed = 1.0f;

    [Tooltip("オブジェクトの高さ")]
    public float WaveHeight = 0.1f;

    // 基準の高さ
    [Tooltip("基準位置")]
    float baseY = 0.5f;

    ItemStateMachine itemStateMachine;

    // Start is called before the first frame update
    void Start()
    {
        itemStateMachine = FindObjectOfType<ItemStateMachine>();
    }

    // Update is called once per frame
    void Update()
    {
        if (itemStateMachine.GetItemStateType() == ItemStateMachine.ItemStateType.DROP)
        {
            float newY = baseY + Mathf.Sin(Time.deltaTime * WaveSpeed) * WaveHeight;

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
