using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    // 最大HP
    public float maxHP = 100f;
    // 現在のHP
    private float currentHP;
    // HPバーのRectTransform
    private RectTransform rectTransform;

    // 初期位置（HPが満タンのときの位置）
    private Vector2 initialPosition;

    // HPの変化に関する変数
    private float targetHP;
    private float lerpSpeed = 5f; // 補間の速度

    void Start()
    {
        // HPバーの初期化
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition; // 初期位置を保存

        // CharacterBaseのコンポーネントを取得
        CharacterBase characterBase = FindObjectOfType<CharacterBase>();
        if (characterBase != null)
        {
            // 初期最大HPを設定
            maxHP = characterBase.GetMaxHP();
            currentHP = characterBase.GetHp();
            targetHP = currentHP; // 初期値をターゲットHPに設定

            UpdateHPBar();
        }
    }

    void Update()
    {
        // プレイヤーの現在のHPを取得して更新
        CharacterBase characterBase = FindObjectOfType<CharacterBase>();
        if (characterBase != null)
        {
            // HPのターゲットを更新（ダメージや回復によって変化する）
            targetHP = characterBase.GetHp();
        }

        // スムーズにHPを補間
        currentHP = Mathf.Lerp(currentHP, targetHP, Time.deltaTime * lerpSpeed);
        UpdateHPBar();
    }

    // HPバーの表示を更新するメソッド
    private void UpdateHPBar()
    {
        // 現在のHPに基づいてポジションを更新
        float normalizedHP = currentHP / maxHP; // 0から1の範囲に正規化
        rectTransform.anchoredPosition = new Vector2(initialPosition.x - (1 - normalizedHP) * 100, initialPosition.y); // 幅に応じて横位置を調整
    }


}