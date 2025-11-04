using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOption_Tanabe : MonoBehaviour
{
    private bool m_isChanged = false;
    [Header("オプションUI"), SerializeField] private GameObject m_gameOptionUI;
    [Header("カメラ感度のバー"), SerializeField] private Scrollbar m_scrollbarUI;

    // Start is called before the first frame update
    void Start()
    {
        if (m_gameOptionUI == null) { return; }
        m_gameOptionUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_gameOptionUI == null) { return; }

        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown("joystick button 7"))
        {
            Cursor.lockState = (CursorLockMode)(Math.Abs((int)Cursor.lockState - 1));
            m_gameOptionUI.SetActive(!m_gameOptionUI.active);
            m_isChanged = !m_gameOptionUI.active;
        }
    }

    public bool IsPause()
    {
        return m_gameOptionUI.active;
    }

    public bool IsChanged()
    {
        if(m_isChanged)
        {
            m_isChanged = false;
            return true;
        }
        return m_isChanged;
    }

    public float GetCameraSensitivity()
    {
        float sensitivityPower = m_scrollbarUI.value;
        if (sensitivityPower < 0.1f)
        {
            sensitivityPower = 0.1f;
        }
        return sensitivityPower * 10f;
    }
}
