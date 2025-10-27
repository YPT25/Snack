using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // プレイヤーの移動速度

    void Update()
    {
        // 入力を取得
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; // A/D または 左/右矢印キー
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime; // W/S または 上/下矢印キー

        // プレイヤーの位置を更新
        transform.Translate(moveX, 0, moveZ);
    }
}
