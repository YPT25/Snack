using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaBar : MonoBehaviour
{
    // 最大スタミナ
    public float maxStamina = 100f;
    // 現在のスタミナ
    public float currentStamina;
    // スタミナ消費量（ボタン押下時）
    public float staminaDecreaseAmount = 15f;
    // スタミナ回復量（ボタン放した時）
    public float staminaIncreaseAmount = 7f;
    // スタミナバーのRectTransform
    private RectTransform rectTransform;

    // 初期位置（スタミナが満タンのときの位置）
    public float initialPositionX = 100f; // 初期位置を100に設定
    private float targetPositionX; // 目標位置

    void Start()
    {
        // スタミナバーの初期化
        rectTransform = GetComponent<RectTransform>();
        currentStamina = maxStamina; // スタミナを最大値で初期化
        targetPositionX = initialPositionX; // 目標位置を初期位置に設定
    }

    void Update()
    {
        // 左シフトキーまたはコントローラーのAボタンが押されているか確認
        if (Input.GetKey(KeyCode.LeftShift)) // "Fire1"はデフォルトでAボタンにマッピングされています
        {
            DecreaseStamina(); // スタミナを減少させる
        }
        else
        {
            RecoverStamina(); // ボタンを離したときにスタミナを回復
        }

        // 現在の位置を徐々に目標位置に移動させる
        rectTransform.anchoredPosition = new Vector2(
            Mathf.Lerp(rectTransform.anchoredPosition.x, targetPositionX, Time.deltaTime * 5f),
            rectTransform.anchoredPosition.y
        );
    }

    private void DecreaseStamina()
    {
        if (currentStamina > 0)
        {
            currentStamina -= staminaDecreaseAmount * Time.deltaTime; // 時間に基づいて減少
            currentStamina = Mathf.Max(0, currentStamina); // スタミナが0未満にならないように

            // 目標位置を設定（スタミナが減少した場合）
            targetPositionX = initialPositionX - (maxStamina - currentStamina); // スタミナに基づく目標位置
        }
    }

    private void RecoverStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaIncreaseAmount * Time.deltaTime; // 時間に基づいて回復
            currentStamina = Mathf.Min(maxStamina, currentStamina); // スタミナが最大値を超えないように

            // 目標位置を設定（スタミナが回復した場合）
            targetPositionX = initialPositionX - (maxStamina - currentStamina); // スタミナに基づく目標位置
        }
        else
        {
            // スタミナが満タンのときは目標位置を初期位置に戻す
            targetPositionX = initialPositionX;
        }
    }
}