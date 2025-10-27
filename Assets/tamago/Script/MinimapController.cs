using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public RawImage minimapImage; // �~�j�}�b�v��RawImage
    public Camera minimapCamera;  // �~�j�}�b�v�p�̃J����

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
            minimapImage.texture = renderTexture; // �~�j�}�b�v���X�V
        }
    }
}
