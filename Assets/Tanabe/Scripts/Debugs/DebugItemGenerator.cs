using UnityEngine;
using Mirror;

public class DebugItemGenerator : NetworkBehaviour
{
    // ===============================
    // 生成するアイテムのプレハブ
    // ===============================
    [Header("生成するアイテムのプレハブ")]
    [Tooltip("トリガーに入ったとき生成されるアイテム")]
    [SerializeField] private GameObject m_itemPrefab;

    // ===============================
    // 効果音
    // ===============================
    [Header("SE")]
    [Tooltip("ボタンを押した音")]
    [SerializeField] private AudioClip sound1;

    // ===============================
    // AudioSource
    // ===============================
    private AudioSource audioSource;

    // ===============================
    // トリガー判定フラグ
    // ===============================
    private bool _isTrigger = false;

    private void Start()
    {
        // AudioSourceコンポーネントを取得
        audioSource = GetComponent<AudioSource>();
    }

    // ------------------------------
    // サーバー専用：トリガー侵入時の処理
    // ------------------------------
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        // レイヤー6 または Player_Tanabe が無ければ処理しない
        if (other.gameObject.layer == 6 || other.gameObject.GetComponent<Player_Tanabe>() == null)
            return;

        // すでに生成済みなら処理しない
        if (_isTrigger) return;

        // 生成済みフラグを立てる
        _isTrigger = true;

        // アイテムを生成
        GameObject obj = Instantiate(
            m_itemPrefab,
            transform.position,
            Quaternion.identity
        );

        // ネットワーク上にスポーン
        NetworkServer.Spawn(obj);

        // サーバーから全クライアントへSE再生
        RpcPlaySE();
    }

    // ------------------------------
    // サーバー専用：トリガー退出時の処理
    // ------------------------------
    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        // レイヤー6 または Player_Tanabe が無ければ処理しない
        if (other.gameObject.layer == 6 || other.gameObject.GetComponent<Player_Tanabe>() == null)
            return;

        // トリガー解除
        _isTrigger = false;
    }

    // ===============================
    // サーバー → 全クライアントへSE再生
    // ===============================
    [ClientRpc]
    private void RpcPlaySE()
    {
        // AudioSourceが無ければ処理しない
        if (audioSource == null) return;

        // 効果音を再生
        audioSource.PlayOneShot(sound1);
    }
}
