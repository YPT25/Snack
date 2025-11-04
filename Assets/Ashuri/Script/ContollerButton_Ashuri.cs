using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerButton_Ashuri : MonoBehaviour
{
    [Header("最初に選択するボタン")]
    [Tooltip("ゲーム開始時にフォーカスを当てるボタン")]
    [SerializeField] private GameObject firstSelectedButton;

    [Header("ボタンが存在するCanvas")]
    [Tooltip("このCanvas内のボタンを自動認識します")]
    [SerializeField] private Canvas targetCanvas;

    [Header("再スキャン用ボタン")]
    [Tooltip("このボタンが押されたらCanvasのボタンを再スキャンします")]
    [SerializeField] private Button rescanButton;

    // --- Canvas内のボタンリスト ---
    private List<Button> canvasButtons = new List<Button>();

    // --- 現在選択中のボタン ---
    private GameObject currentSelectedButton;

    // --- 現在のインデックス ---
    private int currentIndex = 0;

    // --- 入力受付制御用 ---
    private bool canMove = true;

    // --- 自動スキャン用タイマー ---
    private float rescanInterval = 1.0f; // 1秒ごとにスキャン
    private float lastRescanTime = 0f;

    // --- 初期化処理 ---
    void Start()
    {
        // Canvas内のボタンを初回スキャン
        FindAllButtonsInCanvas();

        // 最初のボタンを選択状態に設定
        if (canvasButtons.Count > 0 && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            currentSelectedButton = firstSelectedButton;
            currentIndex = canvasButtons.FindIndex(b => b.gameObject == firstSelectedButton);
            if (currentIndex == -1) currentIndex = 0;
        }

        // 再スキャンボタンが設定されている場合のみイベントを登録
        if (rescanButton != null)
        {
            rescanButton.onClick.AddListener(OnRescanButtonPressed);
        }
        else
        {
            Debug.LogWarning("再スキャンボタンが指定されていません。Inspectorで設定してください。");
        }
    }

    // --- 毎フレームの処理 ---
    void Update()
    {
        // 一定間隔でCanvas内のボタンを自動的に再スキャン
        if (Time.time - lastRescanTime > rescanInterval)
        {
            lastRescanTime = Time.time;
            AutoRescanButtons();
        }

        // 現在選択されているボタンを更新
        currentSelectedButton = EventSystem.current.currentSelectedGameObject;

        // === コントローラー入力（左右・上下）===
        float horizontal = Input.GetAxisRaw("Horizontal Pad");
        float vertical = Input.GetAxisRaw("Vertical Pad");

        // 入力が有効なときにのみ移動処理を受け付ける
        if (canMove)
        {
            if (horizontal > 0.5f)
                MoveSelection(1);
            else if (horizontal < -0.5f)
                MoveSelection(-1);
        }

        // スティックが中央に戻ったら再び入力受付を許可
        if (Mathf.Abs(horizontal) < 0.1f)
            canMove = true;

        // === Aボタンで選択中のボタンを押す処理 ===
        if (Input.GetButtonDown("Submit"))
        {
            if (currentSelectedButton != null)
            {
                Button button = currentSelectedButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                    Debug.Log(currentSelectedButton.name + " が押されました");

                    // ボタン押下後にCanvasを再スキャンして新規ボタンも検出
                    FindAllButtonsInCanvas();
                }
            }
        }
    }

    // --- 再スキャンボタンが押されたときの処理 ---
    private void OnRescanButtonPressed()
    {
        Debug.Log("再スキャンボタンが押されました。Canvas内のボタンを再検出します。");
        FindAllButtonsInCanvas();
    }

    // --- Canvas内のボタンを探してリストに追加する処理 ---
    private void FindAllButtonsInCanvas()
    {
        if (targetCanvas == null)
        {
            Debug.LogWarning("ターゲットCanvasが設定されていません。");
            return;
        }

        // Canvas内のボタンを全て取得
        Button[] foundButtons = targetCanvas.GetComponentsInChildren<Button>(true);

        // 古いリストをクリアして新しい情報に更新
        canvasButtons.Clear();

        // 新しく見つけたボタンをリストに登録
        foreach (Button b in foundButtons)
        {
            if (b != null)
                canvasButtons.Add(b);
        }

        Debug.Log($"Canvas内で {canvasButtons.Count} 個のボタンを検出しました。");
    }

    // --- Canvas内のボタンが増減していたら自動でスキャンし直す処理 ---
    private void AutoRescanButtons()
    {
        if (targetCanvas == null) return;

        Button[] foundButtons = targetCanvas.GetComponentsInChildren<Button>(true);

        // 現在のボタン数と違っていたらリストを再作成
        if (foundButtons.Length != canvasButtons.Count)
        {
            Debug.Log("Canvas内のボタン数が変化したため自動再スキャンを実行します。");
            FindAllButtonsInCanvas();
        }
    }

    // --- 現在の選択ボタンを移動させる処理 ---
    private void MoveSelection(int direction)
    {
        // ボタンが存在しない場合は処理しない
        if (canvasButtons.Count == 0) return;

        // 1回の入力で1つだけ移動するように制御
        canMove = false;

        // 現在のインデックスを更新
        currentIndex += direction;

        // 範囲外になったらループさせる
        if (currentIndex >= canvasButtons.Count) currentIndex = 0;
        if (currentIndex < 0) currentIndex = canvasButtons.Count - 1;

        // 無効なボタン（削除済み）をスキップ
        while (canvasButtons[currentIndex] == null)
        {
            FindAllButtonsInCanvas();
            currentIndex = 0;
        }

        // 新しいボタンを選択状態に設定
        EventSystem.current.SetSelectedGameObject(canvasButtons[currentIndex].gameObject);
        currentSelectedButton = canvasButtons[currentIndex].gameObject;

        Debug.Log("選択中のボタン: " + currentSelectedButton.name);
    }
}
