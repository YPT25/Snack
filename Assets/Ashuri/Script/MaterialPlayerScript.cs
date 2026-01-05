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
}
