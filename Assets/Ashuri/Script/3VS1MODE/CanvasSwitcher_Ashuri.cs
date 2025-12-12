using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasSwitcher_Ashuri : MonoBehaviour
{
    // ローカルプレイヤーの参照
    private Player_Tanabe _localPlayer;

    // Start is called before the first frame update
    void Start()
    {
        // チームを検索する
        FindOfTeam();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // -----------------------------------------------
    // 所持アイテムに合わせて UI を更新する処理
    // -----------------------------------------------
    private void FindOfTeam()
    {
        // StatePlayer_Ashuriを検索する
        StatePlayer_Ashuri statePlayer_Ashuri = FindObjectOfType<StatePlayer_Ashuri>();

        // チーム情報を取得する
    }
}
