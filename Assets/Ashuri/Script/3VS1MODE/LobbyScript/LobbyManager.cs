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
    [SerializeField] private ParticipantsFirstScript _firstParticipants;

    [Tooltip("3人側ParticipantsNumberScript")]
    [SerializeField] private ParticipantsThirdScript _thirdParticipants;

    // ーーー シーン遷移を1回だけにするためのフラグ ーーー
    private bool _isSceneChanging = false;

    void Update()
    {
        // ーーー サーバー以外は何もしない ーーー
        if (!isServer) return;

        // ーーー すでにシーン遷移中なら何もしない ーーー
        if (_isSceneChanging) return;

        // ーーー それぞれのエリアがゲーム開始可能か ーーー
        // 1人側の判定
        bool firstMenber = _firstParticipants.GameStart();

        // 3人側の判定
        bool thirdMenber = _thirdParticipants.GameStart();

        // ーーー お互いゲームを可能かどうか     ーーー    
        bool menber = (firstMenber && thirdMenber);

        // ーーー 全員そろったらシーンを遷移 ーーー
        if (menber)
        {
            //ここでシーン遷移の演出を搭載する

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

        // モデルを変更する

        // シーン遷移開始
        NetworkManager.singleton.ServerChangeScene("3VS1ModeGame");
    }
}
