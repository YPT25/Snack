using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoviePlayerChanger : NetworkBehaviour
{
    [Header("撮影用カメラオブジェクト")]
    [SerializeField] private GameObject cameraObject;

    // Start is called before the first frame update
    void Start()
    {
        // PlayerUI レイヤーを描画しないように設定
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if(Input.GetKeyDown(KeyCode.C))
        {
            ModelSwitch();

            // ThreePlayerUI という名前の GameObject を検索
            GameObject uiRoot = GameObject.Find("ThreePlayerUI");

            // UIオブジェクトが存在するか確認
            if (uiRoot != null)
            {
                // UIをまとめたGameObjectを非表示にする
                uiRoot.SetActive(false);
            }
            else
            {
                // 見つからなかった場合のデバッグ用ログ
                Debug.LogWarning("ThreePlayerUI が見つかりません");
            }
        }
    }

    // ----------------------------------------------------
    // モデル切り替え処理（サーバー専用）
    // ----------------------------------------------------
    [Command]
    public void ModelSwitch()
    {
        // 現在のプレイヤーオブジェクトを保持
        GameObject oldPlayer = gameObject;

        // 接続情報を取得
        NetworkConnectionToClient conn = connectionToClient;

        // 新しいプレイヤーを生成
        GameObject newPlayer = Instantiate(
            cameraObject,
            oldPlayer.transform.position,
            oldPlayer.transform.rotation
        );

        // 接続に対してプレイヤーを差し替える
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, false);

        // 1フレーム後に古いプレイヤーを削除
        StartCoroutine(DeleteOldPlayer(oldPlayer));
    }

    // ----------------------------------------------------
    // 1フレーム後に古いプレイヤーを削除
    // ----------------------------------------------------
    private IEnumerator DeleteOldPlayer(GameObject oldPlayer)
    {
        yield return null;

        if (oldPlayer != null)
        {
            NetworkServer.Destroy(oldPlayer);
        }
    }
}
