using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEditor.SearchService;

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
    //動画用
    private bool isVideoPlaying = false;
    //シーン遷移用
    private bool isScene = false;

    // Start is called before the first frame update
    void Start()
    {
        //アルファ値を0にする
        OffPlayingAlpha();
        videoPlayer.Stop();

        //時間がたったら関数を呼び出す
        Invoke(nameof(StartVideo), time);
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
        Color c = image.color;
        c.a = 1f;
        image.color = c;
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
                //アルファ値を0にする
                isVideoPlaying = false;
                OffPlayingAlpha();
                //時間がたったら関数を呼び出す
                Invoke(nameof(StartVideo), time);
            }
            else
            {
                //アルファ値を1にする
                OnPlayingAlpha();
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
