using UnityEngine;
using System.Runtime.InteropServices;

public class ControllerMouse : MonoBehaviour
{
    // ===============================
    // Windowsのマウス操作API
    // ===============================
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(
    int dwFlags,
    int dx,
    int dy,
    int dwData,
    int dwExtraInfo
);

    // ===============================
    // マウス左クリック用定数
    // ===============================
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;    

    // ===============================
    // 移動速度
    // ===============================
    [Header("カーソル移動速度")]
    [Tooltip("スティック操作時のマウス移動速度")]
    [SerializeField] private float cursorSpeed = 800f;

    // ===============================
    // クリック状態管理
    // ===============================
    private bool isClicking = false;

    // ===============================
    // マウス座標
    // ===============================
    private Vector2 mousePosition;

    void Start()
    {
        // ===============================
        // 現在のマウス位置を取得
        // ===============================
        mousePosition = Input.mousePosition;
    }

    void Update()
    {
        // ===============================
        // コントローラースティック入力取得
        // ===============================
        float h = Input.GetAxis("Horizontal Pad");
        float v = Input.GetAxis("Vertical Pad");

        // ===============================
        // スティックが倒されていない場合は何もしない
        // ===============================
        //if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
        //{
        //    return;
        //}

        // ===============================
        // マウス位置更新
        // ===============================
        mousePosition.x += h * cursorSpeed * Time.deltaTime;
        mousePosition.y += v * cursorSpeed * Time.deltaTime;

        // ===============================
        // 画面外制限
        // ===============================
        mousePosition.x = Mathf.Clamp(mousePosition.x, 0, Screen.width);
        mousePosition.y = Mathf.Clamp(mousePosition.y, 0, Screen.height);

        // ===============================
        // カーソル移動（この時だけ強制）
        // ===============================
        SetCursorPos((int)mousePosition.x, Screen.height - (int)mousePosition.y);

        // ===============================
        // クリック操作
        // ===============================
        if (Input.GetButtonDown("Jump") && !isClicking)
        {
            // ===============================
            // 左ボタン押下
            // ===============================
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);

            // ===============================
            // クリック中フラグON
            // ===============================
            isClicking = true;
        }

        // ===============================
        // Aボタンを離した瞬間
        // ===============================
        if (Input.GetButtonUp("Jump") && isClicking)
        {
            // ===============================
            // 左ボタン解放
            // ===============================
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

            // ===============================
            // クリック中フラグOFF
            // ===============================
            isClicking = false;
        }
    }
}
