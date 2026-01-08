using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static Player_Tanabe;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WeaponLobbyScript : MonoBehaviour
{
    [Header("武器のUI")]
    [Tooltip("Image")]
    [SerializeField] private Image _weaponImage;

    [Tooltip("銃のイラスト")]
    [SerializeField] private Sprite _gunSprite;

    [Tooltip("ハンマーのイラスト")]
    [SerializeField] private Sprite _hammerSprite;

    [Tooltip("ポップコーンのイラスト")]
    [SerializeField] private Sprite _poppcornSprite;

    [Tooltip("綿あめのイラスト")]
    [SerializeField] private Sprite _watagashiSprite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
