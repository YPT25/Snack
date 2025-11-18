using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect_Tanabe : MonoBehaviour
{
    [Header("このエフェクトの表示時間"), SerializeField, Range(0f, 10f)]
    private float RENDERTIME;
    private float m_renderTimer = 0f;
    private bool m_isDestroy = false;

    // Start is called before the first frame update
    void Start()
    {
        m_renderTimer = RENDERTIME;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_isDestroy) { return; }
        m_renderTimer -= Time.deltaTime;
        if(m_renderTimer <= 0f)
        {
            Destroy(this.gameObject);
            m_isDestroy = true;
        }
    }
}
