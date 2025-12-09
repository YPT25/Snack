using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testPlayerMenberCheck : NetworkBehaviour
{
    [Header("マテリアルの指定")]
    [Tooltip("プレイヤーが触れている間に表示するマテリアル")]
    [SerializeField] private Material _touchMaterial;

    // 元のマテリアルを保存
    private Material _defaultMaterial;

    // ===============================
    // 現在触れている人数
    // ===============================
    [SyncVar]
    private int _touchPlayerCount = 0;

    // ===============================
    // マテリアルの状態を全クライアントへ反映するための SyncVar
    // ・"default" または "touch" などを送る
    // ===============================
    [SyncVar(hook = nameof(OnMaterialStateChanged))]
    private string _materialState = "default";

    // Start は最初に呼ばれる
    void Start()
    {
        // デフォルトのマテリアルを保存
        _defaultMaterial = GetComponent<Renderer>().material;
    }

    // ===============================
    // マテリアル状態が変わった時に呼ばれる（クライアント全員）
    // ===============================
    private void OnMaterialStateChanged(string oldState, string newState)
    {
        // 新しい状態に応じてマテリアル変更
        if (newState == "touch")
        {
            // タッチ状態のマテリアルを反映
            GetComponent<Renderer>().material = _touchMaterial;
        }
        else
        {
            // デフォルトマテリアルを反映
            GetComponent<Renderer>().material = _defaultMaterial;
        }
    }

    // ===============================
    // 当たった時の処理（サーバーのみ）
    // ===============================
    private void OnTriggerEnter(Collider other)
    {
        // サーバーでなければ何もしない
        if (!isServer) return;

        // Player タグに触れた時
        if (other.CompareTag("Player"))
        {
            // プレイヤー数を増やす
            _touchPlayerCount++;

            // マテリアル状態を “touch” に変更（全クライアントに反映される）
            _materialState = "touch";
        }
    }

    // ===============================
    // 離れた時の処理（サーバーのみ）
    // ===============================
    private void OnTriggerExit(Collider other)
    {
        // サーバーでなければ何もしない
        if (!isServer) return;

        if (other.CompareTag("Player"))
        {
            // 人数を減算
            _touchPlayerCount--;

            // 0以下にならないように補正
            if (_touchPlayerCount <= 0)
            {
                _touchPlayerCount = 0;

                // マテリアル状態を “default” に戻す
                _materialState = "default";
            }
        }
    }

    // ===============================
    // 外部から人数取得
    // ===============================
    public int GetTouchPlayerCount()
    {
        return _touchPlayerCount;
    }
}
