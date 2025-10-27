using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkImage : MonoBehaviour
{
    public Image targetImage; // 点滅させるImage
    public float blinkDuration = 0.2f; // 点滅の間隔
    private Color originalColor;

    void Start()
    {
        // 元の色を保存
        originalColor = targetImage.color;
    }

    void Update()
    {
        // スペースキーが押されたときに点滅を開始
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Blink());
        }
    }

    private IEnumerator Blink()
    {
        for (int i = 0; i < 2; i++) // 2回点滅
        {
            // アルファ値を0にする
            Color newColor = originalColor;
            newColor.a = 0.5f; // 透明にする
            targetImage.color = newColor;

            yield return new WaitForSeconds(blinkDuration); // 指定した時間待機

            // 元の色に戻す
            targetImage.color = originalColor;

            yield return new WaitForSeconds(blinkDuration); // 指定した時間待機
        }
    }
}
