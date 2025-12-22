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
            // 乗っているプレイヤーのスクリプトを取得する
            var player = other.GetComponent<Player_Tanabe>();

            //プレイヤーの武器を取得する
            if (player.GetWeaponID() == Player_Tanabe.WeaponID.NONE) return;

            // タッチ中のカラーを設定
            GetComponent<Renderer>().material.color = Color.red;

            // 乗っていたらture
            isGetOn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            //離れたら色を戻す
            GetComponent<Renderer>().material.color = Color.white;

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
