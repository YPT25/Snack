using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemStateMachine;

public class CandyDisplayManager : MonoBehaviour
{
    // 左側のアイテムを表示する Image
    [Header("お菓子のUI")]
    [Tooltip("左側のイラスト")]
    [SerializeField] private Image _leftSprite;

    // 右側のアイテムを表示する Image
    [Tooltip("右のイラスト")]
    [SerializeField] private Image _rightSprite;

    // アイテムの状態を管理しているクラス
    private PossessionManager_Tanabe _PossessionManager_Tanabe;

    // プレイヤー情報を持つクラス
    private Player_Tanabe _player_Tanabe;

    // プレイヤーが所持しているアイテムを保存する配列
    private ItemStateMachine.ItemType[] _items = new ItemStateMachine.ItemType[2];

    // 投擲（ポップコーン）の Sprite
    [Header("アイテム画像")]
    [Tooltip("ポップコーンの画像")]
    [SerializeField] private Sprite _popcorn;

    // 罠（ふわふわ）の Sprite
    [Tooltip("綿菓子の画像")]
    [SerializeField] private Sprite _fluffy;

    // 透明画像の Sprite
    [Tooltip("透過画像")]
    [SerializeField] private Sprite _transparent;

    // ゲーム開始時に最初に呼ばれる処理（今回は特に何もしない）
    void Start()
    {
        // プレイヤーと管理クラスが揃うまで待つ
        StartCoroutine(WaitForCandy());
    }

    private IEnumerator WaitForCandy()
    {
        // このコンポーネントが乗っているオブジェクトからプレイヤー情報を取得
        _player_Tanabe = GetComponentInParent<Player_Tanabe>();    // 各プレイヤーの PossessionManager を Player から取得
        _PossessionManager_Tanabe = _player_Tanabe.GetPossesionManager();        // 所持アイテムが変わるたびに UI 更新
        while (true)
        {
            UpdateCandyUI(_PossessionManager_Tanabe);
            yield return null; // 毎フレームチェックして更新
        }
    }
    // 所持アイテムに応じて UI アイコンを更新する処理
    private void UpdateCandyUI(PossessionManager_Tanabe possession)
    {
        // 所持アイテム2種類を取得する（左と右）
        possession.GetItem(out _items[0], out _items[1]);

        Debug.Log($"0:" + _items[0]);
        Debug.Log($"1:" + _items[1]);

        // 左側が罠アイテムだった場合の処理
        if (_items[0] == ItemType.TRAP)
        {
            // 左側にふわふわの画像を表示する
            _leftSprite.sprite = _fluffy;
            Debug.Log("変わったよ");
        }

        // 左側が投擲アイテムだった場合の処理
        if (_items[0] == ItemType.THROW)
        {
            // 左側にポップコーンの画像を表示する
            _leftSprite.sprite = _popcorn;
            Debug.Log("変わったよ");
        }

        // 右側が罠アイテムだった場合の処理
        if (_items[1] == ItemType.TRAP)
        {
            // 右側にふわふわの画像を表示する
            _rightSprite.sprite = _fluffy;
            Debug.Log("変わったよ");
        }

        // 右側が投擲アイテムだった場合の処理
        if (_items[1] == ItemType.THROW)
        {
            // 右側にポップコーンの画像を表示する
            _rightSprite.sprite = _popcorn;
            Debug.Log("変わったよ");
        }

        // 左側が空欄の場合の処理
        if (_items[0] == ItemType.NONE_TYPE)
        {
            // 左側を透明画像にする
            _leftSprite.sprite = _transparent;
        }

        // 右側が空欄の場合の処理
        if (_items[1] == ItemType.NONE_TYPE)
        {
            // 右側を透明画像にする
            _rightSprite.sprite = _transparent;
        }
    }
}
