using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand_Tanabe : MonoBehaviour
{
    private bool m_isHand = false;

    private Vector3 m_defaultPosition;

    // Start is called before the first frame update
    void Start()
    {
        m_defaultPosition = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetIsHand(bool _flag)
    {
        m_isHand = _flag;
        if (_flag)
        {
            this.transform.localPosition = m_defaultPosition + new Vector3(0f, 0f, 0.6f);
        }
        else
        {
            this.transform.localPosition = m_defaultPosition;
        }
    }
}
