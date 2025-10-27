using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImageSwitcher : MonoBehaviour
{
    public GameObject canvasC; // "How to operate C" Canvas
    public GameObject canvasK; // "How to operate K" Canvas

    private void Start()
    {
        // �ŏ��ɕ\������Canvas��ݒ�
        ShowCanvas(canvasC);
    }

    private void Update()
    {
        // �E���L�[�������ꂽ�Ƃ�
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ShowCanvas(canvasK);
        }

        // �����L�[�������ꂽ�Ƃ�
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ShowCanvas(canvasC);
        }
    }

    private void ShowCanvas(GameObject canvasToShow)
    {
        // ���ׂĂ�Canvas���\���ɂ��A�w�肳�ꂽCanvas��\������
        canvasC.SetActive(false);
        canvasK.SetActive(false);
        canvasToShow.SetActive(true);
    }
}
