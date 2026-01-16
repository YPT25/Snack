using UnityEngine;
using Mirror;

public class MaterialPlayerScript : NetworkBehaviour
{
    // ===============================
    // 変更したいマテリアルのインデックス
    // ===============================
    [Header("変更したいマテリアルのインデックス")]
    [SerializeField] private int _materialIndex;

    // ===============================
    // スプレーのプレハブ
    // ===============================
    [Header("シェーダー")]
    [SerializeField] private GameObject m_SprayObject;

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

    private void Start()
    {
        // AudioSourceコンポーネントを取得
        audioSource = GetComponent<AudioSource>();
    }

    // ===============================
    // 衝突時の処理（クライアント側）
    // ===============================
    private void OnTriggerEnter(Collider other)
    {
        // PlayerColorChangerを取得
        var changer = other.gameObject.GetComponentInParent<PlayerColorChanger>();

        // ローカルプレイヤーのみ処理
        if (changer != null && changer.isLocalPlayer)
        {
            // クライアントからサーバーへ色変更リクエスト
            changer.CmdChangeMaterial(_materialIndex);

            // クライアントからサーバーへSE再生リクエスト
            CmdRequestPlaySE();
        }

        // ===============================
        // サーバー以外は以降の処理をしない
        // ===============================
        if (!isServer)
            return;

        // ===============================
        // スプレーオブジェクト生成
        // ===============================
        GameObject obj = Instantiate(
            m_SprayObject,
            transform.position,
            Quaternion.identity
        );

        // ===============================
        // ネットワーク上にSpawn
        // ===============================
        NetworkServer.Spawn(obj);

        // ===============================
        // 色をサーバー側で設定
        // ===============================
        SprayEffectMaterial spray = obj.GetComponent<SprayEffectMaterial>();
        if (spray != null)
        {
            spray.SetColorServer(_materialIndex);
        }
    }

    // ===============================
    // クライアント → サーバーへSE再生要求
    // ===============================
    [Command]
    private void CmdRequestPlaySE()
    {
        // サーバーから全クライアントへSE再生
        RpcPlaySE();
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
