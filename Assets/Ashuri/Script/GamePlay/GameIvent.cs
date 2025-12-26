using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIvent : NetworkBehaviour
{
    // ===============================
    // ピニャータの情報を取得する
    // ===============================
    [Header("ピニャータのオブジェクト")]
    [Tooltip("生成するピニャータのプレハブを取得する")]
    [SerializeField] private GameObject _Pinata;

    // ===============================
    // お菓子を管理しているオブジェクト
    // ===============================
    [Header("お菓子を管理しているオブジェクト")]
    [SerializeField] private GameObject _sweetContainer;

    // ===============================
    // ピニャータの生成場所
    // ===============================
    [Header("ピニャータの生成場所")]
    [Tooltip("ピニャータの生成場所のポジション")]
    [SerializeField] private List<Transform> _pinataPosition = new List<Transform>();

    // シングルトンとして自身を登録
    public static GameIvent Instance { get; private set; }

    // クライアント開始時に初期設定を行う
    public override void OnStartClient()
    {
        base.OnStartClient();

        // インスタンス登録
        if (Instance == null) Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// ピニャータを生成する
    /// </summary>
    [ClientRpc]
    public void AddPinata()
    {
        for (int i = 0; i < NetworkManager.singleton.numPlayers; i++)
        {
            // 生成場所を決める
            int rand = Random.Range(0, _pinataPosition.Count);
            // 生成ポジションを設定
            Transform _transform = _pinataPosition[rand];
            // ピニャータの生成
            GameObject gameObject = Instantiate(
                _Pinata,
                _transform.position,
                _transform.rotation);
            // サーバーに追加する
            NetworkServer.Spawn(gameObject);
        }
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
