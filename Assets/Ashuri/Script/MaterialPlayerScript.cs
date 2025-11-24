using UnityEngine;
using Mirror;

public class MaterialPlayerScript : NetworkBehaviour
{
    [Header("変更したいマテリアルのインデックス")]
    [SerializeField] private int _materialIndex;

    // 衝突した時に呼ばれる処理
    private void OnCollisionEnter(Collision collision)
    {
        // 衝突相手から PlayerColorChanger を取得
        var changer = collision.gameObject.GetComponentInParent<PlayerColorChanger>();

        // changer が存在し なおかつ ローカルプレイヤーなら実行
        if (changer != null && changer.isLocalPlayer)
        {
            // クライアント → サーバーへ色変更リクエスト
            changer.CmdChangeMaterial(_materialIndex);
        }
    }
}
