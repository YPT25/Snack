// コルーチンを使用するために必要
using System.Collections;

// Unityの基本機能を使用するために必要
using UnityEngine;

// UI(Image)を操作するために必要
using UnityEngine.UI;

// Mirrorのネットワーク機能を使用するために必要
using Mirror;

// フェード処理を管理するクラス
public class FadeManager : MonoBehaviour
{
    // ==============================
    // フェード用の黒いImageを指定
    // ==============================
    [SerializeField]
    private Image fadePanel;

    // ==============================
    // フェードにかかる時間（秒）
    // ==============================
    [SerializeField]
    private float fadeDuration = 1.5f;

    // ==============================
    // シングルトン用のインスタンス
    // ==============================
    public static FadeManager Instance;

    // ==============================
    // オブジェクト生成時に一度だけ呼ばれる
    // ==============================
    private void Awake()
    {
        // まだインスタンスが存在しない場合
        if (Instance == null)
        {
            // 自分自身をシングルトンとして登録
            Instance = this;

            // シーンが変わっても破棄されないようにする
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // すでに存在している場合は自分を破棄
            Destroy(gameObject);
        }
    }

    // ==============================
    // フェードアウトを開始する外部用メソッド
    // ==============================
    public void FadeOut(string sceneName)
    {
        // フェードアウト用コルーチンを開始
        StartCoroutine(FadeOutCoroutine(sceneName));
    }

    // ==============================
    // フェードアウト処理本体
    // ==============================
    private IEnumerator FadeOutCoroutine(string sceneName)
    {
        // 現在のImageの色を取得
        Color color = fadePanel.color;

        // 指定時間かけてアルファ値を0→1に変化させる
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            // 時間経過に応じてアルファ値を補間
            color.a = Mathf.Lerp(0f, 1f, t / fadeDuration);

            // Imageに反映
            fadePanel.color = color;

            // 次のフレームまで待つ
            yield return null;
        }

        // 最終的に完全な黒にする
        color.a = 1f;
        fadePanel.color = color;

        // サーバーの場合のみシーンを変更する
        if (NetworkServer.active)
        {
            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
    }

    // ==============================
    // フェードインを開始する外部用メソッド
    // ==============================
    public void FadeIn()
    {
        // フェードイン用コルーチンを開始
        StartCoroutine(FadeInCoroutine());
    }

    // ==============================
    // フェードイン処理本体
    // ==============================
    private IEnumerator FadeInCoroutine()
    {
        // 現在のImageの色を取得
        Color color = fadePanel.color;

        // 指定時間かけてアルファ値を1→0に変化させる
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            // 時間経過に応じてアルファ値を補間
            color.a = Mathf.Lerp(1f, 0f, t / fadeDuration);

            // Imageに反映
            fadePanel.color = color;

            // 次のフレームまで待つ
            yield return null;
        }

        // 最終的に完全に透明にする
        color.a = 0f;
        fadePanel.color = color;
    }
}
