using UnityEngine;
using Mirror;

public class MaterialPlayerScript : NetworkBehaviour
{
    [Header("変更したいマテリアルのインデックス")]
    [SerializeField] private int _materialIndex;

    // 衝突した時に呼ばれる処理
    private void OnTriggerEnter(Collider other)
    {
        // 衝突相手から PlayerColorChanger を取得
        var changer = other.gameObject.GetComponentInParent<PlayerColorChanger>();

        // changer が存在し なおかつ ローカルプレイヤーなら実行
        if (changer != null && changer.isLocalPlayer)
        {
            // クライアント → サーバーへ色変更リクエスト
            changer.CmdChangeMaterial(_materialIndex);
        }
    }
}
