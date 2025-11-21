using UnityEngine;
using Mirror;

public class MaterialPlayerScript : NetworkBehaviour
{
    [Header("変更したいマテリアル")]
    [SerializeField] private int _materialIndex;

    private void OnCollisionEnter(Collision collision)
    {
        var changer = collision.gameObject.GetComponentInParent<PlayerColorChanger>();

        if (changer != null && changer.isLocalPlayer)
        {
            // Cmd でサーバーにリクエスト
            changer.CmdChangeMaterial(_materialIndex);
        }
    }
}
