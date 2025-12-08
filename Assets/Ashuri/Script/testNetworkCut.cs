using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testNetworkCut : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            Disconnect();
        }
    }

    // ボタンの OnClick にこの関数を割り当ててください
    public void Disconnect()
    {
        // NetworkManagerが存在しない場合は何もしない
        if (NetworkManager.singleton == null) return;

        // 自分がホスト（サーバー兼クライアント）として動いている場合
        if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
        {
            NetworkManager.singleton.StopHost();
        }
        // 純粋なクライアントとして接続している場合
        else if (NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
        {
            NetworkManager.singleton.StopClient();
        }

        // 注: StopHost/StopClientが呼ばれると、Mirrorは
        // NetworkManagerの「Offline Scene」で設定したシーンへ自動的に遷移します。
    }
}
