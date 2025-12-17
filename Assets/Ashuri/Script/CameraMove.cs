using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraMove : NetworkBehaviour
{
    [Header("移動速度の設定")]
    [Tooltip("カメラの移動速度を調整します")]
    public float moveSpeed = 5f;

    [Header("マウス感度の設定")]
    [Tooltip("マウスで視点を回転させる速度を調整します")]
    public float mouseSensitivity = 10f;

    // 現在のカメラ回転角度を保存する変数
    private float rotationX = 0f;
    private float rotationY = 0f;
    // Start is called before the first frame update
    void Start()
    {
        // マウスカーソルを非表示にして画面中央に固定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ローカルプレイヤー初期設定
    public override void OnStartLocalPlayer()
    {
        // カメラ設定
        if (Camera.main != null)
        {
            Camera.main.transform.SetParent(transform); // カメラをプレイヤーの子に
            Camera.main.transform.localPosition = new Vector3(0, 3, -10); // カメラ位置調整
            Camera.main.transform.localRotation = Quaternion.Euler(15, 0, 0); // カメラの向きを少し下向きに調整する場合
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 自分のプレイヤーでなければ処理しない
        if (!isLocalPlayer) return;

        // カメラの移動処理
        MoveCamera();

        // カメラの回転処理
        RotateCamera();
    }

    void MoveCamera()
    {
        // WASD入力を取得（前後左右の移動）
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // カメラの向きを基準に移動方向を計算
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // 実際に移動させる
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    // カメラの回転処理
    void RotateCamera()
    {
        // マウスの入力を取得
        float mouseX = Input.GetAxis("Camera X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Camera Y") * mouseSensitivity;

        // 回転角度を更新
        rotationY += mouseX;
        rotationX -= mouseY;

        // 上下の回転角度を制限（見上げすぎ・見下ろしすぎ防止）
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // カメラの回転を適用
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}
