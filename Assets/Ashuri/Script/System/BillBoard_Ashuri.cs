using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard_Ashuri : MonoBehaviour
{
    void Update()
    {
        // カメラの位置を取得する
        Vector3 p = Camera.main.transform.position;

        // 高さだけはオブジェクト自身に合わせて上下を向かないようにする
        p.y = transform.position.y;

        // オブジェクトをカメラの方向へ向ける
        transform.LookAt(p);

        // 反対を向いてしまうため、Y軸を180度回転させて修正する
        transform.Rotate(0, 180f, 0);
    }
}
