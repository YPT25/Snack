using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UIを操作するために必要
using Mirror; // Mirrorの機能を使用するために必要
using UnityEngine.SceneManagement; // シーン管理のために必要

public class FadeManager : NetworkBehaviour // NetworkBehaviourを継承
{
    public Image fadePanel; // フェード用のUIパネル
    public float fadeDuration = 1.5f; // フェードにかかる時間

    public static FadeManager Instance { get; private set; } // シングルトンパターン

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーン遷移してもこのオブジェクトが破棄されないようにする（必要に応じて）
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // フェードアウト（画面が暗くなる）してからシーンをロードするコルーチン
    public IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // パネルの色を取得
        Color panelColor = fadePanel.color;
        // アルファ値を0から1へ変化させる
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            panelColor.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadePanel.color = panelColor;
            yield return null; // 1フレーム待つ
        }
        panelColor.a = 1; // 完全に不透明にする
        fadePanel.color = panelColor;

        // シーン遷移（Mirrorを使用）
        if (NetworkServer.active) // サーバーの場合
        {
            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
        // クライアントの場合は、サーバーからのシーン変更を待つため、ここでは何もしない

        yield return new WaitForSeconds(0.5f); // シーンロードが完了するのを少し待つ

        // フェードイン（画面が明るくなる）
        StartCoroutine(FadeIn());
    }

    // フェードイン（画面が明るくなる）コルーチン
    public IEnumerator FadeIn()
    {
        Color panelColor = fadePanel.color;
        // アルファ値を1から0へ変化させる
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            panelColor.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = 0; // 完全に透明にする
        fadePanel.color = panelColor;
    }
}