using Mirror;     // Mirrorネットワーク機能
using TMPro;      // TextMeshProを使用
using UnityEngine; // Unityの基本クラス使用

/// <summary>
/// ゲーム終了後のスコアUIを管理するクラス
/// GameManagerから呼び出されてUIを表示し、ゲームを停止させる
/// </summary>
public class ResultUIScore : NetworkBehaviour
{
    [Header("スコアUI関連")]
    [Tooltip("スコアを表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI scoreText; // スコアテキスト

    [Tooltip("スコアパネル（非表示→表示切り替え）")]
    [SerializeField] private GameObject scorePanel; // スコアパネルオブジェクト

    // シングルトンインスタンスを保持
    public static ResultUIScore Instance { get; private set; }

    // ===============================
    // クライアント開始時の処理
    // ===============================
    public override void OnStartClient()
    {
        // 親クラスの処理を呼ぶ
        base.OnStartClient();

        // シングルトン登録（重複防止）
        if (Instance == null) Instance = this;

        // ゲーム開始時はスコアパネルを非表示にしておく
        if (scorePanel != null)
            scorePanel.SetActive(false);
    }

    // ===============================
    // クライアントRPC：スコアを全クライアントに表示
    // ===============================
    [ClientRpc]
    public void RpcShowScore(float finalScore)
    {
        // クライアントでスコア表示を開始
        ShowScore(finalScore);
    }

    // ===============================
    // クライアント側：スコアUI表示処理
    // ===============================
    public void ShowScore(float finalScore)
    {
        // デバッグログを表示
        Debug.Log("Game Over! Showing Score (Client)");

        // すべてのプレイヤーを取得
        Player_Tanabe[] players = FindObjectsOfType<Player_Tanabe>();

        // スコアパネルを表示
        if (scorePanel != null)
            scorePanel.SetActive(true);

        // スコアテキストが存在する場合
        if (scoreText != null)
        {
            // スコア表示用文字列を作る
            string allScores = "";

            // 各プレイヤーのスコアを追加
            for (int i = 0; i < players.Length; i++)
            {
                Player_Tanabe p = players[i];
                allScores += $"Player{p.playerNumber}: {p.m_sweetScore}\n";
            }

            // チーム全体スコアを最後に追加
            allScores += $"\nYour team Score: {finalScore}";

            // テキストに反映
            scoreText.text = allScores;
        }

        // 全クライアントでゲームを停止させる
        Time.timeScale = 0f;
    }

    // ===============================
    // クライアント停止時：シングルトン解除
    // ===============================
    public override void OnStopClient()
    {
        // 親クラスの停止処理を呼ぶ
        base.OnStopClient();

        // 登録済みなら解除
        if (Instance == this)
            Instance = null;
    }
}
