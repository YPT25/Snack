using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TitlePlayVideo : MonoBehaviour
{
    [Header("表示する素材(動画)")]
    [Tooltip("再生用のCanvas")]
    [SerializeField] RawImage move;
    [Tooltip("再生する動画")]
    [SerializeField] VideoPlayer videoPlayer;
    [Tooltip("何秒経過したら流れるか")]
    [SerializeField] float time = 3f;

    [Header("表示する素材(UI)")]
    [Tooltip("表示用のCanvas")]
    [SerializeField] Image ui_image;

    [Header("シーン遷移")]
    [SerializeField] string SceneName = "";

    //動画用
    private bool isVideoPlaying = false;
    //シーン遷移用
    private bool isScene = false;
    //フェイドにかかる時間
    private float FadeTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //アルファ値を0にする
        OffMoveAlpha();
        OffImageAlpha();
        videoPlayer.Stop();

        //時間がたったら関数を呼び出す
        Invoke(nameof(StartVideo), time);
        Invoke(nameof(StartInput), 3);       //３秒起ったらシーン遷移のクリックができるようになる
    }

    // クリックするまでの待機時間
    void StartInput()
    {
        isScene = true;
        OnImageAlpha();
        Debug.Log("シーン遷移できるようになりました");
    }

    //
    void StartVideo()
    {
        Debug.Log(time + "秒起ちました");
        isVideoPlaying = true;
    }

    //動画のアルファ値を０する
    void OffMoveAlpha()
    {
        Color color = move.color;
        color.a = 0f;
        move.color = color;
    }

    //UIのアルファ値を０する
    void OffImageAlpha()
    {
        Color color = ui_image.color;
        color.a = 0f;
        ui_image.color = color;
    }

    //動画のアルファ値を１する
    void OnMoveAlpha()
    {
        Color color = move.color;
        color.a = 1f;
        move.color = color;
    }

    //UIのアルファ値を１にする
    void OnImageAlpha()
    {
        Color color = ui_image.color;
        color.a = 1f;
        ui_image.color = color;
    }

    IEnumerator FadeOut()
    {
        Color color = move.color;
        // アルファ値を0から1へ変化させる
        for (float t = 0; t < FadeTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(0, 1, t / FadeTime);
            move.color = color;
            yield return null;
        }
        color.a = 1f;
        move.color = color;
    }

    IEnumerator FadeIn()
    {
        Color color = move.color;
        // アルファ値を1から0へ変化させる
        for (float t = 0; t < FadeTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(1, 0, t / FadeTime);
            move.color = color;
            yield return null;
        }
        color.a = 0f;
        move.color = color;
    }

    //動画の入力処理
    void VideoInput()
    {
        //カウント前に入力したらカウントをリセットする
        if (Input.anyKeyDown && isVideoPlaying == false)
        {
            Debug.Log("カウントがリセットされました");
            //カウントリセットする
            CancelInvoke(nameof(StartVideo));
            //再びカウントさせる
            Invoke(nameof(StartVideo), time);
        }
        else if (isVideoPlaying == true)
        {
            if (Input.anyKeyDown)
            {
                Debug.Log("入力がありました");
                //再生を止める
                videoPlayer.Stop();
                //再生時間を頭まで戻す
                videoPlayer.time = 0;
                isVideoPlaying = false;
                //アルファ値を0にする
                StartCoroutine(FadeIn());
                OnImageAlpha();
                // シーンのフラグをtrueにする
                isScene = true;
                //時間がたったら関数を呼び出す
                Invoke(nameof(StartVideo), time);
            }
            else
            {
                //　シーンのフラグをfalseにする
                isScene = false;
                //アルファ値を1にする
                OnMoveAlpha();
                OffImageAlpha();
                //StartCoroutine(FadeOut());
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
