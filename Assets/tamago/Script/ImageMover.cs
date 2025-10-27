using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageMover : MonoBehaviour
{
    private RectTransform rectTransform;
    private float moveSpeed = 200.0f; // 最大速度（ピクセル/秒）
    private float bounceHeight = 50.0f; // 跳ねる高さ
    private float bounceDuration = 0.5f; // 跳ねる時間
    private bool isMoving = false;

    // 動ける範囲の設定
    private float minX = 209.9f;
    private float maxX = 269.9f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // 初期位置を画面の右端に設定
        rectTransform.anchoredPosition = new Vector2(239.9f, rectTransform.anchoredPosition.y);
    }

    private void Update()
    {
        // スペースキーが押されたら動き始める
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            StartCoroutine(Move());
        }
    }

    private System.Collections.IEnumerator Move()
    {
        isMoving = true;

        // 左に移動
        float elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            float t = elapsedTime / bounceDuration;
            float xMovement = -moveSpeed * Time.deltaTime; // 左に移動
            float bounceOffset = Mathf.Sin(t * Mathf.PI) * bounceHeight; // X方向に跳ねる動き

            // 現在の位置を更新（範囲内にあるかチェック）
            float newX = rectTransform.anchoredPosition.x + xMovement + bounceOffset;
            if (newX < minX)
            {
                newX = minX; // 最小範囲を超えない
            }
            rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 停止
        yield return new WaitForSeconds(0.5f); // 少し待機

        // 右に少し動かす
        elapsedTime = 0f;

        while (elapsedTime < bounceDuration / 2)
        {
            float t = elapsedTime / (bounceDuration / 2);
            float xMovement = moveSpeed * Time.deltaTime; // 右に移動
            float bounceOffset = Mathf.Sin(t * Mathf.PI) * (bounceHeight / 2); // 小さく跳ねる

            // 現在の位置を更新（範囲内にあるかチェック）
            float newX = rectTransform.anchoredPosition.x + xMovement + bounceOffset;
            if (newX > maxX)
            {
                newX = maxX; // 最大範囲を超えない
            }
            rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 停止
        yield return new WaitForSeconds(1.0f); // 待機

        // 左に戻る
        elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            float t = elapsedTime / bounceDuration;
            float xMovement = -moveSpeed * Time.deltaTime; // 左に移動
            float bounceOffset = Mathf.Sin(-t * Mathf.PI) * bounceHeight; // X方向に跳ねる動き

            // 現在の位置を更新（範囲内にあるかチェック）
            float newX = rectTransform.anchoredPosition.x + xMovement + bounceOffset;
            if (newX < minX)
            {
                newX = minX; // 最小範囲を超えない
            }
            rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 停止
        isMoving = false; // 動作を終了
    }
}
