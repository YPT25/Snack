using Mirror;                     // Mirrorネットワーク機能
using System.Collections;
using System.Collections.Generic;
using TMPro;                      // TextMeshProを使用
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;             // Unityの基本クラス使用

/// <summary>
/// ゲーム終了後のスコアUIを管理するクラス
/// GameManagerから呼び出されてUIを表示し、ゲームを停止させる
/// </summary>
public class ResultUIScore : NetworkBehaviour
{
    // シングルトンとして自身を登録
    public static ResultUIScore Instance { get; private set; }

    // ===============================
    // 生成するオブジェクトのリスト
    // ===============================
    [Header("王冠プレハブ")]
    [SerializeField] private List<GameObject> crownPrefab = new List<GameObject>();

    // ===============================
    // お菓子を管理しているオブジェクト
    // ===============================
    [Header("お菓子を管理しているオブジェクト")]
    [SerializeField] private GameObject _sweetContainer;


    // クライアント開始時に初期設定を行う
    public override void OnStartClient()
    {
        base.OnStartClient();

        // インスタンス登録
        if (Instance == null) Instance = this;
    }


    // 毎フレーム、サーバーのみボタン判定を行う
    private void Update()
    {
        if (!isServer) return;

        // Nキーでロビーに戻れるようにする（サーバー用）
        if (Input.GetKeyDown(KeyCode.N))
        {
            OnClickReturnLobby();
        }
    }


    // スコア表示をすべてのクライアントに送る
    [ClientRpc]
    public void RpcShowScore(float finalScore)
    {
        //お菓子を削除
        _sweetContainer.SetActive(false);
        // スコアを表示する処理
        ShowScore(finalScore);
    }


    // スコア UI を生成して表示する
    public void ShowScore(float finalScore)
    {
        // デバッグログを表示する
        Debug.Log("Game Over! Showing Crowns Instead of Score (Client)");

        // 全プレイヤー情報を取得する
        Player_Tanabe[] players = FindObjectsOfType<Player_Tanabe>();

        // スコア順に並び替え（降順）
        System.Array.Sort(players, (a, b) => b.m_sweetScore.CompareTo(a.m_sweetScore));
        // ランキング順にクラウン（BWCrownManager付き）を割り当てる処理
        for (int i = 0; i < players.Length; i++)
        {
            // 対象プレイヤーを取得する
            Player_Tanabe p = players[i];

            // ランキングの並びに対応するクラウンオブジェクトがあるか確認
            if (i < crownPrefab.Count && crownPrefab[i] != null)
            {
                // crownPrefab には BWCrownManager を持つオブジェクトが入っている前提
                GameObject crownObj = crownPrefab[i];

                // プレハブをシーンに生成（位置は適当、後で BWCrownManager が動かす）
                GameObject crownInstance = Instantiate(crownObj);

                // BWCrownManager を取得
                BWCrownManager crownManager = crownInstance.GetComponent<BWCrownManager>();

                if (crownManager != null)
                {
                    // プレイヤーの Transform を登録
                    crownManager.SetPlayerPosition(p.transform);

                    Debug.Log($"Player{p.playerNumber} に {i + 1} 位クラウンを割り当てました。");
                }
                else
                {
                    Debug.LogWarning("BWCrownManager が crownPrefab に付いていません");
                }
            }
        }


        // ゲームを一時停止する
        //Time.timeScale = 0f;
    }


    // ロビーに戻す処理（サーバーのみ）
    private void OnClickReturnLobby()
    {
        if (!isServer) return;

        // 時間を通常に戻す
        Time.timeScale = 1f;

        // サーバー側でロビーシーンへ遷移
        NetworkManager.singleton.ServerChangeScene("LobbyScene");
    }

    // クライアント終了時にインスタンスを削除
    public override void OnStopClient()
    {
        base.OnStopClient();

        // 自身がインスタンスなら解除
        if (Instance == this)
            Instance = null;
    }
}
