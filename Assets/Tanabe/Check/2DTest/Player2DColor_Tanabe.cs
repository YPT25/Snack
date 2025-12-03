using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player2DColor_Tanabe : MonoBehaviour
{
    [SerializeField, Range(0, 9)] private int m_color;

    [SerializeField] private SpriteRenderer m_headImage;
    [SerializeField] private SpriteRenderer m_bodyImage;

    [SerializeField] private Sprite[] m_head = new Sprite[10];
    [SerializeField] private Sprite[] m_body = new Sprite[10];

    // Start is called before the first frame update
    void Start()
    {
        m_headImage.sprite = m_head[m_color];
        m_bodyImage.sprite = m_body[m_color];
    }

    private void Update()
    {
        m_headImage.sprite = m_head[m_color];
        m_bodyImage.sprite = m_body[m_color];
    }
}
