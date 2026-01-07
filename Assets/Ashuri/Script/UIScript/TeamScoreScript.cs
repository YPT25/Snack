using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamScoreScript : MonoBehaviour
{
    [Header("数字0～9に対応するスプライト")]
    [Tooltip("インデックス 0 が数字0、1 が数字1 ... 9 が数字9 になるように設定してください。")]
    [SerializeField] private List<Sprite> numberSprites = new List<Sprite>();

    [Header("各桁の Image（左 -> 右 の順）")]
    [Tooltip("images[0] が最上位桁（左端）になります。桁数分登録してください。")]
    [SerializeField] private List<Image> images = new List<Image>();

    // 3人チームのスコア
    private float _teamScore;

    // プレイヤー情報
    private Player_Tanabe _localPlayer;

    void Start()
    {
        // ゲーム開始時にスコア表示を更新
        UpdateScoreDisplay();
    }

    void Update()
    {
        // 現状は未使用（必要ならキーでの増減等をここで行えます）
    }

    /// <summary>
    /// スコアを加算する関数
    /// </summary>
    /// <param name="amount">加算する値（float）</param>
    public void AddScore(float amount)
    {
        // 現在のスコアに加算（内部は float のまま）
        _teamScore += amount;

        // 加算後に表示を更新
        UpdateScoreDisplay();
    }

    /// <summary>
    /// スコアを画像で表示する（不足桁は 0 で埋める）
    /// </summary>
    private void UpdateScoreDisplay()
    {
        // numberSprites が 10 個揃っているかチェック
        if (numberSprites == null || numberSprites.Count < 10)
        {
            Debug.LogWarning("numberSprites に 0～9 のスプライトが揃っていません。Inspector を確認してください。");
            return;
        }

        // images リストが設定されているかチェック
        if (images == null || images.Count == 0)
        {
            Debug.LogWarning("images に桁分の Image が登録されていません。Inspector を確認してください。");
            return;
        }

        // スコアを非負整数に変換（小数は切り捨て）。負値は 0 に補正。
        int scoreInt = Mathf.Max(0, (int)_teamScore);

        // 表示用に文字列化し、左側を 0 で埋めて images.Count 桁にする（例：100 -> "0100"）
        string scoreStr = scoreInt.ToString().PadLeft(images.Count, '0');

        // 各 Image を左->右 の順で更新
        for (int i = 0; i < images.Count; i++)
        {
            // 対応桁の文字を取得（images[0] は最上位桁＝scoreStr[0]）
            char c = scoreStr[i];

            // 文字が数字であることを確認（念のため）
            if (c < '0' || c > '9')
            {
                Debug.LogWarning($"桁文字が数字ではありません: {c}");
                // 数字以外だったら 0 を設定
                images[i].sprite = numberSprites[0];
                images[i].enabled = true;
                continue;
            }

            // 文字を数字に変換
            int digit = c - '0';

            // 対応するスプライトを設定
            images[i].sprite = numberSprites[digit];

            // Image を有効にする（非表示にしない）
            images[i].enabled = true;
        }
    }
}

