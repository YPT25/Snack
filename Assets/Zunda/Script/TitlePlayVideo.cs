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

    [Header("ボタン")]
    [Tooltip("ゲーム開始するボタン")]
    [SerializeField] private Button _gameStartButton;

    [Tooltip("クレジットに行くボタン")]
    [SerializeField] private Button _staffSceneButton;

    //動画用
    private bool isVideoPlaying = false;
    //シーン遷移用
    private bool isScene = false;
    //フェイドにかかる時間
    private float FadeTime = 0.5f;

    [Header("SE")]
    [Header("ボタンを押した音")]
    [SerializeField] public AudioClip sound1;

    [Tooltip("AudioSource")]private AudioSource audioSource;

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

        // ホスト開始ボタン押下時のイベント登録
        _gameStartButton.onClick.AddListener(SceneInput);

        // ホスト開始ボタン押下時のイベント登録
        _staffSceneButton.onClick.AddListener(OnStaffSceneChange);

        //Componentを取得
        audioSource = GetComponent<AudioSource>();
    }

    // クリックするまでの待機時間
    public void StartInput()
    {
        isScene = true;
        OnImageAlpha();
        Debug.Log("シーン遷移できるようになりました");
    }

    //
    public void StartVideo()
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
                _gameStartButton.gameObject.SetActive(true);
                _staffSceneButton.gameObject.SetActive(true);
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
                _gameStartButton.gameObject.SetActive(false);
                _staffSceneButton.gameObject.SetActive(false);
                //StartCoroutine(FadeOut());
                //動画を再生する
                videoPlayer.Play();
            }
        }
    }

    //シーン遷移の入力処理
    void SceneInput()
    {
        //音(sound1)を鳴らす
        PlayButtonSE();
        //シーン遷移する
        SceneManager.LoadScene(SceneName);
    }

    // Update is called once per frame
    void Update()
    {
        //シーン遷移に関する入力処理
        //SceneInput();
        //動画に関する入力処理
        VideoInput();
    }

    private void OnGameStart()
    {

    }

    private void OnStaffSceneChange()
    {
        //音(sound1)を鳴らす
        PlayButtonSE();
        // シーン遷移する
        SceneManager.LoadScene("TextTestScene");
    }

    // ===============================
    // ★ 追加：SEだけを鳴らす関数
    // ===============================

    /// <summary>
    /// ボタン用SE再生
    /// 他CanvasのButtonから呼び出す想定
    /// </summary>
    public void PlayButtonSE()
    {
        // AudioSourceが存在しない場合は処理しない
        if (audioSource == null) return;

        // 効果音を1回再生
        audioSource.PlayOneShot(sound1);
    }
}
