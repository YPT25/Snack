using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム開始前カウントダウンをUIに表示するクラス
/// ・GameManagerのカウントダウン秒数を取得
/// ・3 → 2 → 1 → GO の順でスプライトを切り替え
/// ・数字・GOを徐々に大きくするアニメーション付き
/// </summary>
public class CountDownScript : MonoBehaviour
{
    [Header("カウントダウン画像")]
    [Tooltip("3 → 2 → 1 の順で表示するスプライト")]
    [SerializeField] private Sprite sprite3;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private Sprite sprite1;

    [Header("GO表示用スプライト")]
    [Tooltip("GO表記に使うスプライト")]
    [SerializeField] private Sprite spriteGo;

    [Header("表示先Image")]
    [Tooltip("カウントダウンを表示するUIのImage")]
    [SerializeField] private Image countDownImage;

    [Header("アニメーション設定")]
    [Tooltip("1秒でどれだけ大きくなるかの倍率")]
    [SerializeField] private float scaleMultiplier = 1.5f;

    // 元のスケールを保持
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = countDownImage.transform.localScale;

        // GameManager が生成されるまで待機
        StartCoroutine(WaitForGameManagerAndStart());
    }

    private IEnumerator WaitForGameManagerAndStart()
    {
        while (GameManager.Instance == null)
            yield return null;

        StartCoroutine(CountDownRoutine());
    }

    private IEnumerator CountDownRoutine()
    {
        float countdown = GameManager.Instance.preGameCountdownTime;

        Debug.Log($"CountDownRoutine started! countdown={countdown}");

        while (countdown > 0)
        {
            int displayNum = Mathf.CeilToInt(countdown);

            // 数値に応じてスプライト切替
            switch (displayNum)
            {
                case 3: countDownImage.sprite = sprite3; break;
                case 2: countDownImage.sprite = sprite2; break;
                case 1: countDownImage.sprite = sprite1; break;
            }

            countDownImage.gameObject.SetActive(true);

            // スケールアニメーション
            yield return StartCoroutine(ScaleUpAnimation(1f));

            countdown -= 1f;
        }

        // GO表示
        countDownImage.sprite = spriteGo;
        countDownImage.gameObject.SetActive(true);

        yield return StartCoroutine(ScaleUpAnimation(1f));

        // 表示を消す
        countDownImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 1秒かけてスケールを元のサイズ → scaleMultiplier倍 に変化させるコルーチン
    /// </summary>
    /// <param name="duration">変化にかける時間（秒）</param>
    private IEnumerator ScaleUpAnimation(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // 0〜1の割合
            float t = Mathf.Clamp01(timer / duration);

            // 線形補間でスケール変更
            countDownImage.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleMultiplier, t);

            yield return null;
        }

        // 最終的に正確に scaleMultiplier にする
        countDownImage.transform.localScale = originalScale * scaleMultiplier;

        // アニメ終了後に元のスケールに戻す
        countDownImage.transform.localScale = originalScale;
    }
}
