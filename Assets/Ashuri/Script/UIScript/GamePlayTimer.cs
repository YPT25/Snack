using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 残り時間を「分:秒」の画像で表示するクラス
/// ・GameManagerから秒数を取得
/// ・分と秒に分解して数字画像を表示する
/// </summary>
public class GamePlayTimer : MonoBehaviour
{
    [Header("数字画像 (0〜9)")]
    [Tooltip("index が数字に対応（0 → 0の画像、1 → 1の画像）")]
    [SerializeField] private List<Sprite> numberSpriteList = new List<Sprite>();

    [Header("UI（分の桁）")]
    [Tooltip("左の桁（分の10の位）")]
    [SerializeField] private Image _minuteLeft;

    [Tooltip("右の桁（分の1の位）")]
    [SerializeField] private Image _minuteRight;

    [Header("UI（秒の桁）")]
    [Tooltip("左の桁（秒の10の位）")]
    [SerializeField] private Image _secondLeft;

    [Tooltip("右の桁（秒の1の位）")]
    [SerializeField] private Image _secondRight;

    // Update is called once per frame
    void Update()
    {
        // GameManagerが存在しない場合は更新しない
        if (GameManager.Instance == null) return;

        // GameManagerから残り時間（秒）を取得
        float time = GameManager.Instance.CurrentTime;

        // 時間を整数の秒に変換
        int totalSeconds = Mathf.FloorToInt(time);

        // 分を計算
        int minutes = totalSeconds / 60;

        // 秒を計算
        int seconds = totalSeconds % 60;

        // 分の10の位
        int minLeft = minutes / 10;

        // 分の1の位
        int minRight = minutes % 10;

        // 秒の10の位
        int secLeft = seconds / 10;

        // 秒の1の位
        int secRight = seconds % 10;

        // 分の10の位画像を更新
        _minuteLeft.sprite = numberSpriteList[minLeft];

        // 分の1の位画像を更新
        _minuteRight.sprite = numberSpriteList[minRight];

        // 秒の10の位画像を更新
        _secondLeft.sprite = numberSpriteList[secLeft];

        // 秒の1の位画像を更新
        _secondRight.sprite = numberSpriteList[secRight];
    }
}
