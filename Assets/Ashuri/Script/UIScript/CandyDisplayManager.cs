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

    [Tooltip("何もないときの透明画像")]
    [SerializeField] private Sprite _transparent;

    // ローカルプレイヤーの参照
    private Player_Tanabe _localPlayer;

    // ローカルプレイヤーが持っているアイテム管理クラス
    private PossessionManager_Tanabe _possession;

    // 所持アイテムの２枠
    private ItemType[] _items = new ItemType[2];

    // -----------------------------------------------
    // ゲーム開始時にローカルプレイヤーを探す
    private void Start()
    {
        // ローカルプレイヤー探索の開始
        StartCoroutine(FindLocalPlayer());
    }

    // -----------------------------------------------
    // ローカルプレイヤーが見つかるまで探す処理
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
    private void UpdateCandyUI()
    {
        // 所持アイテム2種類を取得
        _possession.GetItem(out _items[0], out _items[1]);

        // 左側のアイコン切り替え
        SetSprite(_leftSprite, _items[0]);

        // 右側のアイコン切り替え
        SetSprite(_rightSprite, _items[1]);
    }

    // -----------------------------------------------
    // アイテムの種類に応じて Sprite を更新する共通処理
    private void SetSprite(Image target, ItemType type)
    {
        // 罠アイテム（綿菓子）
        if (type == ItemType.TRAP)
        {
            target.sprite = _fluffy;
            return;
        }

        // 投擲アイテム（ポップコーン）
        if (type == ItemType.THROW)
        {
            target.sprite = _popcorn;
            return;
        }

        // 何もない時は透明
        target.sprite = _transparent;
    }
}
