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

    //表示する名前
    private string userName;

    // ----------------------------------------
    // 起動時に定期更新を開始
    // ----------------------------------------
    private void Start()
    {
    }
}
