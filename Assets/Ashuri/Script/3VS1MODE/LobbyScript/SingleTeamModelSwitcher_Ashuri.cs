using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SingleTeamModelSwitcher_Ashuri : NetworkBehaviour
{
    [Header("変更後のプレイヤー番号")]
    [Tooltip("現在のモードID 1 = 1人側:0 = ３人側")]
    [SyncVar] public int modeID;

    public static object Instance { get; internal set; }

    // ----------------------------------------------------
    // アイテム側から呼び出されるメソッド
    // ----------------------------------------------------
    [Command]
    public void TryChangePlayer(int id)
    {
        // サーバーでモードIDを保存
        modeID = id;

        Debug.LogError($"当たったよ:{id}");
    }
}
