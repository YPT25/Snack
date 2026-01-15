using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerScript : MonoBehaviour
{
    // ===============================
    // AudioMixer
    // ===============================
    [Header("AudioMixer")]
    [Tooltip("AudioMixerをアタッチ")]
    [SerializeField] private AudioMixer audioMixer;

    // ===============================
    // スライダー
    // ===============================
    [Header("スライダー")]
    [Tooltip("BGMの音量調節をするスライダー")]
    [SerializeField] private Slider _bgmSlider;

    [Tooltip("SEの音量調節をするスライダー")]
    [SerializeField] private Slider _seSlider;

    // ===============================
    // PlayerPrefsのキー
    // ===============================
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";
    private const string SE_VOLUME_KEY = "SE_VOLUME";

    void Start()
    {
        // -------------------------------
        // 保存されているBGM音量を読み込む
        // -------------------------------
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);

        // -------------------------------
        // 保存されているSE音量を読み込む
        // -------------------------------
        float seVolume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1.0f);

        // -------------------------------
        // BGMスライダーとAudioMixerを初期化
        // -------------------------------
        if (_bgmSlider != null)
        {
            _bgmSlider.value = bgmVolume;
            SetBGMVolume(bgmVolume);

            _bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        // -------------------------------
        // SEスライダーとAudioMixerを初期化
        // -------------------------------
        if (_seSlider != null)
        {
            _seSlider.value = seVolume;
            SetSEVolume(seVolume);

            _seSlider.onValueChanged.AddListener(SetSEVolume);
        }
    }

    // ===============================
    // BGM音量を設定する処理
    // ===============================
    private void SetBGMVolume(float value)
    {
        // 音量を0～1に制限
        value = Mathf.Clamp01(value);

        // 0対策（-Infinity防止）
        float decibel = value > 0
            ? 20f * Mathf.Log10(value)
            : -80f;

        // AudioMixerに反映
        audioMixer.SetFloat("BGMVolume", decibel);

        // PlayerPrefsに保存
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, value);
    }

    // ===============================
    // SE音量を設定する処理
    // ===============================
    private void SetSEVolume(float value)
    {
        // 音量を0～1に制限
        value = Mathf.Clamp01(value);

        // 0対策（-Infinity防止）
        float decibel = value > 0
            ? 20f * Mathf.Log10(value)
            : -80f;

        // AudioMixerに反映
        audioMixer.SetFloat("SEVolume", decibel);

        // PlayerPrefsに保存
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, value);
    }
}
