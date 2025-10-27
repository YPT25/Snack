using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    // 最大HP
    public float maxHP = 100f;
    // 現在のHP
    public float currentHP;
    // 一回あたりの減少量
    public float decreaseAmount = 20f;
    // 減少にかかる時間
    public float decreaseDuration = 1f; // 1秒で減少
    // 初期位置（HPが満タンのときの位置）
    public Vector2 initialPosition;

    // HPバーのRectTransform
    private RectTransform rectTransform;

    void Start()
    {
        // 現在のHPを最大HPで初期化
        currentHP = maxHP;
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition; // 初期位置を保存
        UpdateHPBar();
    }

    void Update()
    {
        // スペースキーが押されたか確認
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DecreaseHPOverTime(decreaseAmount, decreaseDuration));
        }
    }

    // HPを少しずつ減少させるコルーチン
    private IEnumerator DecreaseHPOverTime(float amount, float duration)
    {
        float startHP = currentHP;
        float targetHP = Mathf.Max(0, currentHP - amount); // HPが0未満にならないようにする

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // 経過時間を更新
            currentHP = Mathf.Lerp(startHP, targetHP, elapsed / duration); // 線形補間でHPを減少させる
            UpdateHPBar(); // HPバーを更新
            yield return null; // 次のフレームまで待機
        }

        currentHP = targetHP; // 最終的なHPを設定
        UpdateHPBar(); // 最終的なHPバーを更新
    }

    // HPバーの表示を更新するメソッド
    private void UpdateHPBar()
    {
        // 現在のHPに基づいてポジションを更新
        float normalizedHP = currentHP / maxHP; // 0から1の範囲に正規化
        rectTransform.anchoredPosition = new Vector2(initialPosition.x - (1 - normalizedHP) * 100, initialPosition.y); // 幅に応じて横位置を調整
    }
}
