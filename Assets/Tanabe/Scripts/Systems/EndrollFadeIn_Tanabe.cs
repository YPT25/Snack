using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndrollFadeIn_Tanabe : MonoBehaviour
{
    [SerializeField] private Endroll_Tanabe m_endroll;
    [SerializeField] private Image[] m_fadeInImage;
    [SerializeField] private float m_fadeSpeed;
    private bool m_isFadeIn = false;

    // Update is called once per frame
    void Update()
    {
        if(m_endroll == null || !m_endroll.GetIsStopped()) { return; }

        if (m_isFadeIn)
        {
            for (int i = 0; i < m_fadeInImage.Length; i++)
            {
                Color color = m_fadeInImage[i].color;
                color.a += m_fadeSpeed * Time.deltaTime;
                color.a = Mathf.Min(Mathf.Max(color.a, 0f), 1f);
                m_fadeInImage[i].color = color;
            }
            if (m_fadeInImage[0].color.a <= 0f)
            {
                SceneManager.LoadScene("ConnectionScene");
            }
        }
        else if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Jump"))
        {
            m_isFadeIn = true;
        }
    }
}
