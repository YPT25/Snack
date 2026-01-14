using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscQuit : MonoBehaviour
{
    [Header("ESCCanvas")]
    [Tooltip("ESC関連のCanvas")]
    [SerializeField] private Canvas _escCanvas;

    [Header("ゲーム終了ボタン")]
    [Tooltip("ゲーム終了するときのボタン")]
    [SerializeField] private Button _gameFinishButton;

    [Header("NormalCanvas")]
    [Tooltip("通常時に表示するCanvas一覧")]
    [SerializeField] private List<Canvas> _normalCanvasList = new List<Canvas>();

    // オプション画面が開いているかどうかを管理するフラグ
    private bool _isOptionOpen = false;

    // ゲーム開始時に呼ばれる
    private void Start()
    {
        // 初期状態ではオプション画面を非表示にする
        SetOptionUI(false);

        // ゲーム終了ボタンが押された時の処理を登録する
        _gameFinishButton.onClick.AddListener(GameFinishButton);
    }

    // 毎フレーム呼ばれるUnityの標準処理
    private void Update()
    {
        // ESCキーが押されたかをチェックする
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // オプション画面の表示状態を切り替える
            ToggleOption();
        }
    }

    /// <summary>
    /// オプション画面の表示・非表示を切り替える
    /// </summary>
    private void ToggleOption()
    {
        // 開いている状態を反転させる
        _isOptionOpen = !_isOptionOpen;

        // UIの表示状態を反映する
        SetOptionUI(_isOptionOpen);
    }

    /// <summary>
    /// オプションUI全体のON/OFFを管理する
    /// </summary>
    private void SetOptionUI(bool isShow)
    {        // 現在アクティブなシーン名を取得
        string currentSceneName = SceneManager.GetActiveScene().name;

            if (isShow)
        {
            // カーソルの設定  
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // シーン名が「GameScene」の場合
            if (currentSceneName == "GameScene" || currentSceneName == "LobbyScene" || currentSceneName == "3VS1ModeGame")
            {
                // マウスカーソルを画面中央に固定
                Cursor.lockState = CursorLockMode.Locked;

                // マウスカーソルを非表示
                Cursor.visible = false;
            }
        }

        // ESC用Canvasの表示を切り替える
        _escCanvas.gameObject.SetActive(isShow);

        // 通常時Canvasをすべて切り替える
        foreach (Canvas canvas in _normalCanvasList)
        {
            // nullチェックを行う
            if (canvas == null) continue;

            // 通常CanvasはESC表示中は非表示にする
            canvas.gameObject.SetActive(!isShow);
        }

        // ゲーム終了ボタンの表示を切り替える
        _gameFinishButton.gameObject.SetActive(isShow);
    }

    /// <summary>
    /// ゲーム終了ボタンが押されたときの処理
    /// </summary>
    private void GameFinishButton()
    {
#if UNITY_EDITOR
        // Unityエディタ上で実行している場合は再生を停止する
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルドされたアプリケーションを強制終了する
        Application.Quit();
#endif
    }
}
