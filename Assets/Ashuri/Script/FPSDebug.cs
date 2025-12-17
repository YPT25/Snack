using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSDebug : MonoBehaviour
{
    // FPS・Ping・時間を表示するTextMeshPro
    [SerializeField]
    private TextMeshProUGUI debugText;

    // 表示中かどうかを管理するフラグ
    private bool isVisible = false;

    // FPS計算用の変数
    private float deltaTime = 0.0f;

    // --------------------------------------
    // 初期化処理
    void Start()
    {
        // 最初は非表示にする
        debugText.gameObject.SetActive(false);
    }

    // ----------------------------------------------------
    // このオブジェクトをシーンを跨いでも残す処理
    // ----------------------------------------------------
    private void Awake()
    {
        // シーン遷移しても削除されないようにする
        DontDestroyOnLoad(gameObject);
    }

    // --------------------------------------
    // 毎フレーム呼ばれる処理
    void Update()
    {
        // Bキーが押されたら表示／非表示を切り替える
        if (Input.GetKeyDown(KeyCode.B))
        {
            isVisible = !isVisible;
            debugText.gameObject.SetActive(isVisible);
        }

        // 表示中でなければ処理しない
        if (!isVisible) return;

        // --------------------------------------
        // FPS計算（フレーム時間のブレを吸収）
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // --------------------------------------
        // Ping表示用の初期値
        string pingText = "---";

        // --------------------------------------
        // クライアントとして接続している場合のみPingを取得
        if (NetworkClient.isConnected)
        {
            // Mirrorが計測しているRTT（往復遅延）を取得（秒）
            double rttSeconds = NetworkTime.rtt;

            // Hostの場合は0msになりやすい
            if (NetworkServer.active && NetworkClient.active)
            {
                pingText = "0 (HOST)";
            }
            else
            {
                // ミリ秒に変換
                float pingMs = (float)(rttSeconds * 1000.0);
                pingText = pingMs.ToString("F0");
            }
        }

        // --------------------------------------
        // 現在の時刻を取得
        string time = System.DateTime.Now.ToString("HH:mm:ss");

        // --------------------------------------
        // TextMeshProに表示
        debugText.text =
            $"FPS  : {fps:F1}\n" +
            $"PING : {pingText} ms\n" +
            $"TIME : {time}";
    }
}
