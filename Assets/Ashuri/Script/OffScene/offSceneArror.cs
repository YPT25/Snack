using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class offSceneArror : MonoBehaviour
{
    // ===============================
    // 移動先のUI座標（anchoredPosition）
    // ===============================
    [Header("移動先ポジション（X,Y）")]
    public Vector2 targetPosition;

    // ===============================
    // 移動スピード
    // ===============================
    [Header("移動スピード")]
    public float moveSpeed = 5f;

    // ===============================
    // 自分自身のRectTransform
    // ===============================
    private RectTransform rectTransform;

    // ===============================
    // 移動中かどうか
    // ===============================
    private bool isMoving = false;

    // ===============================
    // 開始時処理
    // ===============================
    void Start()
    {
        // ===============================
        // RectTransformを取得
        // ===============================
        rectTransform = GetComponent<RectTransform>();

        // ===============================
        // 移動開始
        // ===============================
        isMoving = true;
    }

    // ===============================
    // 毎フレーム処理
    // ===============================
    void Update()
    {
        // ===============================
        // 移動していなければ何もしない
        // ===============================
        if (!isMoving)
        {
            return;
        }

        // ===============================
        // 現在位置から目標位置へなめらかに移動
        // ===============================
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // ===============================
        // 目標位置に近づいたら停止
        // ===============================
        if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) < 1f)
        {
            rectTransform.anchoredPosition = targetPosition;
            isMoving = false;
        }
    }
}
