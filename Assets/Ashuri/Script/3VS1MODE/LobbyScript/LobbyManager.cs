using Mirror;
using UnityEngine;
using System.Collections;

public class LobbyManager : NetworkBehaviour
{
    [Header("ゲームに参加するのを確認するオブジェクト")]
    [Tooltip("1人側FirstPersonField")]
    [SerializeField] private testPlayerMenberCheck _firstMenber;

    [Tooltip("3人側FirstPersonField")]
    [SerializeField] private testPlayerMenberCheck _thirdMenber;

    [Header("ゲームを開始できるかどうかを確認するオブジェクト")]
    [Tooltip("1人側ParticipantsNumberScript")]
    [SerializeField] private ParticipantsNumberScript _firstParticipants;

    [Tooltip("3人側ParticipantsNumberScript")]
    [SerializeField] private ParticipantsNumberScript _thirdParticipants;

    // ーーー シーン遷移を1回だけにするためのフラグ ーーー
    private bool _isSceneChanging = false;

    void Update()
    {
        // ーーー サーバー以外は何もしない ーーー
        if (!isServer) return;

        // ーーー すでにシーン遷移中なら何もしない ーーー
        if (_isSceneChanging) return;

        // ーーー それぞれのエリアがゲーム開始可能か ーーー
        bool firstMenber = _firstParticipants.FirstGameStart();
        bool thirdMenber = _thirdParticipants.ThirdGameStart();

        // ーーー お互いゲームを可能かどうか     ーーー
        bool menber = (firstMenber && thirdMenber);

        // ーーー 全員そろったらシーンを遷移 ーーー
        if (menber)
        {
            //移動します
            //Debug.LogError($"一致してます {menber} == {totalPlayers} F: {firstMenber} T : {thirdMenber}");

            // 次のシーンに移動
            ChangeScene();
        }
    }

    // ーーー サーバー専用のシーン遷移処理 ーーー
    [Server]
    private void ChangeScene()
    {
        // フラグを立てて2回呼ばれないようにする
        _isSceneChanging = true;

        // シーン遷移開始
        NetworkManager.singleton.ServerChangeScene("3VS1ModeGame");
    }
}
