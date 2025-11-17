using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Player_Tanabe;

public class WeaponDisplayManager : MonoBehaviour
{
    [Header("武器のUI")]
    [Tooltip("銃のイラスト")]
    [SerializeField] private Image _gunSprite;

    [Tooltip("ハンマーのイラスト")]
    [SerializeField] private Image _hammerSprite;

    private Player_Tanabe _player_Tanabe;
    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーが現れるまで待つ処理を開始
        StartCoroutine(WaitForPlayer());
    }

    // Update is called once per frame
    void Update()
    {

    }

    // プレイヤーが生成されるまで探し続ける処理
    private IEnumerator WaitForPlayer()
    {
        // プレイヤーが見つかるまでループ
        while (_player_Tanabe == null)
        {
            _player_Tanabe = FindObjectOfType<Player_Tanabe>();
            yield return null;  // 1フレーム待つ
        }

        // プレイヤーが見つかったので武器UIを更新
        UpdateWeaponUI(_player_Tanabe.GetWeaponID());
    }

    // 武器UIを更新する処理
    private void UpdateWeaponUI(Player_Tanabe.WeaponID id)
    {
        // UIを一度全部消す処理
        _hammerSprite.gameObject.SetActive(false);
        _gunSprite.gameObject.SetActive(false);

        // ハンマー表示処理
        if (id == Player_Tanabe.WeaponID.HAMMER)
        {
            _hammerSprite.gameObject.SetActive(true);
        }

        // 銃の表示処理
        if (id == Player_Tanabe.WeaponID.GUN)
        {
            _gunSprite.gameObject.SetActive(true);
        }
    }
}
