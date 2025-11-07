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
    // プレイヤー名を追加表示
    // ----------------------------------------
    public void RespownName(string userName)
    {
        if (!string.IsNullOrEmpty(userName))
        {
            // 改行してどんどん追加
            userNameText.text += userName + "\n";
            Debug.Log($"表示更新: {userName}");
        }
        else
        {
            Debug.LogWarning("プレイヤー名が空です");
        }
    }
}
