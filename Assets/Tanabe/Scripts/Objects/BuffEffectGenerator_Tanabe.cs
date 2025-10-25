using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffectGenerator_Tanabe : MonoBehaviour
{
    [SerializeField] private GameObject m_powerUp;
    [SerializeField] private GameObject m_powerDown;
    [SerializeField] private GameObject m_speedUp;
    [SerializeField] private GameObject m_healing;

    public GameObject GetEffect_PowerUp() { return m_powerUp; }
    public GameObject GetEffect_PowerDown() { return m_powerDown; }
    public GameObject GetEffect_SpeedUp() { return m_speedUp; }
    public GameObject GetEffect_Healing() { return m_healing; }
}
