using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticeBoard_Tanabe : MonoBehaviour
{
    private readonly float STARTPOSITION_Y = 240f;
    private readonly float ENDPOSITION_Y = -115f;

    [Header("お知らせ画像"), SerializeField] private Sprite[] m_sprites;
    [Header("お知らせボードの移動速度"), SerializeField] private float m_speed;
    [Header("画像の切り替える間隔"), SerializeField] private float CHANGETIME;
    [Header("アニメーション画像の切り替える間隔"), SerializeField] private float ANIMCHANGETIME;
    [Header("アニメーションする画像の開始番号"), SerializeField] private int m_animStartTexNumber;
    [Header("アニメーションする画像の終了番号"), SerializeField] private int m_animEndTexNumber;
    private int m_currentTexNumber = 0;
    private RectTransform m_boardTransform;
    private Image m_board;
    private bool m_isStart = false;

    private bool m_isStop = true;

    private bool m_isFall = false;
    private bool m_isUp = false;
    private bool m_isPerformance = false;

    private float m_moveTimer = 0f;

    private float m_changeTimer = 3f;


    // Start is called before the first frame update
    void Start()
    {
        m_boardTransform = GetComponent<RectTransform>();
        m_board = GetComponent<Image>();
        m_changeTimer = CHANGETIME;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isStart) { return; }

        if(!m_isStop)
        {
            if(m_isFall) { this.AnimFall(); }
            else if(m_isUp) { this.AnimUp(); }
        }
        else if(m_isPerformance)
        {
            this.AnimPerformance();
        }
    }

    private void AnimFall()
    {
        m_moveTimer = Mathf.Min(m_moveTimer + m_speed * Time.deltaTime, 1f);

        Vector3 pos = m_boardTransform.anchoredPosition;
        pos.y = Mathf.LerpUnclamped(STARTPOSITION_Y, ENDPOSITION_Y, this.Easing(m_moveTimer));
        m_boardTransform.anchoredPosition = pos;

        if (m_moveTimer >= 1f)
        {
            m_moveTimer = 0f;
            m_isFall = false;
            m_isStop = true;
            m_isPerformance = true;
        }
    }

    private void AnimUp()
    {
        m_moveTimer = Mathf.Min(m_moveTimer + m_speed * Time.deltaTime, 1f);

        Vector3 pos = m_boardTransform.anchoredPosition;
        pos.y = Mathf.LerpUnclamped(ENDPOSITION_Y, STARTPOSITION_Y, this.Easing(m_moveTimer));
        m_boardTransform.anchoredPosition = pos;

        if (m_moveTimer >= 1f)
        {
            m_moveTimer = 0f;
            m_isUp = false;
            m_isStop = true;
            m_isPerformance = false;
            m_isStart = false;
        }
    }

    private float Easing(float _time)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        return _time < 0.5f
          ? (Mathf.Pow(2f * _time, 2) * ((c2 + 1f) * 2f * _time - c2)) / 2f
          : (Mathf.Pow(2f * _time - 2f, 2) * ((c2 + 1f) * (_time * 2f - 2f) + c2) + 2f) / 2f;
    }

    private void AnimPerformance()
    {
        if(m_changeTimer > 0f)
        {
            m_changeTimer -= Time.deltaTime;
            return;
        }

        m_currentTexNumber++;

        if(m_currentTexNumber >= m_sprites.Length)
        {
            m_isPerformance = false;
            m_isStop = false;
            m_isUp = true;
            return;
        }

        if (m_currentTexNumber >= m_animStartTexNumber &&
            m_currentTexNumber <= m_animEndTexNumber)
        {
            m_changeTimer = ANIMCHANGETIME;
        }
        else
        {
            m_changeTimer = CHANGETIME;
        }

        m_board.sprite = m_sprites[m_currentTexNumber];


    }

    public void SetIsStart(bool _flag)
    {
        if(m_isStart) { return; }

        m_isStart = _flag;
        if(_flag)
        {
            m_isFall = true;
            m_isStop = false;
            m_board.sprite = m_sprites[0];
            m_currentTexNumber = 0;
        }
    }
}
