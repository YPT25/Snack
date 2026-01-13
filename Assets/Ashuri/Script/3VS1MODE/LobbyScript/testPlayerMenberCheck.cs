using Mirror;
using Mirror.BouncyCastle.Asn1.X509;
using System.Collections.Generic;
using UnityEngine;

public class testPlayerMenberCheck : NetworkBehaviour
{
    [Header("マテリアルの指定")]
    [Tooltip("プレイヤーが触れている間に表示するマテリアル")]
    [SerializeField] private Material _touchMaterial;

    // 元のマテリアルを保存
    private Material _defaultMaterial;

    // 現在触れている人数（同期）
    [SyncVar]
    private int _touchPlayerCount = 0;

    // 触れているプレイヤーIDリスト（同期）
    public SyncList<uint> touchingPlayerIds = new SyncList<uint>();

    // マテリアル状態（同期）
    [SyncVar(hook = nameof(OnMaterialStateChanged))]
    private string _materialState = "default";

    [Header("このオブジェクトに割り当てるID (A=1, B=2など)")]
    [Tooltip("触れたプレイヤーに渡すID")]
    [SerializeField] private int _assignId = 1;


    // デフォルトマテリアルを保存する
    void Start()
    {
        _defaultMaterial = GetComponent<Renderer>().material;
    }


    // マテリアルの状態が変わったときに呼ばれる
    private void OnMaterialStateChanged(string oldState, string newState)
    {
        if (newState == "touch")
        {
            // タッチ中のマテリアルを設定
            GetComponent<Renderer>().material = _touchMaterial;
        }
        else
        {
            // デフォルトマテリアルに戻す
            GetComponent<Renderer>().material = _defaultMaterial;
        }
    }


    // ===============================
    // プレイヤーが入った時（サーバーのみ）
    // ===============================
    private void OnTriggerEnter(Collider other)
    {
        // サーバー以外は処理しない
        if (!isServer) return;

        // Player タグ確認
        if (!other.CompareTag("Player")) return;

        // 人数管理
        _touchPlayerCount++;
        _materialState = "touch";

        // Player_Tanabe を取得
        Player_Tanabe holder = other.GetComponent<Player_Tanabe>();
        if (holder == null) return;

        // チームごとの設定
        if (_assignId == 0)
        {
            holder.ServerSetTeamAndName("Team1", Color.red, 0);
            Debug.LogError("設定された");
        }
        else
        {
            holder.ServerSetTeamAndName("Team2", Color.blue, 1);
            Debug.LogError("設定された");
        }
    }


    // プレイヤーが離れた時の処理（サーバーのみ）
    private void OnTriggerExit(Collider other)
    {
        // サーバー以外は処理しない
        if (!isServer) return;

        if (other.CompareTag("Player"))
        {
            // 人数を減らす
            _touchPlayerCount--;

            // カウントが0になったらマテリアルを戻す
            if (_touchPlayerCount <= 0)
            {
                _touchPlayerCount = 0;
                _materialState = "default";
            }

            // プレイヤーかチェックする
            SingleTeamModelSwitcher_Ashuri singleTeamModelSwitcher_Ashuri = other.GetComponent<SingleTeamModelSwitcher_Ashuri>();
            if (singleTeamModelSwitcher_Ashuri == null) return;

            if (!singleTeamModelSwitcher_Ashuri.isLocalPlayer) return;

            singleTeamModelSwitcher_Ashuri.TryChangePlayer(0);

            // プレイヤーのIDを取得してリストから削除
            var identity = other.GetComponent<NetworkIdentity>();
            if (identity != null)
            {
                uint playerId = identity.netId;

                if (touchingPlayerIds.Contains(playerId))
                {
                    touchingPlayerIds.Remove(playerId);
                }
            }
        }
    }


    // 外部から人数を取得する
    public int GetTouchPlayerCount()
    {
        return _touchPlayerCount;
    }


    // 外部からプレイヤーIDリストを取得する
    public List<uint> GetTouchingPlayerIds()
    {
        return new List<uint>(touchingPlayerIds);
    }
}
