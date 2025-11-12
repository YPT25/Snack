using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public Player_Tanabe player; // プレイヤーへの参照
    public Image weaponImage; // UIイメージへの参照
    public Sprite hammerSprite; // ハンマーのスプライト
    public Sprite gunSprite; // 銃のスプライト

    void Start()
    {
        // プレイヤーの参照が指定されていない場合、シーン内から自動的に取得
        if (player == null)
        {
            player = FindObjectOfType<Player_Tanabe>();
        }

        // 初期状態で武器画像を非表示にする
        weaponImage.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateWeaponUI();
    }

    private void UpdateWeaponUI()
    {
        // プレイヤーが持っている武器IDを取得
        Player_Tanabe.WeaponID currentWeaponID = player.GetWeaponID();

        // 武器IDに基づいてUIを更新
        switch (currentWeaponID)
        {
            case Player_Tanabe.WeaponID.HAMMER:
                weaponImage.sprite = hammerSprite;
                weaponImage.gameObject.SetActive(true);
                break;

            case Player_Tanabe.WeaponID.GUN:
                weaponImage.sprite = gunSprite;
                weaponImage.gameObject.SetActive(true);
                break;

            case Player_Tanabe.WeaponID.NONE:
            default:
                weaponImage.gameObject.SetActive(false);
                break;
        }
    }
}
