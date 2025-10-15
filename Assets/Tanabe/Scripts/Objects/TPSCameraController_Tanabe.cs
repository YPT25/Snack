using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPSCameraController_Tanabe : MonoBehaviour
{
    private Player_Tanabe m_player;
    [SerializeField] private Transform target;         // プレイヤー
    [SerializeField] private Vector3 aimingAdjustment = new Vector3(3f, 0f, 0f);
    [SerializeField] private Vector3 offset = new Vector3(0, 3, -6);
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minY = -35f;
    [SerializeField] private float maxY = 60f;
    [SerializeField] private float aimingMinY = -47f;
    [SerializeField] private float aimingMaxY = 40f;

    private float yaw = 0f;  // 横方向回転
    private float pitch = 10f; // 縦方向回転

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // マウスロック
        m_player = GetComponentInParent<Player_Tanabe>();
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.C))
        {
            Cursor.lockState = (CursorLockMode)(Math.Abs((int)Cursor.lockState - 1));
        }

        if (Cursor.lockState != CursorLockMode.Locked) { return; }

        float axisX = Input.GetAxis("Camera X");
        float axisY = Input.GetAxis("Camera Y");

        if (Mathf.Abs(axisX) < 0.02f) axisX = 0f;
        if (Mathf.Abs(axisY) < 0.02f) axisY = 0f;

        if (axisX != 0f || axisY != 0f)
        {
            // マウス入力取得
            if (m_player.GetIsAiming() && Input.GetAxisRaw("Aiming Pad") != 0.0f)
            {
                yaw += axisX * mouseSensitivity * 0.5f;
                pitch -= axisY * mouseSensitivity * 0.5f;
            }
            else
            {
                yaw += axisX * mouseSensitivity;
                pitch -= axisY * mouseSensitivity;
            }

            if (m_player.GetIsAiming())
            {
                pitch = Mathf.Clamp(pitch, aimingMinY, aimingMaxY);
            }
            else
            {
                pitch = Mathf.Clamp(pitch, minY, maxY);
            }
        }


        // カメラの回転適用
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        if (m_player.GetIsAiming())
        {
            Vector3 desiredPosition = target.position + rotation * offset * 0.5f;
            //desiredPosition += aimingAdjustment;
            transform.position = desiredPosition;
            Vector3 direction = target.position - desiredPosition;
            Vector3 aaa = target.position + direction * 1.0f + (rotation * aimingAdjustment);
            transform.LookAt(aaa);
        }
        else
        {
            Vector3 desiredPosition = target.position + rotation * offset;
            transform.position = desiredPosition;
            transform.LookAt(target.position + Vector3.up * 1.5f);  // プレイヤーの胸or頭あたり見るように
        }
    }
}
