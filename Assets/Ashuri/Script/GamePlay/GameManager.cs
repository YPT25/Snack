using Mirror;             // Mirrorのネットワーク機能を使用
using System.Collections; // コルーチンを使用
using System.Collections.Generic;
using UnityEngine;        // Unityの基本機能

/// <summary>
/// Mirror用のゲーム進行管理クラス
/// ・ゲーム開始前のカウントダウン
/// ・ゲーム中の時間カウントダウン
/// ・ゲーム終了時のスコア通知
/// ・カウントダウン中はゲームを停止
/// </summary>
public class GameManager : NetworkBehaviour
{
    // ===============================
    // ゲーム時間関連の設定
    // ===============================

    [Header("ゲーム時間設定")]
    [Tooltip("ゲーム開始時の合計時間（秒）")]
    [SerializeField] public float initialGameTime = 180f;

    [Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
    [SerializeField] public float preGameCountdownTime = 3f;

    [Tooltip("サーバーが管理する残り時間（SyncVarでクライアントに同期）")]
    [SyncVar] private float remainingGameTime;

    // ===============================
    // ゲーム進行フラグ
    // ===============================
    [Header("ゲーム状態")]
    [Tooltip("ゲームが開始されたかどうか")]
    [SyncVar] public bool gameStarted = false; // カウントダウン中は false

    // ===============================
    // シングルトン
    // ===============================
    public static GameManager Instance { get; private set; }

    // ===============================
    // 外部から残り時間を取得するプロパティ
    // ===============================
    public float CurrentTime => remainingGameTime;

    // ===============================
    // サーバー開始時の処理
    // ===============================
    public override void OnStartServer()
    {
        base.OnStartServer();

        // シングルトン登録（初回のみ）
        if (Instance == null) Instance = this;

        // 残り時間を初期化
        remainingGameTime = initialGameTime;

        // ゲーム開始前カウントダウン開始
        StartCoroutine(ServerPreGameCountdown());
    }

    // ===============================
    // クライアント開始時の処理
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();

        // クライアント側シングルトン登録（初回のみ）
        if (Instance == null) Instance = this;
    }

    // ===============================
    // サーバー：ゲーム開始前カウントダウン
    // カウントダウン中は gameStarted = false
    // ===============================
    private IEnumerator ServerPreGameCountdown()
    {
        // カウントダウン時間をセット
        float countdown = preGameCountdownTime;

        // カウントダウン中は残り秒数を1秒ずつ減らす
        while (countdown > 0)
        {
            // デバッグ用ログ（サーバー側）
            Debug.Log($"Game starts in {Mathf.Ceil(countdown)} seconds");

            // 1秒待機
            yield return new WaitForSeconds(1f);

            // カウントダウンを減らす
            countdown -= 1f;
        }

        // カウントダウン終了 → ゲーム開始
        gameStarted = true;

        // ゲーム時間カウントダウン開始
        StartCoroutine(ServerCountdownCoroutine());
    }

    // ===============================
    // サーバー：ゲーム中の時間カウントダウン
    // ===============================
    private IEnumerator ServerCountdownCoroutine()
    {
        // ゲーム時間が残っている限りループ
        while (remainingGameTime > 0)
        {
            // 1秒待機
            yield return new WaitForSeconds(1f);

            // 残り時間を1秒減らす
            remainingGameTime -= 1f;

            // マイナス補正
            if (remainingGameTime < 0) remainingGameTime = 0;
        }

        // 残り時間が0になったらゲーム終了
        EndGame();
    }

    // ===============================
    // サーバー：ゲーム終了処理
    // ===============================
    [Server]
    private void EndGame()
    {
        // サーバー側で終了ログ
        Debug.Log("Time's up! Game Over (Server)");

        // ゲーム進行フラグをOFF
        gameStarted = false;

        // SweetScoreを探して現在のスコアを取得
        SweetScore score = FindObjectOfType<SweetScore>();
        float currentScore = score != null ? score.currentScore : 0f;

        // 全クライアントにスコア表示を通知
        ResultUIScore.Instance.RpcShowScore(currentScore);
    }

    // ===============================
    // クライアント停止時の処理
    // ===============================
    public override void OnStopClient()
    {
        base.OnStopClient();

        // シングルトン解除
        if (Instance == this)
            Instance = null;
    }

    // ===============================
    // サーバー停止時の処理
    // ===============================
    public override void OnStopServer()
    {
        base.OnStopServer();

        // 全コルーチン停止
        StopAllCoroutines();
    }
}
