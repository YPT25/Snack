using Mirror;             // Mirrorネットワーク機能を使用
using Mirror.Examples.MultipleMatch; // Mirrorのサンプル機能（必要に応じて）
using System.Collections; // コルーチンを使用するため
using TMPro;              // TextMeshProを使用するため
using UnityEngine;        // Unity基本機能

/// <summary>
/// Mirror用のゲーム進行管理クラス
/// ・時間経過によるゲーム終了処理
/// ・スコア表示通知を行う（UI処理はResultUIScoreに任せる）
/// </summary>
public class GameManager : NetworkBehaviour
{
    // ===============================
    // ゲーム時間関連の設定
    // ===============================

    [Header("ゲーム全般の時間設定")]
    [Tooltip("現在の残り時間。サーバーからクライアントへ同期されます。")]
    [SyncVar(hook = nameof(OnTimeChange))]
    public float remainingGameTime; // 残り時間（サーバー→クライアント同期）

    [Header("ゲーム初期設定")]
    [Tooltip("ゲーム開始時の初期時間（秒単位）")]
    [SerializeField] public float initialGameTime = 180f; // ゲームの合計時間（秒）

    [Header("ゲーム開始前カウントダウン")]
    [Tooltip("ゲーム開始前のカウントダウン秒数")]
    [SerializeField] public float preGameCountdownTime = 3f; // 開始前カウントダウン

    // ===============================
    // ゲーム進行状態の管理
    // ===============================

    [Header("ゲーム状態")]
    [Tooltip("ゲームが開始されたかどうか")]
    [SyncVar(hook = nameof(OnGameStartChanged))]
    public bool gameStarted = false; // ゲーム開始フラグ

    // ===============================
    // シングルトンインスタンスの設定
    // ===============================
    public static GameManager Instance { get; private set; }

    // ===============================
    // サーバー開始時の処理
    // ===============================
    public override void OnStartServer()
    {
        // 親クラスの開始処理を実行
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
        // 親クラスの開始処理を実行
        base.OnStartClient();

        // クライアント側もシングルトン登録
        if (Instance == null) Instance = this;
    }

    // ===============================
    // SyncVarフック：残り時間変更時に呼ばれる
    // ===============================
    void OnTimeChange(float _oldTime, float _newTime)
    {
        // 残り時間が0以下になったら終了処理を呼ぶ（サーバーのみ）
        if (isServer && _newTime <= 0f)
        {
            EndGame();
        }
    }

    // ===============================
    // SyncVarフック：ゲーム開始状態が変化したとき
    // ===============================
    void OnGameStartChanged(bool _oldValue, bool _newValue)
    {
        // クライアント側でゲーム開始時にログ出力
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
        // カウントダウンの残り時間を設定
        float countdown = preGameCountdownTime;

        // カウントダウンが0になるまで繰り返す
        while (countdown > 0)
        {
            // 残り秒数を表示
            Debug.Log($"Game starts in {Mathf.Ceil(countdown)}");

            // 1秒待つ
            yield return new WaitForSeconds(1f);

            // 残り時間を1減らす
            countdown -= 1f;
        }

        // サーバーでゲーム開始を表示
        Debug.Log("Game Started! (Server)");

        // ゲーム開始フラグをON
        gameStarted = true;

        // ゲーム時間カウントダウン開始
        StartCoroutine(ServerCountdownCoroutine());
    }

    // ===============================
    // サーバー側：ゲーム中の時間カウントダウン
    // ===============================
    IEnumerator ServerCountdownCoroutine()
    {
        // 残り時間が0より大きい間繰り返す
        while (remainingGameTime > 0)
        {
            // 1秒待機（同期負荷軽減）
            yield return new WaitForSeconds(1f);

            // 残り時間を1秒減らす
            remainingGameTime -= 1f;

            // 負の値にならないように補正
            if (remainingGameTime < 0)
                remainingGameTime = 0;
        }

        // 残り時間が0になったら終了処理
        EndGame();
    }

    // ===============================
    // サーバー側：ゲーム終了処理
    // ===============================
    [Server]
    void EndGame()
    {
        // サーバーでゲーム終了をログ表示
        Debug.Log("Time's up! (Server) Game Over!");

        // ゲーム進行フラグをOFF
        gameStarted = false;

        // SweetScoreを探して現在スコアを取得
        SweetScore sweetScore = FindObjectOfType<SweetScore>();
        float currentScore = sweetScore.currentScore;

        // 全クライアントにスコア表示を通知
        ResultUIScore.Instance.RpcShowScore(currentScore);
    }

    // ===============================
    // クライアント停止時の処理
    // ===============================
    public override void OnStopClient()
    {
        // 親クラスの終了処理を呼ぶ
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
        // 親クラスの終了処理を呼ぶ
        base.OnStopServer();

        // 全コルーチンを停止
        StopAllCoroutines();
    }
}
