using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasSwitcher_Ashuri : NetworkBehaviour
{
    // チーム0用Canvas
    [Header("Canvas設定")]
    [Tooltip("チームIDが一人チームのときに表示する Canvas")]
    [SerializeField] private GameObject teamFirstCanvas;

    // チーム1用Canvas
    [Tooltip("チームIDが三人チームのときに表示する Canvas")]
    [SerializeField] private GameObject teamThirdCanvas;

    // ローカルプレイヤーの参照
    private Player_Tanabe _localPlayer;

    private void Start()
    {
        // ローカルプレイヤーが生成されるまで待つ
        if (NetworkClient.localPlayer == null)
            return;

        // ローカルプレイヤーの接続情報を取得
        NetworkConnectionToClient conn =
            NetworkClient.localPlayer.connectionToClient;

        // StatePlayer を取得
        StatePlayer_Ashuri state = StatePlayer_Ashuri.Instance;

        // モードID（チームID）を取得
        int teamId = state.GetModeId(conn);

        // Canvas を切り替える
        SwitchCanvas(teamId);
    }

    // ----------------------------------------------------
    // Canvas 切り替え処理
    // ----------------------------------------------------
    private void SwitchCanvas(int teamId)
    {
        // 一度すべて非表示
        teamFirstCanvas.SetActive(false);
        teamThirdCanvas.SetActive(false);

        // チームIDに応じて表示
        if (teamId == 1)
        {
            teamFirstCanvas.SetActive(true);
        }
        else if (teamId == 2)
        {
            teamThirdCanvas.SetActive(true);
        }
    }
}
