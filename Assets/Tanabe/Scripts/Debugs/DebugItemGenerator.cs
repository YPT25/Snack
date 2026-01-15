using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugItemGenerator : NetworkBehaviour
{
    [Header("生成するアイテムのプレハブ")]
    [Tooltip("トリガーに入ったとき生成されるアイテム")]
    [SerializeField] private GameObject m_itemPrefab;

    [Header("SE")]
    [Header("ボタンを押した音")]
    [SerializeField] public AudioClip sound1;

    [Tooltip("AudioSource")] private AudioSource audioSource;
    // 当たったトリガー
    private bool _isTrigger = false;

    private void Start()
    {
        //Componentを取得
        audioSource = GetComponent<AudioSource>();
    }

    // ------------------------------
    // サーバー専用：トリガー侵入時の処理
    // ------------------------------
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.GetComponent<Player_Tanabe>() == null) { return; }

        // 生成したか確認する
        if (_isTrigger) return;

        // 生成したらtrueにする
        _isTrigger = true;

        // アイテムを生成する
        GameObject obj = Instantiate(m_itemPrefab, transform.position, Quaternion.identity);

        // ネットワーク上にスポーンさせる
        NetworkServer.Spawn(obj);

        // 音声を流す
        this.RpcPlaySE();
    }

    [ServerCallback]

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.GetComponent<Player_Tanabe>() == null) { return; }

        // 離れたらfalse
        _isTrigger = false;
    }
    // ===============================
    // サーバーから全クライアントへSE再生
    // ===============================
    [ClientRpc]
    private void RpcPlaySE()
    {
        // AudioSourceが存在しない場合は処理しない
        if (audioSource == null) return;

        // 効果音を再生
        audioSource.PlayOneShot(sound1);
    }
}
