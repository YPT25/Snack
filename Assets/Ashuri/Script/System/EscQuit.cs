using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscQuit : MonoBehaviour
{
    // ===============================
    // Instance（シングルトン）
    // ===============================

    // どこからでも参照できるInstance
    public static EscQuit Instance { get; private set; }

    // ===============================
    // UI関連
    // ===============================

    [Header("ESCCanvas")]
    [Tooltip("ESC関連のCanvas")]
    [SerializeField] private Canvas _escCanvas;

    [Header("ゲーム終了ボタン")]
    [Tooltip("ゲーム終了するときのボタン")]
    [SerializeField] private Button _gameFinishButton;

    [Header("NormalCanvas")]
    [Tooltip("通常時に表示するCanvas一覧")]
    [SerializeField] private List<Canvas> _normalCanvasList = new List<Canvas>();

    // ===============================
    // 入力制御
    // ===============================

    // プレイヤー操作を許可するかどうか
    public bool canInput = true;

    // オプション画面が開いているかどうか
    public bool _isOptionOpen = false;

    // ===============================
    // Unity標準処理
    // ===============================

    // オブジェクト生成時に呼ばれる
    private void Awake()
    {
        // すでにInstanceが存在する場合は自分を破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Instanceを設定
        Instance = this;
    }

    // ゲーム開始時に呼ばれる
    private void Start()
    {
        // 初期状態ではオプション画面を非表示
        SetOptionUI(false);

        // ゲーム終了ボタンに処理を登録
        _gameFinishButton.onClick.AddListener(GameFinishButton);
    }

    // 毎フレーム呼ばれる
    private void Update()
    {
        // 入力が無効な場合は何もしない
        if (!canInput) return;

        // ESCキーが押されたかチェック
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // オプション画面の表示切り替え
            ToggleOption();
        }
    }

    // ===============================
    // UI制御
    // ===============================

    /// <summary>
    /// オプション画面の表示・非表示を切り替える
    /// </summary>
    private void ToggleOption()
    {
        // 状態を反転
        _isOptionOpen = !_isOptionOpen;

        // UI状態を反映
        SetOptionUI(_isOptionOpen);
    }

    /// <summary>
    /// オプションUI全体のON/OFF制御
    /// </summary>
    private void SetOptionUI(bool isShow)
    {
        // 現在のシーン名を取得
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (isShow)
        {
            // カーソル固定を解除
            Cursor.lockState = CursorLockMode.None;

            // カーソル表示
            Cursor.visible = true;
        }
        else
        {
            // 指定シーンの場合のみカーソルをロック
            if (currentSceneName == "GameScene" ||
                currentSceneName == "LobbyScene" ||
                currentSceneName == "3VS1ModeGame")
            {
                // カーソルを中央に固定
                Cursor.lockState = CursorLockMode.Locked;

                // カーソル非表示
                Cursor.visible = false;
            }
        }

        // ESC用Canvasの表示切り替え
        _escCanvas.gameObject.SetActive(isShow);

        // 通常Canvasの表示切り替え
        foreach (Canvas canvas in _normalCanvasList)
        {
            // nullチェック
            if (canvas == null) continue;

            // ESC表示中は通常Canvasを非表示
            canvas.gameObject.SetActive(!isShow);
        }

        // ゲーム終了ボタンの表示切り替え
        _gameFinishButton.gameObject.SetActive(isShow);
    }

    // ===============================
    // 外部操作用メソッド
    // ===============================

    /// <summary>
    /// 外部から入力のON/OFFを切り替える
    /// </summary>
    public void SetCanInput(bool value)
    {
        // 入力許可状態を変更
        canInput = value;
    }

    // ===============================
    // ゲーム終了処理
    // ===============================

    /// <summary>
    /// ゲーム終了ボタンが押された時の処理
    /// </summary>
    private void GameFinishButton()
    {
#if UNITY_EDITOR
        // エディタ再生停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // アプリケーション終了
        Application.Quit();
#endif
    }
}
