using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using static Player_Tanabe;
using UnityEngine.Rendering;

/// <summary>
/// ローカルプレイヤーの武器に応じて UI を切り替えるクラス
/// Canvas に置くだけで動作する
/// </summary>
public class WeaponDisplayManager : MonoBehaviour
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

    // ローカルプレイヤーを保持する変数
    private Player_Tanabe _localPlayer;

    // ゲーム開始時に呼ばれる処理
    private void Start()
    {
        // ローカルプレイヤーを見つける処理を開始
        StartCoroutine(FindLocalPlayer());
    }

    // ローカルプレイヤーを探す処理
    private IEnumerator FindLocalPlayer()
    {
        // ローカルプレイヤーが見つかるまで繰り返す
        while (_localPlayer == null)
        {
            // シーン内の全 Player_Tanabe を取得
            var players = FindObjectsOfType<Player_Tanabe>();

            // ローカルプレイヤーを探す処理
            foreach (var p in players)
            {
                // ローカルプレイヤーかどうかチェック
                if (p.isLocalPlayer)
                {
                    _localPlayer = p;
                    break;
                }
            }
                
            // 見つからなかったら次のフレームまで待機
            if (_localPlayer == null)
                yield return null;
        }

        // 見つかったら武器 UI を更新
        UpdateWeaponUI(_localPlayer.GetWeaponID());
    }

    // 武器 UI を更新する処理
    public void UpdateWeaponUI(WeaponID id)
    {
        // 銃表示
        if (id == WeaponID.GUN)
            _weaponImage.sprite = _gunSprite;

        // ハンマー表示
        if (id == WeaponID.HAMMER)
            _weaponImage.sprite = _hammerSprite;
    }

    /// <summary>
    /// ローカルプレイヤーの取得
    /// </summary>
    /// <param name="_localPlayer"></param>
    public void SetLocalPlayer(Player_Tanabe _localPlayerTanabe)
    {
        _localPlayer = _localPlayerTanabe;
    }
}
