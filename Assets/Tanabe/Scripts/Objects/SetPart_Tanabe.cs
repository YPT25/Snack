using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPart_Tanabe : MonoBehaviour
{
    public enum PartType
    {
        NONE_TYPE,
        LONGBARREL,
        SHARPBULLET,
    }

    [Header("セットパーツのタイプ"), SerializeField] private PartType m_partType;
    // 強化パーツを使用するか
    private bool m_isUse = true;

    public PartType GetPartType()
    {
        return m_partType;
    }

    public bool GetIsUse()
    {
        return m_isUse;
    }
}
