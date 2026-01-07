using Mirror;             // Mirrorのネットワーク機能を使用
using System.Collections; // コルーチンを使用
using UnityEngine;        // Unityの基本機能

/// <summary>
/// Mirror用ゲーム進行管理クラス
/// ・_isGameStart が true になるまで何も始まらない
/// ・true になったら開始前カウントダウン開始
/// ・カウントダウン終了後にゲーム時間が進む
/// ・時間切れでゲーム終了
/// </summary>
public class VS_GameManager : NetworkBehaviour
{
    // ===============================
    // シングルトン
    // ===============================

    /// <summary>
    /// どこからでも参照できるようにする
    /// </summary>
    public static VS_GameManager Instance { get; private set; }

    // ===============================
    // ゲーム時間設定
    // ===============================

    [Header("ゲーム時間設定")]
    [Tooltip("ゲームの合計時間（秒）")]
    [SerializeField] public float initialGameTime = 180f;

    [Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
    [SerializeField] public float preGameCountdownTime = 3f;

    // ===============================
    // サーバー管理の残り時間
    // ===============================

    /// <summary>
    /// サーバーが管理しクライアントに同期される残り時間
    /// </summary>
    [SyncVar]
    public float remainingGameTime;

    // ===============================
    // ゲーム状態
    // ===============================

    [Header("ゲーム状態")]
    [Tooltip("ゲームが開始しているか")]
    [SyncVar]
    public bool gameStarted = false;

    // ===============================
    // ゲーム開始トリガー
    // ===============================

    [Header("開始トリガー")]
    [Tooltip("これが true になったら開始前カウントダウンが始まる")]
    [SyncVar]
    public bool _isGameStart = false;

    // ===============================
    // 外部参照用
    // ===============================

    /// <summary>
    /// 現在の残り時間を取得
    /// </summary>
    public float CurrentTime => remainingGameTime;

    // ===============================
    // サーバー開始時
    // ===============================
    public override void OnStartServer()
    {
        base.OnStartServer();

        // シングルトン登録
        Instance = this;

        // ゲーム時間を初期化
        remainingGameTime = initialGameTime;

        // ゲームはまだ開始していない
        gameStarted = false;

        // 開始トリガーもOFF
        _isGameStart = false;

        // ゲーム全体の進行管理を開始
        StartCoroutine(ServerGameFlow());
    }

    // ===============================
    // クライアント開始時
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();

        // クライアント側でも参照できるようにする
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // ===============================
    // サーバー：ゲーム全体の流れ管理
    // ===============================
    private IEnumerator ServerGameFlow()
    {
        // _isGameStart が true になるまで待機
        while (!_isGameStart)
        {
            yield return null;
        }

        // 開始前カウントダウンを実行
        yield return StartCoroutine(ServerPreGameCountdown());

        // ゲーム時間のカウントダウンを実行
        yield return StartCoroutine(ServerGameTimeCountdown());
    }

    // ===============================
    // サーバー：開始前カウントダウン
    // ===============================
    private IEnumerator ServerPreGameCountdown()
    {
        // カウントダウン用変数
        float countdown = preGameCountdownTime;

        // カウントダウンが終わるまで繰り返す
        while (countdown > 0f)
        {
            // サーバーログ（確認用）
            Debug.Log($"Game Start In : {Mathf.Ceil(countdown)}");

            // 1秒待機
            yield return new WaitForSeconds(1f);

            // カウントダウンを減らす
            countdown -= 1f;
        }

        // カウントダウン終了 → ゲーム開始
        gameStarted = true;
    }

    // ===============================
    // サーバー：ゲーム中の時間管理
    // ===============================
    private IEnumerator ServerGameTimeCountdown()
    {
        // 残り時間がある間ループ
        while (remainingGameTime > 0f)
        {
            // 1秒待機
            yield return new WaitForSeconds(1f);

            // ゲーム中でなければ時間を進めない
            if (!gameStarted)
            {
                continue;
            }

            // 残り時間を1秒減らす
            remainingGameTime -= 1f;

            // マイナス防止
            if (remainingGameTime < 0f)
            {
                remainingGameTime = 0f;
            }
        }

        // 時間切れでゲーム終了
        EndGame();
    }

    // ===============================
    // サーバー：ゲーム終了処理
    // ===============================
    [Server]
    private void EndGame()
    {
        // サーバーログ
        Debug.Log("Game Over : Time Up");

        // ゲーム停止
        gameStarted = false;

        // スコア取得
        SweetScore score = FindObjectOfType<SweetScore>();
        float currentScore = score != null ? score.currentScore : 0f;

        // 全クライアントに結果を通知
        if (ResultUIScore.Instance != null)
        {
            ResultUIScore.Instance.RpcShowScore(currentScore);
        }
    }

    // ===============================
    // クライアント停止時
    // ===============================
    public override void OnStopClient()
    {
        base.OnStopClient();

        // シングルトン解除
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ===============================
    // サーバー停止時
    // ===============================
    public override void OnStopServer()
    {
        base.OnStopServer();

        // 全コルーチン停止
        StopAllCoroutines();
    }
}
