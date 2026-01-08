using Mirror;                     // Mirrorネットワーク機能
using System.Collections;
using System.Collections.Generic;
using TMPro;                      // TextMeshProを使用
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;             // Unityの基本クラス使用

/// <summary>
/// ゲーム終了後のスコアUIを管理するクラス
/// GameManagerから呼び出されてUIを表示し、一定時間後にロビーへ戻す
/// </summary>
public class ResultUIScore : NetworkBehaviour
{
    // ===============================
    // シングルトンインスタンス
    // ===============================
    public static ResultUIScore Instance { get; private set; }

    // ===============================
    // 王冠プレハブリスト
    // ===============================
    [Header("王冠プレハブ")]
    [SerializeField] private List<GameObject> crownPrefab = new List<GameObject>();

    // ===============================
    // お菓子管理オブジェクト
    // ===============================
    [Header("お菓子を管理しているオブジェクト")]
    [SerializeField] private GameObject _sweetContainer;

    // ===============================
    // ロビーへ戻るまでの時間
    // ===============================
    [Tooltip("ロビーに戻るまでの時間（秒）")]
    [SerializeField] private float _countResult = 10f;

    // ===============================
    // 結果処理が開始されたか
    // ===============================
    private bool _isResultStarted = false;

    // ===============================
    // ロビー遷移を実行したか
    // ===============================
    private bool _isReturnedLobby = false;

    // ===============================
    // クライアント開始時処理
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();

        // シングルトン登録
        if (Instance == null)
            Instance = this;
    }

    // ===============================
    // 毎フレーム処理（サーバーのみ）
    // ===============================
    private void Update()
    {
        // サーバー以外では処理しない
        if (!isServer) return;

        // 結果表示が始まっていなければ処理しない
        if (!_isResultStarted) return;

        // すでにロビーへ戻っていたら処理しない
        if (_isReturnedLobby) return;

        // 時間を減らす
        _countResult -= Time.deltaTime;

        // 時間が0以下になったらロビーへ戻る
        if (_countResult <= 0f)
        {
            // 二重実行防止
            _isReturnedLobby = true;

            // ロビーへ戻る処理を実行
            OnClickReturnLobby();
        }
    }

    // ===============================
    // スコア表示を全クライアントへ送信
    // ===============================
    [ClientRpc]
    public void RpcShowScore(float finalScore)
    {
        // お菓子オブジェクトを非表示
        _sweetContainer.SetActive(false);

        // スコアUI表示
        ShowScore(finalScore);
    }

    // ===============================
    // スコアUI表示処理
    // ===============================
    public void ShowScore(float finalScore)
    {
        // 結果処理開始フラグを立てる（サーバー）
        if (isServer)
            _isResultStarted = true;

        // デバッグログ
        Debug.Log("Game Over! Showing Result UI");

        // 全プレイヤー取得
        Player_Tanabe[] players = FindObjectsOfType<Player_Tanabe>();

        // スコア順に並び替え（降順）
        System.Array.Sort(players, (a, b) => b.m_sweetScore.CompareTo(a.m_sweetScore));

        // 順位ごとに王冠を付与
        for (int i = 0; i < players.Length; i++)
        {
            // 対象プレイヤー取得
            Player_Tanabe p = players[i];

            // 対応する王冠が存在するか確認
            if (i < crownPrefab.Count && crownPrefab[i] != null)
            {
                // 王冠生成
                GameObject crownInstance = Instantiate(crownPrefab[i]);

                // 王冠制御スクリプト取得
                BWCrownManager crownManager = crownInstance.GetComponent<BWCrownManager>();

                // プレイヤーに追従設定
                if (crownManager != null)
                {
                    crownManager.SetPlayerPosition(p.transform);
                }
            }
        }
    }

    // ===============================
    // ロビーへ戻る処理（サーバー専用）
    // ===============================
    private void OnClickReturnLobby()
    {
        // サーバー以外では処理しない
        if (!isServer) return;

        // 時間停止解除
        Time.timeScale = 1f;

        // ロビーシーンへ遷移
        NetworkManager.singleton.ServerChangeScene("LobbyScene");
    }

    // ===============================
    // クライアント終了時処理
    // ===============================
    public override void OnStopClient()
    {
        base.OnStopClient();

        // インスタンス解除
        if (Instance == this)
            Instance = null;
    }
}
