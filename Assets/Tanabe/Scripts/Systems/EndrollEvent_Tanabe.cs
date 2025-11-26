using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndrollEvent_Tanabe : MonoBehaviour
{
    [SerializeField] private Image m_image;
    [SerializeField] private Sprite[] m_sprites;

    // Update is called once per frame
    void Update()
    {
        if(m_image == null) { return; }

        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            m_image.sprite = null;
            m_image.color = Color.black;
        }

        if(!Input.GetKey(KeyCode.P)) { return; }

        for(int i = 0; i < m_sprites.Length; i++)
        {
            if(Input.GetKeyDown((KeyCode)(49 + i)))
            {
                m_image.sprite = m_sprites[i];
                m_image.color = Color.white;
            }
        }
    }
}
