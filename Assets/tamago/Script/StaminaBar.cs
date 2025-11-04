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
    public float staminaDecreaseAmount = 10f;
    // スタミナ回復量（ボタン放した時）
    public float staminaIncreaseAmount = 3f;
    // スタミナバーのRectTransform
    private RectTransform rectTransform;

    // 初期位置（スタミナが満タンのときの位置）
    public Vector2 initialPosition;

    void Start()
    {
        // スタミナバーの初期化
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition; // 初期位置を保存

        // スタミナを最大値で初期化
        currentStamina = maxStamina;
        //UpdateStaminaBar();
    }

    void Update()
    {
        // 左シフトキーまたはコントローラーのAボタンが押されているか確認
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire1")) // "Fire1"はデフォルトでAボタンにマッピングされています
        {
            DecreaseStamina();
        }
        else
        {
            RecoverStamina(); // ボタンを離したときにスタミナを回復
        }

        //// スタミナバーを更新
        //UpdateStaminaBar();
    }

    private void DecreaseStamina()
    {
        if (currentStamina > 0)
        {
            currentStamina -= staminaDecreaseAmount * Time.deltaTime; // 時間に基づいて減少
            currentStamina = Mathf.Max(0, currentStamina); // スタミナが0未満にならないように
        }
        // 現在のスタミナに基づいてポジションを更新
        float normalizedStamina = currentStamina / maxStamina; // 0から1の範囲に正規化
        rectTransform.anchoredPosition = new Vector2(initialPosition.x - (1 - normalizedStamina) * 100, initialPosition.y);
    }

    private void RecoverStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaIncreaseAmount * Time.deltaTime; // 時間に基づいて回復
            currentStamina = Mathf.Min(maxStamina, currentStamina); // スタミナが最大値を超えないように
        }
        // 現在のスタミナに基づいてポジションを更新
        float normalizedStamina = currentStamina / maxStamina; // 0から1の範囲に正規化
        rectTransform.anchoredPosition = new Vector2(initialPosition.x + (1 + normalizedStamina) * 100, initialPosition.y);
    }

    //// スタミナバーの表示を更新するメソッド
    //private void UpdateStaminaBar()
    //{
    //    // 現在のスタミナに基づいてポジションを更新
    //    float normalizedStamina = currentStamina / maxStamina; // 0から1の範囲に正規化
    //    rectTransform.anchoredPosition = new Vector2(initialPosition.x - (1 - normalizedStamina) * 100, initialPosition.y); 
    //}
}