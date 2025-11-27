using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndrollEvent_Tanabe : MonoBehaviour
{
    [SerializeField] private Image m_image;
    [SerializeField] private Sprite[] m_sprites;
    [SerializeField] private Material m_colorfulMaterial;
    private int m_commandCount = 0;
    private string m_commandName = "colorful";
    private int[] m_commandIndex = { (int)KeyCode.C, (int)KeyCode.O, (int)KeyCode.L, (int)KeyCode.O, (int)KeyCode.R, (int)KeyCode.F, (int)KeyCode.U, (int)KeyCode.L };

    // Update is called once per frame
    void Update()
    {
        if(m_image == null) { return; }

        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            m_image.sprite = null;
            m_image.color = Color.black;
            m_commandCount = 0;
            m_image.material = null;
        }

        if(m_commandCount < m_commandName.Length)
        {
            InputCommand();
        }

        if (!Input.GetKey(KeyCode.P) || !Input.GetKey(KeyCode.T)) { return; }

        for(int i = 0; i < m_sprites.Length; i++)
        {
            if(Input.GetKeyDown((KeyCode)(49 + i)))
            {
                m_image.sprite = m_sprites[i];
                m_image.color = Color.white;
            }
        }
    }

    private void InputCommand()
    {
        for (int i = 97; i < 123; i++)
        {
            if(Input.GetKeyDown((KeyCode)(i)) && i == m_commandIndex[m_commandCount])
            {
                m_commandCount++;
            }
            else if (Input.GetKeyDown((KeyCode)(i)))
            {
                m_commandCount = 0;

                if (i == m_commandIndex[0])
                {
                    m_commandCount = 1;
                }
            }
        }

        if(m_commandCount == m_commandName.Length)
        {
            m_image.material = m_colorfulMaterial;
        }
    }
}
