using System.Collections; // コルーチン使用のため
using UnityEngine;        // Unity基本機能
using Mirror;             // Mirrorネットワーク機能
using TMPro;              // TextMeshProUI表示用

/// <summary>
/// Mirror用のゲーム進行管理クラス
/// ・時間経過によるゲーム終了処理
/// ・スコア表示UIの制御
/// </summary>
public class GameManager : NetworkBehaviour
{
    // ===============================
    // ゲーム時間関連の設定
    // ===============================

    [Header("ゲーム全般の時間設定")]
    [Tooltip("現在の残り時間。サーバーからクライアントへ同期されます。")]
    [SyncVar(hook = nameof(OnTimeChange))] // 値変更時に呼ばれるフックを設定
    public float remainingGameTime; // 残り時間（サーバー→クライアント同期）

    // ゲーム開始時の初期時間（秒単位）
    [Header("ゲーム初期設定")]
    [SerializeField] public float initialGameTime = 180f;

    // ゲーム開始前のカウントダウン時間（秒単位）
    [Header("ゲーム開始前カウントダウン")]
    [SerializeField] public float preGameCountdownTime = 3f;

    // ===============================
    // ゲーム進行状態の管理
    // ===============================

    [Header("ゲーム状態")]
    [Tooltip("ゲームが開始されたかどうか")]
    [SyncVar(hook = nameof(OnGameStartChanged))] // 変更時にクライアント側処理を実行
    public bool gameStarted = false; // ゲーム開始フラグ

    // ===============================
    // スコア表示用UI設定
    // ===============================

    [Header("スコアUI関連")]
    [Tooltip("ゲーム終了後にスコアを表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI scoreText; // スコアを表示するUIテキスト
    [Tooltip("スコア表示パネル（非表示→表示に切り替える）")]
    [SerializeField] private GameObject scorePanel; // スコアUIパネル

    // 現在のスコアを保持する変数
    private int currentScore = 0;

    // シングルトンインスタンス（どこからでも参照できる）
    public static GameManager Instance { get; private set; }

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

        // ゲーム開始前のカウントダウンを開始
        StartCoroutine(ServerPreGameCountdown());
    }

    // ===============================
    // クライアント開始時の処理
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();

        // シングルトン登録（クライアント側）
        if (Instance == null) Instance = this;

        // スコアパネルを最初は非表示に設定
        if (scorePanel != null)
            scorePanel.SetActive(false);
    }

    // ===============================
    // SyncVarフック：時間変更時に呼ばれる
    // ===============================
    void OnTimeChange(float _oldTime, float _newTime)
    {
        // 残り時間が0以下になったらゲーム終了処理を実行（サーバー側のみ）
        if (isServer && _newTime <= 0f)
        {
            EndGame();
        }
    }

    // ===============================
    // SyncVarフック：ゲーム開始状態が変化したときに呼ばれる
    // ===============================
    void OnGameStartChanged(bool _oldValue, bool _newValue)
    {
        // クライアント側でゲームが開始されたときに呼ばれる
        if (_newValue)
        {
            Debug.Log("Game Started! (Client)");
        }
    }

    // ===============================
    // サーバー側：ゲーム開始前カウントダウン
    // ===============================
    IEnumerator ServerPreGameCountdown()
    {
        // カウントダウン秒数を設定
        float countdown = preGameCountdownTime;

        // カウントダウンが0になるまでループ
        while (countdown > 0)
        {
            // 残り秒数をログに表示
            Debug.Log($"Game starts in {Mathf.Ceil(countdown)}");

            // 1秒待機
            yield return new WaitForSeconds(1f);

            // カウントダウンを1秒減らす
            countdown -= 1f;
        }

        // サーバー側でゲーム開始ログを出力
        Debug.Log("Game Started! (Server)");

        // ゲーム開始フラグを有効化
        gameStarted = true;

        // ゲーム中の時間カウントダウンを開始
        StartCoroutine(ServerCountdownCoroutine());
    }

    // ===============================
    // サーバー側：ゲーム中の残り時間カウントダウン
    // ===============================
    IEnumerator ServerCountdownCoroutine()
    {
        // 1秒ごとに時間を減らして同期を行う
        while (remainingGameTime > 0)
        {
            // 1秒待機（SyncVarのネットワーク負荷を軽減）
            yield return new WaitForSeconds(1f);

            // 残り時間を1秒減らす
            remainingGameTime -= 1f;

            // 残り時間が負にならないよう補正
            if (remainingGameTime < 0)
                remainingGameTime = 0;
        }

        // 時間切れになったらゲーム終了処理を実行
        EndGame();
    }


    // ===============================
    // サーバー側：ゲーム終了処理
    // ===============================
    [Server]
    void EndGame()
    {
        // ゲーム終了をログ出力
        Debug.Log("Time's up! (Server) Game Over!");

        // ゲーム進行フラグをオフに
        gameStarted = false;

        //スコアを取得
        SweetScore sweetScore = FindObjectOfType<SweetScore>();
        currentScore = sweetScore.currentScore;

        // 全クライアントにゲーム終了とスコア表示を通知
        RpcShowScore(currentScore);
    }

    // ===============================
    // クライアント側：スコアUIを表示する処理
    // ===============================
    [ClientRpc]
    void RpcShowScore(int finalScore)
    {
        // クライアントでログ出力
        Debug.Log("Game Over! Showing Score (Client)");

        // スコアパネルを表示
        if (scorePanel != null)
            scorePanel.SetActive(true);

        // スコアテキストに最終スコアを表示
        if (scoreText != null)
            scoreText.text = $"Your team Score: {finalScore}";

        // ゲーム全体を一時停止
        Time.timeScale = 0f;
    }

    // ===============================
    // サーバー側：スコア加算処理（外部から呼び出し可）
    // ===============================
    [Server]
    public void AddScore(int value)
    {
        // 現在のスコアに加算
        currentScore += value;
    }

    // ===============================
    // クライアント停止時：シングルトンを解除
    // ===============================
    public override void OnStopClient()
    {
        base.OnStopClient();

        // 自身がインスタンスなら解放
        if (Instance == this)
            Instance = null;
    }

    // ===============================
    // サーバー停止時：全コルーチンを停止
    // ===============================
    public override void OnStopServer()
    {
        base.OnStopServer();

        // 動作中のコルーチンをすべて停止
        StopAllCoroutines();
    }
}
