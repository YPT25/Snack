using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraMove : NetworkBehaviour
{
    [Header("移動速度の設定")]
    [Tooltip("プレイヤーの移動速度を調整します")]
    public float moveSpeed = 5f;

    [Header("マウス感度の設定")]
    [Tooltip("マウスで視点を回転させる速度を調整します")]
    public float mouseSensitivity = 3f;

    // 縦方向の回転角度（カメラ用）
    private float rotationX = 0f;

    // 横方向の回転角度（プレイヤー用）
    private float rotationY = 0f;

    // カメラ参照
    private Camera playerCamera;

    // 開始時の処理
    void Start()
    {
        // ローカルプレイヤーでなければ何もしない
        if (!isLocalPlayer) return;

        // メインカメラを取得
        playerCamera = Camera.main;

        // カメラが存在するか確認
        if (playerCamera != null)
        {
            // カメラをプレイヤーの子にする
            playerCamera.transform.SetParent(transform);

            // カメラの位置を設定
            playerCamera.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            // カメラの回転を初期化
            playerCamera.transform.localRotation = Quaternion.identity;
        }

        // マウスカーソルを画面中央に固定
        Cursor.lockState = CursorLockMode.Locked;

        // マウスカーソルを非表示
        Cursor.visible = false;
    }

    // 毎フレーム呼ばれる
    void Update()
    {
        // ローカルプレイヤーでなければ処理しない
        if (!isLocalPlayer) return;

        // 移動処理
        Move();

        // 視点回転処理
        Look();
    }

    // プレイヤーの移動処理
    void Move()
    {
        // 横移動入力を取得
        float moveX = Input.GetAxis("Horizontal");

        // 前後移動入力を取得
        float moveZ = Input.GetAxis("Vertical");

        // 移動方向を計算
        Vector3 move =
            transform.right * moveX +
            transform.forward * moveZ;

        // プレイヤーを移動させる
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    // 視点回転処理
    void Look()
    {
        // マウスの横移動量を取得
        float mouseX = Input.GetAxis("Camera X") * mouseSensitivity;

        // マウスの縦移動量を取得
        float mouseY = Input.GetAxis("Camera Y") * mouseSensitivity;

        // 横回転を加算（プレイヤー）
        rotationY += mouseX;

        // 縦回転を加算（カメラ）
        rotationX -= mouseY;

        // 縦回転の制限
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        // プレイヤーの横回転を適用
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // カメラの縦回転を適用
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        }
    }
}
