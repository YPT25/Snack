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
    private GameOption_Tanabe m_gameOption;
    private float m_sensitivityPower = 5f;

    private float yaw = 0f;  // 横方向回転
    private float pitch = 10f; // 縦方向回転

    void Start()
    {
        m_player = GetComponentInParent<Player_Tanabe>();
        if(m_player == null || !m_player.isLocalPlayer) { return; }
        target = m_player.transform;
        Transform camera = GameObject.FindWithTag("MainCamera").transform;
        camera.parent = this.transform;
        camera.localPosition = new Vector3(0f, 0.35f, 0f);
        camera.localRotation = Quaternion.identity;
        Cursor.lockState = CursorLockMode.Locked;  // マウスロック

        m_gameOption = GameObject.Find("GameOption")?.GetComponent<GameOption_Tanabe>();
    }

    void LateUpdate()
    {
        if (m_player == null || !m_player.isLocalPlayer) { return; }

        if(m_gameOption != null && m_gameOption.IsChanged())
        {
            m_sensitivityPower = m_gameOption.GetCameraSensitivity();
        }

        //if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.C))
        //{
        //    Cursor.lockState = (CursorLockMode)(Math.Abs((int)Cursor.lockState - 1));
        //}

        if(m_gameOption != null && m_gameOption.IsPause())
        {
            // カメラの回転適用
            Quaternion _rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPosition = target.position + _rotation * offset;
            transform.position = desiredPosition;
            transform.LookAt(target.position + Vector3.up * 1.5f);  // プレイヤーの胸or頭あたり見るように
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked) { return; }

        float axisX = Input.GetAxis("Camera X");
        float axisY = Input.GetAxis("Camera Y");

        float axisPadX = Input.GetAxis("CameraPad X");
        float axisPadY = Input.GetAxis("CameraPad Y");

        if (Mathf.Abs(axisPadX) < 0.02f) axisPadX = 0f;
        if (Mathf.Abs(axisPadY) < 0.02f) axisPadY = 0f;

        if (axisPadX != 0f || axisPadY != 0f)
        {
            this.ViewUpdate(axisPadX, axisPadY, m_sensitivityPower);
        }
        else if (axisX != 0f || axisY != 0f)
        {
            this.ViewUpdate(axisX, axisY);
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

    private void ViewUpdate(float _axisX, float _axisY, float _sensitivityPower = 1f)
    {
        // マウス入力取得
        if (m_player.GetIsAiming() && Input.GetAxisRaw("Aiming Pad") != 0.0f)
        {
            yaw += _axisX * mouseSensitivity * 0.5f * _sensitivityPower;
            pitch -= _axisY * mouseSensitivity * 0.5f * _sensitivityPower;
        }
        else
        {
            yaw += _axisX * mouseSensitivity * _sensitivityPower;
            pitch -= _axisY * mouseSensitivity * _sensitivityPower;
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

    public void SetTarget(Transform _target)
    {
        target = _target;
    }
}
