using System.Collections; // コルーチン用
using UnityEngine; // Unity基本機能
using Mirror; // Mirrorネットワーク
using TMPro; // UI表示用（必要に応じて）

public class GameManager : NetworkBehaviour
{
    [Header("ゲーム全般の時間設定")]
    [Tooltip("現在の残り時間。サーバーからクライアントへ同期されます。")]
    [SyncVar(hook = nameof(OnTimeChange))]
    public float remainingGameTime; // 現在のゲーム残り時間（サーバー同期用）

    [Header("ゲーム初期設定")]
    [Tooltip("ゲーム開始時の初期時間 (秒単位)")]
    [SerializeField]
    private float initialGameTime = 180f; // ゲーム時間の初期値（変更可能）

    [Header("ゲーム開始前カウントダウン")]
    [Tooltip("ゲーム開始前のカウントダウン時間 (秒単位)")]
    [SerializeField]
    private float preGameCountdownTime = 3f; // 最初の3秒カウントダウン（変更可能）

    [Header("ゲーム状態")]
    [Tooltip("ゲームが開始されたかどうか")]
    [SyncVar(hook = nameof(OnGameStartChanged))]
    private bool gameStarted = false; // ゲーム開始フラグ（サーバーからクライアントに同期）

    // プロパティ：他のスクリプトから参照可能
    public float GetRemainingTime => remainingGameTime; // 残り時間を取得
    public bool IsGameStarted => gameStarted; // ゲーム開始フラグを取得

    // シングルトンインスタンス
    public static GameManager Instance { get; private set; } // GameManagerの唯一インスタンス

    // ===============================
    // サーバー開始時処理
    // ===============================
    public override void OnStartServer()
    {
        base.OnStartServer();
        // 残り時間を初期化
        remainingGameTime = initialGameTime;
        // ゲーム前カウントダウン開始
        StartCoroutine(ServerPreGameCountdown());
    }

    // ===============================
    // クライアント開始時処理
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (Instance == null)
        {
            // シングルトン登録
            Instance = this;
        }
        else if (Instance != this)
        {
            // 既に存在する場合は警告
            Debug.LogWarning("Duplicate GameManager found! Destroying this instance. (This should not happen)");
        }
    }

    // ===============================
    // SyncVarフック：残り時間変更時
    // ===============================
    void OnTimeChange(float _oldTime, float _newTime)
    {
        // クライアント側でremainingGameTimeが変わったときに呼ばれる
        // UI側はTimeTextなどがGetRemainingTimeを参照して自動更新する想定
    }

    // ===============================
    // SyncVarフック：ゲーム開始フラグ変更時
    // ===============================
    void OnGameStartChanged(bool _oldValue, bool _newValue)
    {
        if (_newValue)
        {
            // クライアントでゲーム開始ログ
            Debug.Log("Game Started! (Client)");
            // ゲーム開始に応じた処理はここに追加可能
        }
    }

    // ===============================
    // サーバー側：ゲーム開始前カウントダウン
    // ===============================
    IEnumerator ServerPreGameCountdown()
    {
        float countdown = preGameCountdownTime; // カウントダウン用変数に初期値設定
        while (countdown > 0)
        {
            // 残り秒数ログ
            Debug.Log($"Game starts in {Mathf.Ceil(countdown)}");
            // 1秒待機
            yield return new WaitForSeconds(1f);
            // カウントダウンを1秒減らす
            countdown -= 1f;
        }

        // サーバー側でゲーム開始ログ
        Debug.Log("Game Started! (Server)");
        // SyncVarでクライアントにゲーム開始通知
        gameStarted = true;
        // ゲーム本編カウントダウン開始
        StartCoroutine(ServerCountdownCoroutine());
    }

    // ===============================
    // サーバー側：ゲーム中の時間カウントダウン
    // ===============================
    IEnumerator ServerCountdownCoroutine()
    {
        while (remainingGameTime > 0)
        {
            // フレームごとに待機
            yield return null;
            // 残り時間をフレーム分減算
            remainingGameTime -= Time.deltaTime;

            if (remainingGameTime < 0)
            {
                // 負の値にならないよう補正
                remainingGameTime = 0;
            }
        }

        // ゲーム終了ログ
        Debug.Log("Time's up! (GameManager Server)");
        // ゲーム終了処理をここに追加可能
    }

    // ===============================
    // クライアント停止時：シングルトン解放
    // ===============================
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (Instance == this)
        {
            // インスタンス解放
            Instance = null;
        }
    }
}
