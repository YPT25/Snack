using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer1 : MonoBehaviour
{
    public Image minutesImage; // 分の表示用Image（1の位）
    public Image secondsImage; // 秒の十の位用Image
    public Image secondsOnesImage; // 秒の一の位用Image
    public float countdownTime = 180f; // 3分（180秒）
    private float timeRemaining;

    // スプライトシートのスプライトを格納する配列
    public Sprite[] numberSprites; // 0から9のスプライト

    void Start()
    {
        timeRemaining = countdownTime;
        UpdateTimerDisplay();
    }

    void Update()
    {
        // タイマーのカウントダウン
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // タイマーが0未満になった場合は0にクリップ
            if (timeRemaining < 0)
            {
                timeRemaining = 0;
            }

            UpdateTimerDisplay();
        }
    }

    // タイマーの表示を更新するメソッド
    private void UpdateTimerDisplay()
    {
        // 残り時間を分と秒に分解
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // 分の一の位を取得
        int onesMinutes = minutes % 10;  // 一の位

        // 数字を表示するためのスプライトを取得
        int tensSeconds = seconds / 10;   // 秒の十の位
        int onesSeconds = seconds % 10;    // 秒の一の位

        // スプライトを設定
        minutesImage.sprite = numberSprites[onesMinutes]; // 分の一の位
        secondsImage.sprite = numberSprites[tensSeconds]; // 秒の十の位
        secondsOnesImage.sprite = numberSprites[onesSeconds]; // 秒の一の位

        // 変更: 残り時間が30秒以下のときに画像を赤に変更
        if (timeRemaining <= 30f) // 変更: 残り時間が30秒以下の場合
        {
            minutesImage.color = Color.red; // 分の表示用Imageを赤に変更
            secondsImage.color = Color.red; // 秒の十の位用Imageを赤に変更
            secondsOnesImage.color = Color.red; // 秒の一の位用Imageを赤に変更
        }
        else
        {
            // 残り時間が30秒より多い場合は元の色に戻す
            minutesImage.color = Color.white; // 元の色
            secondsImage.color = Color.white; // 元の色
            secondsOnesImage.color = Color.white; // 元の色
        }
    }
}
