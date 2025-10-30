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
    [SerializeField] private int currentScore = 0;

    void Start()
    {
        // 初期スコアを表示に反映
        UpdateScoreDisplay();
    }

    void Update()
    {
        // Oキーを押したときにスコアを加算
        if (Input.GetKeyDown(KeyCode.O))
        {
            AddScore(10); // 例：10点追加
        }
    }

    /// <summary>
    /// スコアを加算する関数。
    /// </summary>
    /// <param name="amount">加算するスコアの値。</param>
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// スコアを減少する関数。
    /// </summary>
    /// <param name="amount">減少するスコアの値。</param>
    public void SubtractScore(int amount)
    {
        currentScore -= amount;
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
