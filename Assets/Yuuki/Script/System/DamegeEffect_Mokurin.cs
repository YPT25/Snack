using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamegeEffect_Mokurin : MonoBehaviour
{
    [Header("点滅させる MeshRenderer（複数可）")]
    [SerializeField] private MeshRenderer[] m_meshRenderers;

    [Header("ヒット時の継続時間")]
    [SerializeField] private float HITTIME = 0.2f;

    [Header("ダメージ時の色")]
    [SerializeField] private Color m_damageColor = Color.red;

    private float m_hitTimer = 0f;

    private Color[] m_defaultColors;

    void Start()
    {
        // ▼Inspectorで設定されていなければ、自動で子階層からRenderer取得
        if (m_meshRenderers == null || m_meshRenderers.Length == 0)
        {
            m_meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        // ▼元の色を保存
        m_defaultColors = new Color[m_meshRenderers.Length];
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_defaultColors[i] = m_meshRenderers[i].material.color;
        }
    }

    void Update()
    {
        if (m_hitTimer > 0f)
        {
            m_hitTimer -= Time.deltaTime;

            bool flash = ((int)(m_hitTimer * 10f) % 2 == 0);

            ChangeColor(flash ? m_damageColor : m_defaultColors[0]);

            if (m_hitTimer <= 0f)
            {
                ResetColor();
            }
        }
    }

    public void Damage()
    {
        m_hitTimer = HITTIME;
        ChangeColor(m_damageColor);
    }

    private void ChangeColor(Color _color)
    {
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].material.color = _color;
        }
    }

    private void ResetColor()
    {
        for (int i = 0; i < m_meshRenderers.Length; i++)
        {
            m_meshRenderers[i].material.color = m_defaultColors[i];
        }
    }
}