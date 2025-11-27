using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using static Player_Tanabe;

/// <summary>
/// ローカルプレイヤーの武器に応じて UI を切り替えるクラス
/// Canvas に置くだけで動作する
/// </summary>
public class WeaponDisplayManager : MonoBehaviour
{
    [Header("武器のUI")]
    [Tooltip("銃のイラスト")]
    [SerializeField] private Image _gunSprite;

    [Tooltip("ハンマーのイラスト")]
    [SerializeField] private Image _hammerSprite;

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
    private void UpdateWeaponUI(WeaponID id)
    {
        // UI をすべて非表示にする
        _gunSprite.gameObject.SetActive(false);
        _hammerSprite.gameObject.SetActive(false);

        // 銃表示
        if (id == WeaponID.GUN)
            _gunSprite.gameObject.SetActive(true);

        // ハンマー表示
        if (id == WeaponID.HAMMER)
            _hammerSprite.gameObject.SetActive(true);
    }
}
