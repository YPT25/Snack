using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BWStartManager : NetworkBehaviour
{
    [Header("バトロワ開始のボタン")]
    [Tooltip("BWStartButton をすべてアタッチする")]
    [SerializeField] private List<BWStartButton> _BWButton = new List<BWStartButton>();

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
    }
}
