using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerNameButton_Ashuri : MonoBehaviour
{
    [Header("最初に選択するボタン")]
    [Tooltip("ゲーム開始時にフォーカスを当てるボタン")]
    [SerializeField] private GameObject firstSelectedButton;

    [Header("ボタンが存在するCanvas")]
    [Tooltip("このCanvas内のボタンを自動認識します")]
    [SerializeField] private Canvas targetCanvas;

    [Header("名前入力するボタンCanvas")]
    [Tooltip("このCanvas内の文字ボタンを自動認識します")]
    [SerializeField] private Canvas StringCanvas;

    [Header("ネットワークに戻るボタン")]
    [Tooltip("ネットワークモードに戻るためのボタン")]
    [SerializeField] private Button BackButton;

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

        // 最初に選択するボタンを設定
        if (canvasButtons.Count > 0 && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            currentSelectedButton = firstSelectedButton;
            currentIndex = canvasButtons.FindIndex(b => b.gameObject == firstSelectedButton);
            if (currentIndex == -1) currentIndex = 0;
        }

        // BackButtonが設定されていない場合は警告を出す
        if (BackButton == null)
        {
            Debug.LogWarning("BackButtonが指定されていません。Inspectorで設定してください。");
        }
    }

    // --- 毎フレームごとの処理 ---
    void Update()
    {
        // 一定時間ごとにCanvas内のボタンを自動的に再スキャン
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

        // 入力受付が有効な場合にのみ上下移動を処理
        if (canMove)
        {
            if (vertical > 0.5f)
                MoveSelection(-1); // 上に移動
            else if (vertical < -0.5f)
                MoveSelection(1); // 下に移動
        }

        // スティックが中央に戻ったら再度入力を受付
        if (Mathf.Abs(vertical) < 0.1f)
            canMove = true;

        // Aボタン（Submit）で現在選択中のボタンを押す処理
        if (Input.GetButtonDown("Submit"))
        {
            if (currentSelectedButton != null)
            {
                Button button = currentSelectedButton.GetComponent<Button>();
                if (button != null)
                {
                    // ボタンのクリックイベントを実行
                    button.onClick.Invoke();
                    Debug.Log(currentSelectedButton.name + " が押されました");

                    // 押した後にCanvasのボタンを再スキャン
                    FindAllButtonsInCanvas();
                }
            }
        }
    }

    // --- 画面が有効化されたときに最初のボタンを自動選択 ---
    void OnEnable()
    {
        // 移動入力をすぐに受け付けられるようにリセット
        canMove = true;

        // 少し遅らせて選択する（UIの初期化が終わるまで待つため）
        StartCoroutine(SelectFirstButtonNextFrame());
    }

    // --- 次のフレームで最初のボタンを選択するコルーチン ---
    private System.Collections.IEnumerator SelectFirstButtonNextFrame()
    {
        // 1フレーム待つ（Canvas切り替え後にUI初期化を完了させるため）
        yield return null;

        // 最初のボタンが設定されていれば選択状態にする
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            Debug.Log($"[{gameObject.name}] 最初のボタン {firstSelectedButton.name} を選択しました。");
        }
    }

    // --- Canvas内のボタンを探してリストに追加する処理 ---
    private void FindAllButtonsInCanvas()
    {
        // Canvasが設定されていない場合は警告を出す
        if (targetCanvas == null)
        {
            Debug.LogWarning("ターゲットCanvasが設定されていません。");
            return;
        }

        // Canvas内のボタンをすべて取得
        Button[] foundButtons = targetCanvas.GetComponentsInChildren<Button>(true);

        // 古いリストをクリア
        canvasButtons.Clear();

        // 新しく見つけたボタンを登録
        foreach (Button b in foundButtons)
        {
            if (b != null)
                canvasButtons.Add(b);
        }

        // 検出されたボタン数をログ表示
        Debug.Log($"Canvas内で {canvasButtons.Count} 個のボタンを検出しました。");
    }

    // --- Canvas内のボタンが増減していたら自動で再スキャンする処理 ---
    private void AutoRescanButtons()
    {
        // Canvasが設定されていない場合はスキップ
        if (targetCanvas == null) return;

        // 現在のボタン一覧を取得
        Button[] foundButtons = targetCanvas.GetComponentsInChildren<Button>(true);

        // ボタン数が変化していたら再スキャンを実行
        if (foundButtons.Length != canvasButtons.Count)
        {
            Debug.Log("Canvas内のボタン数が変化したため、自動再スキャンを実行します。");
            FindAllButtonsInCanvas();
        }
    }

    // --- 現在の選択ボタンを移動させる処理 ---
    private void MoveSelection(int direction)
    {
        // ボタンが存在しない場合は処理を行わない
        if (canvasButtons.Count == 0) return;

        // 1回の入力で1つだけ移動できるようにする
        canMove = false;

        // 現在のインデックスを更新
        currentIndex += direction;

        // インデックスが範囲外になったらループさせる
        if (currentIndex >= canvasButtons.Count) currentIndex = 0;
        if (currentIndex < 0) currentIndex = canvasButtons.Count - 1;

        // 無効なボタン（削除済みなど）があればスキップ
        while (canvasButtons[currentIndex] == null)
        {
            FindAllButtonsInCanvas();
            currentIndex = 0;
        }

        // 新しいボタンを選択状態に設定
        EventSystem.current.SetSelectedGameObject(canvasButtons[currentIndex].gameObject);
        currentSelectedButton = canvasButtons[currentIndex].gameObject;

        // 現在選択中のボタン名をログ出力
        Debug.Log("選択中のボタン: " + currentSelectedButton.name);
    }
}
