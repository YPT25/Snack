using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using static ItemStateMachine;

public class CandyDisplayManager : MonoBehaviour
{
    [Header("お菓子UI")]
    [Tooltip("左のお菓子画像")]
    [SerializeField] private Image _leftSprite;

    [Tooltip("右のお菓子画像")]
    [SerializeField] private Image _rightSprite;

    [Header("アイテム画像")]
    [Tooltip("ポップコーン画像")]
    [SerializeField] private Sprite _popcorn;

    [Tooltip("綿菓子画像")]
    [SerializeField] private Sprite _fluffy;

    [Tooltip("ハンドの画像")]
    [SerializeField] private Sprite _handItem;

    [Tooltip("ドリンク(パワーアップ)")]
    [SerializeField] private Sprite _drink_power;

    [Tooltip("ドリンク(スピード)")]
    [SerializeField] private Sprite _drink_speed;

    [Tooltip("ドリンク(HP)")]
    [SerializeField] private Sprite _drink_hp;

    [Tooltip("ガム")]
    [SerializeField] private Sprite _gum;

    [Tooltip("ドリンク(Stamoina)")]
    [SerializeField] private Sprite _drink_stamina;

    [Tooltip("何もないときの透明画像")]
    [SerializeField] private Sprite _transparent;

    // ローカルプレイヤーの参照
    private Player_Tanabe _localPlayer;

    // ローカルプレイヤーが持っているアイテム管理クラス
    private PossessionManager_Tanabe _possession;

    // 所持アイテムの２枠
    private ItemNameID[] _items = new ItemNameID[2];

    // -----------------------------------------------
    // ゲーム開始時にローカルプレイヤーを探す
    // -----------------------------------------------
    private void Start()
    {
        // ローカルプレイヤー探索の開始
        StartCoroutine(FindLocalPlayer());
    }

    // -----------------------------------------------
    // ローカルプレイヤーが見つかるまで探す処理
    // -----------------------------------------------
    private IEnumerator FindLocalPlayer()
    {
        // ローカルプレイヤーが見つかるまで繰り返す
        while (_localPlayer == null)
        {
            // シーン上の全プレイヤーを取得
            var players = FindObjectsOfType<Player_Tanabe>();

            // ローカルプレイヤーであるか確認
            foreach (var p in players)
            {
                // ローカルプレイヤー判定
                if (p.isLocalPlayer)
                {
                    _localPlayer = p;
                    break;
                }
            }

            // 見つからなければ次のフレームへ
            if (_localPlayer == null)
                yield return null;
        }

        // ローカルプレイヤーの PossessionManager を取得
        _possession = _localPlayer.GetPossesionManager();

        // UI の更新コルーチンを開始
        StartCoroutine(UpdateUIRoutine());
    }

    // -----------------------------------------------
    // 毎フレーム UI を更新する処理
    // -----------------------------------------------
    private IEnumerator UpdateUIRoutine()
    {
        while (true)
        {
            // UI 更新処理を実行
            UpdateCandyUI();

            // 次のフレームまで待機
            yield return null;
        }
    }

    // -----------------------------------------------
    // 所持アイテムに合わせて UI を更新する処理
    // -----------------------------------------------
    private void UpdateCandyUI()
    {
        // 所持アイテム2種類を取得
        _possession.GetItemName(out _items[0], out _items[1]);

        // 左側のアイコン切り替え
        SetSprite(_leftSprite, _items[0]);

        // 右側のアイコン切り替え
        SetSprite(_rightSprite, _items[1]);
    }

    // -----------------------------------------------
    // アイテムの種類に応じて Sprite を更新する共通処理
    private void SetSprite(Image target, ItemNameID type)
    {
        // 罠アイテム（綿菓子）
        if (type == ItemNameID.WATAGASHI)
        {
            target.sprite = _fluffy;
            return;
        }

        // 投擲アイテム（ポップコーン）
        if (type == ItemNameID.POPCORN)
        {
            target.sprite = _popcorn;
            return;
        }

        // 缶バフ（HP回復）
        if (type == ItemNameID.DRINK_HEALING)
        {
            target.sprite = _drink_hp;
            return;
        }

        // 缶バフ（パワーアップ）
        if (type == ItemNameID.DRINK_POWERUP)
        {
            target.sprite = _drink_power;
            return;
        }

        // 缶バフ（スピード）
        if (type == ItemNameID.DRINK_SPEEDUP)
        {
            target.sprite = _drink_speed;
            return;
        }

        // 投擲アイテム（ハンド）
        if (type == ItemNameID.HOOKHAND)
        {
            target.sprite = _handItem;
            return;
        }

        // 投擲アイテム（ガム）
        if (type == ItemNameID.BUBBLEGUM)
        {
            target.sprite = _gum;
            return;
        }

        // 缶バフ(スタミナ)
        if(type == ItemNameID.DRINK_STAMINAN)
        {
            target.sprite = _drink_stamina;
            return;
        }

        // 何もない時は透明
        target.sprite = _transparent;
    }
}
