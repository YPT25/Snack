using Mirror;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [Header("ゲームに参加するのを確認するオブジェクト")]
    [Tooltip("1人側FirstPersonField")]
    [SerializeField] private testPlayerMenberCheck _firstParticipants;

    [Tooltip("3人側FirstPersonField")]
    [SerializeField] private testPlayerMenberCheck _thirdParticipants;

    // ーーー シーン遷移を1回だけにするためのフラグ ーーー
    private bool _isSceneChanging = false;

    void Update()
    {
        // ーーー サーバー以外は何もしない ーーー
        if (!isServer) return;

        // ーーー すでにシーン遷移中なら何もしない ーーー
        if (_isSceneChanging) return;

        // ーーー それぞれのエリアに乗っている人数を取得 ーーー
        int firstMenber = _firstParticipants.GetTouchPlayerCount();
        int thirdMenber = _thirdParticipants.GetTouchPlayerCount();

        // ーーー 合計人数を計算 ーーー
        int menber = firstMenber + thirdMenber;

        // ーーー Mirror が認識しているプレイヤー数 ーーー
        int totalPlayers = NetworkManager.singleton.numPlayers;

        // ーーー 全員そろったらシーンを遷移 ーーー
        if (menber > totalPlayers)
        {
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
