using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPerformance : MonoBehaviour
{
    [SerializeField] private Transform m_performanceItem;
    [SerializeField, Range(0f, 100f)] private float m_rotateSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localEulerAngles = m_performanceItem.transform.eulerAngles;
        localEulerAngles.y += m_rotateSpeed * Time.deltaTime;
        m_performanceItem.transform.rotation = Quaternion.Euler(localEulerAngles);
    }
}
