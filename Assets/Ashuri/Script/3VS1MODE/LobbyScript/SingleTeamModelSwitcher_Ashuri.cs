using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SingleTeamModelSwitcher_Ashuri : NetworkBehaviour
{
    [Header("変更後のプレイヤーPrefab")]
    [Tooltip("変身後のプレイヤーPrefabを入れる")]
    public GameObject newPlayerPrefab;

    // ----------------------------------------------------
    // アイテム側から呼び出されるメソッド
    // ----------------------------------------------------
    public void TryChangePlayer(int i)
    {
        if (!isLocalPlayer) return;

        Debug.Log("CmdChangePlayer を実行します");

        // StatePlayer_Ashuriを取得する
        StatePlayer_Ashuri statePlayer = FindObjectOfType<StatePlayer_Ashuri>();

        // 1人側か３人側かを保存させる
        statePlayer.SetModeId(connectionToClient, i);

    }
}
