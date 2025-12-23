using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraMove : NetworkBehaviour
{
    [Header("移動速度")]
    [Tooltip("カメラの向きに沿って移動する速度")]
    public float moveSpeed = 5f;

    [Header("マウス感度")]
    [Tooltip("マウス視点操作の感度")]
    public float mouseSensitivity = 3f;

    // カメラの上下回転角度
    private float rotationX = 0f;

    // プレイヤーの左右回転角度
    private float rotationY = 0f;

    // プレイヤー用カメラ
    private Camera playerCamera;

    // 開始時に一度だけ呼ばれる
    void Start()
    {
        // ローカルプレイヤーでなければ処理しない
        if (!isLocalPlayer) return;

        // メインカメラを取得
        playerCamera = Camera.main;

        // カメラが存在するか確認
        if (playerCamera != null)
        {
            // カメラをプレイヤーの子に設定
            playerCamera.transform.SetParent(transform);

            // カメラの位置を頭の高さに設定
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

        // 視点操作
        Look();

        // 移動処理
        Move();
    }

    // マウスで視点を操作する処理
    void Look()
    {
        // マウスの左右移動量を取得
        float mouseX = Input.GetAxis("Camera X") * mouseSensitivity;

        // マウスの上下移動量を取得
        float mouseY = Input.GetAxis("Camera Y") * mouseSensitivity;

        // プレイヤーの左右回転を加算
        rotationY += mouseX;

        // カメラの上下回転を加算
        rotationX -= mouseY;

        // 上下の回転制限
        rotationX = Mathf.Clamp(rotationX, -85f, 85f);

        // プレイヤー本体を左右に回転
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // カメラを上下に回転
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        }
    }

    // カメラの向いている方向へ移動する処理
    void Move()
    {
        // 前後入力を取得（W / S）
        float moveZ = Input.GetAxis("Vertical");

        // 左右入力を取得（A / D）
        float moveX = Input.GetAxis("Horizontal");

        // カメラの向いている前方向（上下成分を含む）
        Vector3 forward = playerCamera.transform.forward;

        // カメラ基準の右方向
        Vector3 right = playerCamera.transform.right;

        // 移動方向を計算（上下方向も含まれる）
        Vector3 moveDirection =
            forward * moveZ +
            right * moveX;

        // 斜め移動時の速度を一定にする
        moveDirection.Normalize();

        // プレイヤーを移動させる
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
