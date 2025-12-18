using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BWStartButton : MonoBehaviour
{
    // プレイヤーが乗っているかどうか
    private bool isGetOn = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            // 乗っていたらture
            isGetOn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            // 降りたらfalse
            isGetOn = false;
        }
    }

    /// <summary>
    /// プレイヤーが乗っているかどうかを取得する
    /// </summary>
    public bool GetOn()
    {
        return isGetOn;
    }
}
