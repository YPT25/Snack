using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePerformance : MonoBehaviour
{
    [Header("このオブジェクトのメッシュレンダラ"), SerializeField] private MeshRenderer[] m_meshRenderers;
    [Header("ヒット時のカウンタ"), SerializeField] private float HITTIME;
    [Header("ダメージ表示色"), SerializeField] private Color m_damageColor;
    private float m_hitTimer = 0f;
    private Color m_defaultColor;

    // Start is called before the first frame update
    void Start()
    {
        m_hitTimer = 0f;
        m_defaultColor = m_meshRenderers[0].material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_hitTimer > 0f)
        {
            m_hitTimer -= Time.deltaTime;

            if((int)(m_hitTimer * 10f) % 2 == 0)
            {
                this.ChangeColor(m_damageColor);
            }
            else
            {
                this.ChangeColor(m_defaultColor);
            }

            if (m_hitTimer <= 0f)
            {
                this.ChangeColor(m_defaultColor);
            }
        }
    }

    public void Damage()
    {
        m_hitTimer = HITTIME;

        this.ChangeColor(m_damageColor);
    }

    private void ChangeColor(Color _color)
    {
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].material.color = _color;
        }
    }
}
