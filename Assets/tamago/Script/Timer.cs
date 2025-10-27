using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Image minutesOnesImage; // 分の一の位用Image
    public Image secondsImage; // 秒の十の位用Image
    public Image secondsOnesImage; // 秒の一の位用Image
    public Image pictureImage;
    public Image warningImage1; // 警告用のImage（0:30から表示）
    public Image warningImage2;
    public Image warningImage3;
    public Image pictureImage2;
    public float countdownTime = 180f; // 3分（180秒）
    private float timeRemaining;

    public float blinkDuration = 0.2f; // 点滅の間隔

    // スプライトシートのスプライトを格納する配列
    public Sprite[] numberSprites; // 0から9のスプライト
    public Sprite[] numberSprites1; // 警告用のスプライト（異なる色）

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

            // タイマーの表示を更新
            if (timeRemaining <= 30f)
            {
                UpdateTimerDisplay1();

            }
            else
            {
                UpdateTimerDisplay();
            }
        }

        // 警告画像の表示
        if (timeRemaining <= 30f)
        {
            warningImage1.gameObject.SetActive(true); // 警告画像を表示
            warningImage2.gameObject.SetActive(true);
            warningImage3.gameObject.SetActive(true);
            pictureImage2.gameObject.SetActive(true);
            minutesOnesImage.gameObject.SetActive(false);
            secondsImage.gameObject.SetActive(false);
            secondsOnesImage.gameObject.SetActive(false);
            pictureImage.gameObject.SetActive(false);

        }
        else
        {
            // 警告画像を非表示
            warningImage1.gameObject.SetActive(false);
            warningImage2.gameObject.SetActive(false);
            warningImage3.gameObject.SetActive(false);
            pictureImage2.gameObject.SetActive (false);
        }
    }

    // タイマーの表示を更新するメソッド
    private void UpdateTimerDisplay()
    {
        // 残り時間を分と秒に分解
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // 分の一の位を取得
        int onesMinutes = minutes % 10; // 一の位

        // 数字を表示するためのスプライトを取得
        int tensSeconds = seconds / 10; // 秒の十の位
        int onesSeconds = seconds % 10;  // 秒の一の位

        // スプライトを設定
        minutesOnesImage.sprite = numberSprites[onesMinutes]; // 分の一の位
        secondsImage.sprite = numberSprites[tensSeconds]; // 秒の十の位
        secondsOnesImage.sprite = numberSprites[onesSeconds]; // 秒の一の位
    }

    private void UpdateTimerDisplay1()
    {
        // 残り時間を分と秒に分解
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // 分の一の位を取得
        int onesMinutes = minutes % 10; // 一の位

        // 数字を表示するためのスプライトを取得
        int tensSeconds = seconds / 10; // 秒の十の位
        int onesSeconds = seconds % 10;  // 秒の一の位

        // スプライトを設定
        warningImage1.sprite = numberSprites1[onesMinutes]; // 分の一の位
        warningImage2.sprite = numberSprites1[tensSeconds]; // 秒の十の位
        warningImage3.sprite = numberSprites1[onesSeconds]; // 秒の一の位
    }
}