using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScale : MonoBehaviour
{
    private Transform[] m_childObject;
    // Start is called before the first frame update
    void Start()
    {
        m_childObject = this.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < m_childObject.Length; i++)
        {
            m_childObject[i].localScale = this.transform.localScale;
        }
    }
}
