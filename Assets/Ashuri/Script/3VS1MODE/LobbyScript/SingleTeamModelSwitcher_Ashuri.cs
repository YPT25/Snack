using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SingleTeamModelSwitcher_Ashuri : NetworkBehaviour
{
    [Header("変更後のプレイヤー番号")]
    [Tooltip("現在のモードID 1 = 1人側:0 = ３人側")]
    [SyncVar] public int modeID;

    // ----------------------------------------------------
    // アイテム側から呼び出されるメソッド
    // ----------------------------------------------------
    [Command]
    public void TryChangePlayer(int id)
    {
        // 1つ上：サーバーでモードIDを保存
        modeID = id;

        // 1つ上：StatePlayer にも保存（復元用）
        StatePlayer_Ashuri.Instance.SetModeId(connectionToClient, id);
    }
}
