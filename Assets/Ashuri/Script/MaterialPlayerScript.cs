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

    [Header("SE")]
    [Header("ボタンを押した音")]
    [SerializeField] public AudioClip sound1;

    [Tooltip("AudioSource")] private AudioSource audioSource;

    private void Start()
    {
        //Componentを取得
        audioSource = GetComponent<AudioSource>();
    }

    // ===============================
    // 衝突時の処理
    // ===============================
    private void OnTriggerEnter(Collider other)
    {
        // ===============================
        // ローカルプレイヤー判定
        // ===============================
        // 衝突相手から PlayerColorChanger を取得
        var changer = other.gameObject.GetComponentInParent<PlayerColorChanger>();

        // changer が存在し なおかつ ローカルプレイヤーなら実行
        if (changer != null && changer.isLocalPlayer)
        {
            // クライアント → サーバーへ色変更リクエスト
            changer.CmdChangeMaterial(_materialIndex);
            //音声を流す
            this.RpcPlaySE();
        }

        // ===============================
        // サーバーでのみ生成
        // ===============================
        if (!isServer)
            return;

        // ===============================
        // スプレー生成
        // ===============================
        GameObject obj = Instantiate(
            m_SprayObject,
            transform.position,
            Quaternion.identity
        );

        // ===============================
        // ネットワークSpawn
        // ===============================
        NetworkServer.Spawn(obj);

        // ===============================
        // 色を設定
        // ===============================
        SprayEffectMaterial spray = obj.GetComponent<SprayEffectMaterial>();
        if (spray != null)
        {
            spray.SetColorServer(_materialIndex);
        }
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
