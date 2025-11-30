using Mirror;                     // Mirrorネットワーク機能
using TMPro;                      // TextMeshProを使用
using UnityEngine;
using UnityEngine.UI;              // Unityの基本クラス使用
using UnityEngine.SceneManagement; // シーン遷移のために必要
using System.Collections;

/// <summary>
/// ゲーム終了後のスコアUIを管理するクラス
/// GameManagerから呼び出されてUIを表示し、ゲームを停止させる
/// </summary>
public class ResultUIScore : NetworkBehaviour
{
    [Header("スコアUI関連")]
    [Tooltip("スコアを表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("スコアパネル（非表示→表示切り替え）")]
    [SerializeField] private GameObject scorePanel;

    [Tooltip("ロビーシーンに戻るボタン")]
    [SerializeField] private Button lobbySceneButton;

    [Tooltip("ロビーシーンで非表示にしたいCanvasの名前")]
    [SerializeField] private string lobbyCanvasName = "NetworkCanvas";

    public static ResultUIScore Instance { get; private set; }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (Instance == null) Instance = this;

        if (scorePanel != null)
            scorePanel.SetActive(false);

        if (lobbySceneButton != null)
        {
            lobbySceneButton.onClick.AddListener(OnClickReturnLobby);

            if (!isServer)
                lobbySceneButton.interactable = false;
        }
    }

    private void Update()
    {
        if (!isServer) return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            OnClickReturnLobby();
        }
    }

    [ClientRpc]
    public void RpcShowScore(float finalScore)
    {
        ShowScore(finalScore);

        if (lobbySceneButton != null)
            lobbySceneButton.gameObject.SetActive(true);
    }

    public void ShowScore(float finalScore)
    {
        Debug.Log("Game Over! Showing Score (Client)");

        Player_Tanabe[] players = FindObjectsOfType<Player_Tanabe>();

        if (scorePanel != null)
            scorePanel.SetActive(true);

        if (scoreText != null)
        {
            string allScores = "";

            for (int i = 0; i < players.Length; i++)
            {
                Player_Tanabe p = players[i];
                //allScores += $"Player{p.playerNumber}: {p.m_sweetScore}\n";
            }

            //allScores += $"\nYour team Score: {finalScore}";

            scoreText.text = allScores;
        }

        System.Array.Sort(players, (a, b) => b.m_sweetScore.CompareTo(a.m_sweetScore));

        string rankingText = "\n\n--- Ranking ---\n";

        for (int i = 0; i < players.Length; i++)
        {
            Player_Tanabe p = players[i];
            rankingText += $"Number{i + 1}: Player{p.playerNumber} - {p.m_sweetScore}PT\n";
        }

        if (scoreText != null)
            scoreText.text += rankingText;

        Time.timeScale = 0f;
    }

    private void OnClickReturnLobby()
    {
        if (!isServer) return;

        Time.timeScale = 1f;

        // ロビーシーンへ移動
        NetworkManager.singleton.ServerChangeScene("LobbyScene");

        // Canvas を非表示にする処理を全クライアントで実行
        RpcHideLobbyCanvas();
    }

    [ClientRpc]
    private void RpcHideLobbyCanvas()
    {
        // シーン移動後に少し待って Canvas を取得する
        StartCoroutine(HideCanvasAfterDelay());
    }

    private IEnumerator HideCanvasAfterDelay()
    {
        // 数フレーム待つことで Canvas が生成されるのを待つ
        for (int i = 0; i < 5; i++)
            yield return null;

        GameObject canvas = GameObject.Find(lobbyCanvasName);
        if (canvas != null)
        {
            canvas.SetActive(false);
            Debug.Log($"{lobbyCanvasName} を非表示にしました");
        }
        else
        {
            Debug.LogWarning($"{lobbyCanvasName} が見つかりませんでした");
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (Instance == this)
            Instance = null;
    }
}
