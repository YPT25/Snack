using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
<<<<<<< HEAD

=======
//using UnityEditor.SearchService;
>>>>>>> origin/main

public class TitlePlayVideo : MonoBehaviour
{
    [Header("表示する素材")]
    [Tooltip("再生用のCanvas")]
    [SerializeField] RawImage image;
    [Tooltip("再生する動画")]
    [SerializeField] VideoPlayer videoPlayer;
    [Tooltip("何秒経過したら流れるか")]
    [SerializeField] float time = 3f;

    [Header("シーン遷移")]
    [SerializeField] string SceneName = "";

    //private FadeManager fadeManager;
    //動画用
    private bool isVideoPlaying = false;
    //シーン遷移用
    private bool isScene = false;
    private bool isFade = false;
    private float FadeTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //アルファ値を0にする
        OffPlayingAlpha();
        videoPlayer.Stop();

        //時間がたったら関数を呼び出す
        Invoke(nameof(StartVideo), time);
        Invoke(nameof(StartInput), 3);       //３秒起ったらシーン遷移のクリックができるようになる
    }

    // クリックするまでの待機時間
    void StartInput()
    {
        isScene = true;
        Debug.Log("シーン遷移できるようになりました");
    }

    void StartVideo()
    {
        Debug.Log(time + "秒起ちました");
        isVideoPlaying = true;
    }

    //動画のアルファ値を０する
    void OffPlayingAlpha()
    {
        Color color = image.color;
        color.a = 0f;
        image.color = color;
    }

    //動画のアルファ値を１する
    void OnPlayingAlpha()
    {
        Color color = image.color;
        color.a = 1f;
        image.color = color;
    }

    IEnumerator FadeOut()
    {
        Color color = image.color;
        // アルファ値を0から1へ変化させる
        for (float t = 0; t < FadeTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(0, 1, t / FadeTime);
            image.color = color;
            yield return null;
        }
        color.a = 1f;
        image.color = color;
    }

    IEnumerator FadeIn()
    {
        Color color = image.color;
        // アルファ値を1から0へ変化させる
        for (float t = 0; t < FadeTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(1, 0, t / FadeTime);
            image.color = color;
            yield return null;
        }
        color.a = 0f;
        image.color = color;
        isFade = false;
    }

    //動画の入力処理
    void VideoInput()
    {
        //
        if (isVideoPlaying == true)
        {
            if (Input.anyKeyDown)
            {
                Debug.Log("入力がありました");
                //再生を止める
                videoPlayer.Stop();
                //再生時間を頭まで戻す
                videoPlayer.time = 0;
                isVideoPlaying = false;
                isFade = true;
                //アルファ値を0にする
                StartCoroutine(FadeIn());
                //OffPlayingAlpha();
                //
                isScene = true;
                //時間がたったら関数を呼び出す
                Invoke(nameof(StartVideo), time);
            }
            else
            {
                //
                isScene = false;
                //アルファ値を1にする
                //OnPlayingAlpha();
                StartCoroutine(FadeOut());
                //動画を再生する
                videoPlayer.Play();
            }
        }
    }

    //シーン遷移の入力処理
    void SceneInput()
    {
        //左クリックを受け付ける
        if (isScene == true)
        {
            //マウスクリックとPADの〇×□▲に対応するのが反応したとき
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Joystick1Button0) ||
                Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.Joystick1Button2) ||
                Input.GetKeyDown(KeyCode.Joystick1Button3))
            {

                SceneManager.LoadScene(SceneName);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //シーン遷移に関する入力処理
        SceneInput();
        //動画に関する入力処理
        VideoInput();
    }
}
