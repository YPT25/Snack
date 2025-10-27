using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public RawImage minimapImage; // ミニマップのRawImage
    public Camera minimapCamera;  // ミニマップ用のカメラ

    void Start()
    {
        if (minimapCamera != null)
        {
            minimapCamera.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (minimapImage != null && minimapCamera != null)
        {
            RenderTexture renderTexture = minimapCamera.targetTexture;
            minimapImage.texture = renderTexture; // ミニマップを更新
        }
    }
}
