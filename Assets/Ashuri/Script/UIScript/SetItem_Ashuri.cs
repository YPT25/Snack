using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SetPart_Tanabe;

public class SetItem_Ashuri : MonoBehaviour
{
    [Header("セットアイテム")]
    [Tooltip("セットアイテムを表示するImage")]
    [SerializeField] private Image _setItemImage;

    [Header("セットアイテムの画像")]
    [Tooltip("セットアイテム画像(マーベル)")]
    [SerializeField] private Sprite _setItem1;

    [Tooltip("セットアイテム画像(たけのこ)")]
    [SerializeField] private Sprite _setItem2;

    [Tooltip("何もないときの透明画像")]
    [SerializeField] private Sprite _transparent;

    // ローカルプレイヤーの参照
    private Player_Tanabe _localPlayer;

    // セットアイテムを管理しているスクリプト
    private SetPart_Tanabe _setPart_Tanabe;

    // セットアイテムの種類
    private PartType _partType;

    // Start is called before the first frame update
    void Start()
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

        // セットパーツクラスを取得
        _setPart_Tanabe = _localPlayer.GetPart();

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
        _partType = _localPlayer.GetPartType();

        // セットアイテムアイコン切り替え
        SetSprite(_setItemImage, _partType);
    }

    // -----------------------------------------------
    // アイテムの種類に応じて Sprite を更新する共通処理
    private void SetSprite(Image target, PartType type)
    {
        // マーベル
        if (type == PartType.LONGBARREL)
        {
            target.sprite = _setItem1;
            return;
        }

        // タケノコ
        if (type == PartType.SHARPBULLET)
        {
            target.sprite = _setItem2;
            return;
        }

        // 何もない時は透明
        target.sprite = _transparent;
    }
}
