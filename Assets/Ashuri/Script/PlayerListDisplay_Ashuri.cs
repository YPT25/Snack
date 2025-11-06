using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerListDisplay_Ashuri : MonoBehaviour
{
    [Header("表示するテキスト")]
    [Tooltip("モニターにユーザー名を表示するTextMeshPro（3DTextでもOK）")]
    [SerializeField] private TextMeshPro userNameText;

    // ------------------------------------------
    // 定期的にプレイヤー一覧を更新
    // ------------------------------------------
    void Start()
    {
        StartCoroutine(UpdatePlayerListRoutine());
    }

    // ------------------------------------------
    // プレイヤーリストを1秒ごとに更新する
    // ------------------------------------------
    private IEnumerator UpdatePlayerListRoutine()
    {
        while (true)
        {
            UpdatePlayerListDisplay();
            yield return new WaitForSeconds(1f);
        }
    }

    // ------------------------------------------
    // Mirrorで接続している全プレイヤーの名前を取得して表示
    // ------------------------------------------
    private void UpdatePlayerListDisplay()
    {
        // 接続済みプレイヤーを格納するリスト
        List<string> playerNames = new List<string>();

        // NetworkClientに存在する全プレイヤーを探索
        foreach (NetworkIdentity identity in NetworkClient.spawned.Values)
        {
            // Player_Tanabeを取得
            Player_Tanabe player = identity.GetComponent<Player_Tanabe>();
            if (player != null && !string.IsNullOrEmpty(player.playerName))
            {
                // 名前をリストに追加
                if (!playerNames.Contains(player.playerName))
                    playerNames.Add(player.playerName);
            }
        }

        // 名前を縦に並べる
        string displayText = "";
        foreach (string name in playerNames)
        {
            displayText += name + "\n";
        }

        // TextMeshProに表示
        if (userNameText != null)
        {
            userNameText.text = displayText;
        }
    }
}
