using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BWStartManager : NetworkBehaviour
{
    [Header("バトロワ開始のボタン")]
    [Tooltip("BWStartButton をすべてアタッチする")]
    [SerializeField] private List<BWStartButton> _BWButton = new List<BWStartButton>();

    public string nextSceneName = "YourNextSceneName"; // 遷移先のシーン名

    //シーン遷移するか
    private bool isTrigger = false;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        // サーバーのみで開始待機を行う
        StartCoroutine(BWStartWaitCoroutine());
    }

    // バトロワ開始を待つCoroutine
    private IEnumerator BWStartWaitCoroutine()
    {
        // 条件が満たされるまでループ
        while (true)
        {
            // 現在接続しているプレイヤー人数を取得
            int playerNum = NetworkManager.singleton.numPlayers;

            // もし人数が5人目のボタンの必要性をなくす
            if(playerNum > 4)
            {
                playerNum = 4;
            }

            // 今ボタンに乗っている人数を取得
            int onButtonCount = GetOnButtonCount();

            // 全員がボタンに乗っていたら
            if (playerNum > 0 && onButtonCount == playerNum)
            {
                // バトロワ開始
                StartBattleRoyale();

                // 1回だけ実行して終了
                yield break;
            }

            // 次のフレームまで待機
            yield return null;
        }
    }

    // ボタンに乗っている人数を数える
    private int GetOnButtonCount()
    {
        // カウントを初期化
        int count = 0;

        // すべてのボタンを確認
        foreach (var button in _BWButton)
        {
            // 乗っていたらカウント
            if (button.GetOn())
            {
                count++;
            }
        }

        // 現在乗っている人数を返す
        return count;
    }

    // バトロワ開始処理
    [Server]
    private void StartBattleRoyale()
    {
        // 実際の開始処理はここに書く
        Debug.Log("バトルロワイヤル開始！");

        // 全てのクライアントにフェードアウトとシーン遷移を指示する
        RpcRequestSceneChange();
        //トリガーを発動させる
        isTrigger = true;
    }

    // クライアントでフェードアウトとシーン遷移を開始するRPC
    [ClientRpc]
    void RpcRequestSceneChange()
    {
        if (FadeManager.Instance != null)
        {
            // フェードアウトとシーン遷移を開始
            // コルーチンはモノビヘイビアからしか実行できないため、FadeManagerのInstanceから呼び出す
            FadeManager.Instance.FadeOut(nextSceneName);
        }
        else
        {
            Debug.LogError("FadeManager.Instance not found! Make sure FadeManager is on an active Canvas and has been initialized.");
            // フェードマネージャーが見つからない場合でも、シーン遷移だけは試みる（フェードなし）
            if (isServer)
            {
                NetworkManager.singleton.ServerChangeScene(nextSceneName);
            }
        }
        // プレイヤー番号をリセット
        ((AshuriNetworkManager)NetworkManager.singleton).PlayerNumberReset();

    }
}
