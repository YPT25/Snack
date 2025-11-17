using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect_Tanabe : MonoBehaviour
{
    [Header("このエフェクトの表示時間"), SerializeField, Range(0f, 10f)]
    private float RENDERTIME;
    private float m_renderTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_renderTimer -= Time.deltaTime;
        if(m_renderTimer <= 0f)
        {

        }
    }
}
