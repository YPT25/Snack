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

    [Header("�Z�b�g�p�[�c�̃^�C�v"), SerializeField] private PartType m_partType;
    // �����p�[�c���g�p���邩
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
