using System.Collections;
using UnityEngine;
using Mirror;   // Mirrorのネットワーク機能

public class VS_GameManager : NetworkBehaviour
{
    // ===============================
    // ゲーム時間関連の設定
    // ===============================

    [Header("ゲーム時間設定")]
    [Tooltip("ゲーム開始時の合計時間（秒）")]
    [SerializeField] public float initialGameTime = 180f;

    [Tooltip("ゲーム開始前のカウントダウン時間（秒）")]
    [SerializeField] public float preGameCountdownTime = 3f;

    // サーバーが管理し、クライアントに同期される残り時間
    [SyncVar] private float remainingGameTime;

    // ===============================
    // ゲーム進行フラグ
    // ===============================

    [Header("ゲーム状態")]
    [Tooltip("ゲームが開始されたかどうか")]
    [SyncVar] public bool gameStarted = false;

    // ===============================
    // シングルトン
    // ===============================
    public static VS_GameManager Instance { get; private set; }

    // 外部から残り時間を取得するためのプロパティ
    public float CurrentTime => remainingGameTime;



    // ===============================
    // Awakeでシングルトンセット
    // ===============================
    private void Awake()
    {
        // シングルトンをセットする処理
        Instance = this;
    }



    // ===============================
    // Startでサーバー時のみゲーム開始処理を実行
    // ===============================
    private void Start()
    {
        // もしサーバー（=ホストも含む）でなければ何もしない
        if (!isServer) return;

        // カウントダウン開始処理を呼ぶ
        StartCoroutine(GameFlowRoutine());
    }



    // ===============================
    // ゲームの流れ（カウントダウン → ゲーム時間計測）を管理するコルーチン
    // ===============================
    private IEnumerator GameFlowRoutine()
    {
        // カウントダウン中はゲーム開始フラグを false にしておく
        gameStarted = false;

        // カウントダウン時間の表示用に SyncVar 時間を使う
        remainingGameTime = preGameCountdownTime;

        // カウントダウン処理
        while (remainingGameTime > 0f)
        {
            // 1秒ごとにカウントダウン
            yield return new WaitForSeconds(1f);
            remainingGameTime -= 1f;
        }

        // カウントダウンが終わったのでゲーム開始フラグを true にする
        gameStarted = true;

        // ゲーム時間を初期値にセット
        remainingGameTime = initialGameTime;

        // ゲーム時間を毎秒減らしていく処理
        while (remainingGameTime > 0f)
        {
            // 1秒待ってから時間を減らす
            yield return new WaitForSeconds(1f);
            remainingGameTime -= 1f;
        }

        // 時間が0になったらゲーム終了処理を呼ぶ（必要なら追加）
        OnGameEnd();
    }



    // ===============================
    // ゲーム終了時の処理（必要ならここに演出やシーン移動）
    // ===============================
    private void OnGameEnd()
    {
        // ここにゲーム終了処理を書く（シーン遷移など）
        Debug.Log("ゲーム終了！");
    }



    // ===============================
    // Update（今回は使わないが残しておく）
    // ===============================
    private void Update()
    {
        // 時間管理はすべてコルーチンで行うため、Updateでは何もしない
    }
}
