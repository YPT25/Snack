using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    // 最大HP
    public float maxHP = 100f;
    // 現在のHP
    public float currentHP;
    // 初期位置（HPが満タンのときの位置）
    public Vector2 initialPosition;

    // HPバーのRectTransform
    private RectTransform rectTransform;

    // CharacterBaseの参照
    private CharacterBase characterBase;

    void Start()
    {
        // HPバーの初期化
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition; // 初期位置を保存

        // CharacterBaseのコンポーネントを取得
        characterBase = FindObjectOfType<CharacterBase>();
        if (characterBase != null)
        {
            // 初期最大HPを設定
            maxHP = characterBase.GetMaxHP();
            currentHP = characterBase.GetHp();
            UpdateHPBar();
        }
    }

    void Update()
    {
        // プレイヤーの現在のHPを取得して更新
        if (characterBase != null)
        {
            currentHP = characterBase.GetHp();
            UpdateHPBar();
        }
    }

    // HPバーの表示を更新するメソッド
    private void UpdateHPBar()
    {
        // 現在のHPに基づいてポジションを更新
        float normalizedHP = currentHP / maxHP; // 0から1の範囲に正規化
        rectTransform.anchoredPosition = new Vector2(initialPosition.x - (1 - normalizedHP) * 100, initialPosition.y); // 幅に応じて横位置を調整
    }
}