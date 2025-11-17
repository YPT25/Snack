using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunReticle_Tanabe : MonoBehaviour
{
    public enum ReticleType
    {
        NORMAL,
        POINT,
        CIRCLE,
    }

    [SerializeField] private Image m_reticleImage;
    [SerializeField] Sprite[] m_reticleSprites = new Sprite[6];
    private int m_reticleTypeNumber = (int)ReticleType.NORMAL;
    private int m_isHit = 1;

    private void UpdateImage()
    {
        m_reticleImage.sprite = m_reticleSprites[m_reticleTypeNumber * 2 + 1];
    }

    public void SetReticleTypeNumber(int _num)
    {
        m_reticleTypeNumber = _num;
        UpdateImage();
    }

    public void SetIsHit(bool _isHit)
    {
        if(_isHit)
        {
            m_isHit = 0;
        }
        else
        {
            m_isHit = 1;
        }
        UpdateImage();
    }
}
