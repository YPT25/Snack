using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPSCameraController_Tanabe : MonoBehaviour
{
    private CharacterBase m_player;
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
        this.transform.parent = null;
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
            // ・ｽJ・ｽ・ｽ・ｽ・ｽ・ｽﾌ会ｿｽ]・ｽK・ｽp
            Quaternion _rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPosition1 = target.position + _rotation * offset;
            transform.position = desiredPosition1;
            transform.LookAt(target.position + Vector3.up * 1.5f);  // ・ｽv・ｽ・ｽ・ｽC・ｽ・ｽ・ｽ[・ｽﾌ具ｿｽor・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ闌ｩ・ｽ・ｽ謔､・ｽ・ｽ
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
            this.ViewUpdate(axisPadX * Mathf.Abs(axisPadX), axisPadY * Mathf.Abs(axisPadY), m_sensitivityPower * 20f);
        }
        else if (axisX != 0f || axisY != 0f)
        {
            this.ViewUpdate(axisX, axisY);
        }


        // カメラの回転適用
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = desiredPosition;
        Vector3 hitDistance = Vector3.one;
        float up = 1f;
        // カメラがステージオブジェクトにぶつかっているか調べる
        if (this.PositionAdjustment(desiredPosition, out hitDistance, out up))
        {
            transform.position = target.position + hitDistance;
            transform.LookAt(target.position + Vector3.up * 1.5f * up);  // プレイヤーの胸or頭あたり見るように
        }
        else
        {
            transform.LookAt(target.position + Vector3.up * 1.5f);  // プレイヤーの胸or頭あたり見るように
        }
    }

    // 視点の更新
    private void ViewUpdate(float _axisX, float _axisY, float _sensitivityPower = 1f)
    {
        // マウス入力取得
        yaw += _axisX * mouseSensitivity * _sensitivityPower;
        pitch -= _axisY * mouseSensitivity * _sensitivityPower;

        pitch = Mathf.Clamp(pitch, minY, maxY);
    }

    // カメラがステージにぶつかっていたら位置を調整する
    private bool PositionAdjustment(Vector3 _cameraPosition, out Vector3 _hitDistance, out float _up)
    {
        Vector3 direction = _cameraPosition - target.transform.position;
        float maxDistance = Vector3.Distance(_cameraPosition, target.transform.position);
        float minDistance = maxDistance;
        RaycastHit[] hits = Physics.RaycastAll(target.transform.position, direction.normalized, minDistance);
        Vector3 distance = direction.normalized * minDistance;
        bool isHit = false;
        for (int i = 0; i < hits.Length; i++)
        {
            // ステージオブジェクトのみ調べる
            if (hits[i].collider.gameObject.layer != 3) { continue; }
            // ぶつかったオブジェクトの中でプレイヤーに最も距離が近い物を参照する
            if(minDistance >= hits[i].distance)
            {
                distance = direction.normalized * hits[i].distance;
                minDistance = hits[i].distance;
                isHit = true;
            }
        }

        _hitDistance = distance;
        _up = minDistance / maxDistance;
        return isHit;
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }
}
