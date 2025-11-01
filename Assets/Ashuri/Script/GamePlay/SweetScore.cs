using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProを使用するために必要

/// <summary>
/// スコアの表示と加算を管理するクラス。
/// Oキーを押すとスコアが増える。
/// </summary>
public class SweetScore : MonoBehaviour
{
    [Header("スコア表示用のTextMeshPro")]
    [Tooltip("スコアを表示するTextMeshProUGUIコンポーネントを指定してください。")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("スコア設定")]
    [Tooltip("現在のスコア。ゲーム開始時は0にリセットされる。")]
    [SerializeField] public float currentScore = 0.0f;

    void Start()
    {
        // 初期スコアを表示に反映
        UpdateScoreDisplay();
    }

    void Update()
    {

    }

    /// <summary>
    /// スコアを取得する関数。
    /// </summary>
    /// <param name="amount">加算するスコアの値。</param>
    public void AddScore(float amount)
    {
        currentScore = amount;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// TextMeshProにスコアを反映する。
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
        else
        {
            Debug.LogWarning("Score TextMeshProがアサインされていません。");
        }
    }
}
