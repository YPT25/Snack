using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerListDisplay_Ashuri : NetworkBehaviour
{
    [Header("表示するテキスト")]
    [Tooltip("全員の名前を表示するTextMeshPro（UIでも3DTextでもOK）")]
    [SerializeField] private TextMeshPro userNameText;

    // ----------------------------------------
    // 起動時に定期更新を開始
    // ----------------------------------------
    private void Start()
    {
        StartCoroutine(UpdatePlayerListRoutine());
    }

    // ----------------------------------------
    // 1秒ごとに全プレイヤー名を更新
    // ----------------------------------------
    private IEnumerator UpdatePlayerListRoutine()
    {
        while (true)
        {
            UpdatePlayerListDisplay();
            yield return new WaitForSeconds(1f);
        }
    }

    // ----------------------------------------
    // Mirrorのspawnedリストから全プレイヤーの名前を取得
    // ----------------------------------------
    private void UpdatePlayerListDisplay()
    {
        if (userNameText == null) return;

        // 表示を初期化
        userNameText.text = "";

        // クライアント上で現在Spawnされている全プレイヤーを確認
        foreach (NetworkIdentity identity in NetworkClient.spawned.Values)
        {
            Player_Tanabe player = identity.GetComponent<Player_Tanabe>();

            // 有効なプレイヤーのみ表示
            if (player != null && !string.IsNullOrEmpty(player.playerName))
            {
                userNameText.text += player.playerName + "\n";
            }
        }
    }
}
