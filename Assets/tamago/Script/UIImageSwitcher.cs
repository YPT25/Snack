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
        // 最初に表示するCanvasを設定
        ShowCanvas(canvasC);
    }

    private void Update()
    {
        // 右矢印キーが押されたとき
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ShowCanvas(canvasK);
        }

        // 左矢印キーが押されたとき
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ShowCanvas(canvasC);
        }
    }

    private void ShowCanvas(GameObject canvasToShow)
    {
        // すべてのCanvasを非表示にし、指定されたCanvasを表示する
        canvasC.SetActive(false);
        canvasK.SetActive(false);
        canvasToShow.SetActive(true);
    }
}
