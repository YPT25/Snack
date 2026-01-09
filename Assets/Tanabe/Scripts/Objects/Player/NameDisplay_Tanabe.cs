using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameDisplay_Tanabe : MonoBehaviour
{
    private GameObject m_camera;

    private void Start()
    {
        m_camera = GameObject.Find("Main Camera");
    }

    private void Update()
    {
        if (m_camera == null) { return; }

        // プレイヤーのY回転をカメラのY回転に合わせる
        Vector3 camForward = m_camera.transform.forward;
        //camForward.y = 0;
        if (camForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward.normalized);
            transform.rotation = targetRotation;
        }
    }

    // プレイヤーの名前の設定
    public void SetPlayerName(string _playerName)
    {
        var nameLabel = GetComponent<TextMeshPro>();
        // プレイヤー名を表示する
        nameLabel.text = _playerName;
    }

    // 名前のUIの色設定
    public void SetNameColor(Color _color)
    {
        var nameLabel = GetComponent<TextMeshPro>();
        nameLabel.color = _color;
    }
}
