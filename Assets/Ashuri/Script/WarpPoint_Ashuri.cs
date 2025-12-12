using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint_Ashuri : MonoBehaviour
{
    [Header("ワープポイント先")]
    [Tooltip("ワープ先のtransform")]
    [SerializeField] private Transform _warpPoint;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たったら
        if(other.CompareTag("Player"))
        {
            // 当たったオブジェクトをワープポイント先に移動させる
            other.gameObject.transform.position = _warpPoint.position;

            //角度を設定する
            other.gameObject.transform.rotation = _warpPoint.rotation;
        }
    }
}
