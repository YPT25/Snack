using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AlivePlayers : MonoBehaviour
{
    private List<CharacterBase> _logged = new();
    [SerializeField] private Image[] _crossImg;

    private void OnEnable()
    {
        //CharacterBase.OnCharacterTakeDamage += LogAlive;
    }
    private void OnDisable()
    {
        //CharacterBase.OnCharacterTakeDamage -= LogAlive;
    }
    private void LogAlive(CharacterBase character)
    {
        if (!_logged.Contains(character) && character.name.Contains("Player"))
        {
            _logged.Add(character);
            foreach (Image cross in _crossImg)
            {
                if (!cross.gameObject.activeSelf)
                {
                    cross.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
}
