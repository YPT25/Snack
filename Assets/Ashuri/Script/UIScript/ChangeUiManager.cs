using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeUiManager : MonoBehaviour
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
        // キーが押されたとき
        if (Input.anyKeyDown)
        {
            ShowCanvas(canvasK);
        }

        // コントローラーのボタンが押された瞬間をチェック
        if (CheckControllerButtonDown())
        {
            // ボタンの入力があったときの処理
            ShowCanvas(canvasC);
        }

        // コントローラーのスティックが動いた瞬間をチェック
        if (CheckControllerStickMoved())
        {
            // スティック入力があったときの処理
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

    // コントローラーのボタン入力をチェックする
    private bool CheckControllerButtonDown()
    {
        // よく使うゲームパッドボタンを確認
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) return true; // A / ○
        if (Input.GetKeyDown(KeyCode.JoystickButton1)) return true; // B / ×
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) return true; // X / □
        if (Input.GetKeyDown(KeyCode.JoystickButton3)) return true; // Y / △

        if (Input.GetKeyDown(KeyCode.JoystickButton4)) return true; // LB
        if (Input.GetKeyDown(KeyCode.JoystickButton5)) return true; // RB
        if (Input.GetKeyDown(KeyCode.JoystickButton6)) return true; // Back
        if (Input.GetKeyDown(KeyCode.JoystickButton7)) return true; // Start

        if (Input.GetKeyDown(KeyCode.JoystickButton8)) return true; // Lスティック押し込み
        if (Input.GetKeyDown(KeyCode.JoystickButton9)) return true; // Rスティック押し込み

        if (Input.GetAxisRaw("Shot") != 0.0f) return true;
        if (Input.GetAxisRaw("Aiming Pad") != 0.0f) return true;


        return false;
    }

    // コントローラーのスティック入力をチェックする
    private bool CheckControllerStickMoved()
    {
        // 左スティックの入力を取得
        float lx = Input.GetAxis("Horizontal Pad");
        float ly = Input.GetAxis("Vertical Pad");

        // 右スティック（設定してある場合）
        float rx = Input.GetAxis("CameraPad X");
        float ry = Input.GetAxis("CameraPad Y");

        // どれかが少しでも動いたら入力扱いにする
        if (Mathf.Abs(lx) > 0.1f) return true;
        if (Mathf.Abs(ly) > 0.1f) return true;
        if (Mathf.Abs(rx) > 0.1f) return true;
        if (Mathf.Abs(ry) > 0.1f) return true;

        return false;
    }
}
