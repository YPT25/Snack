using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerScript : MonoBehaviour
{
    [Header("AudioMixer")]
    [Tooltip("AudioMixerをアタッチ")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("スライダー")]
    [Tooltip("BGMの音量調節をするスライダー")]
    [SerializeField] private Slider _bgmSlider;

    // Start is called before the first frame update
    void Start()
    {
        if(_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.AddListener((value) =>
            {
                value = Mathf.Clamp01(value);

                float decibel = 20f * Mathf.Log10(value);

                decibel = Mathf.Clamp(decibel, -80f, 0f);
                audioMixer.SetFloat("BGMVolume", decibel);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
