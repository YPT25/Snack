using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugParameterText_Tanabe : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_text;
    private CharacterBase m_character;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(m_character == null) { return; }
        m_text.text =
            "HP     :" + m_character.GetHp() + "\n" +
            "Power  :" + m_character.GetPower() + "\n" +
            "Speed  :" + m_character.GetMoveSpeed() + "\n" +
            "Stamina:" + m_character.GetStamina();
    }

    public void SetCharacter(CharacterBase _character)
    {
        m_character = _character;
    }
}
