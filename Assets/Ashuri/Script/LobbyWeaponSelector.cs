using UnityEngine;
using Mirror;

/// <summary>
/// 武器に触れたら対応するプレイヤーに切り替える
/// </summary>
public class LobbyWeaponSelector : NetworkBehaviour
{
    [Header("切り替えるプレイヤータイプ（PlayerManagerのインデックス）")]
    public int playerTypeIndex = 0;

    // PlayerManagerをアタッチしておく
    public PlayerManager_Ashuri playerManager;

    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤー以外は無視する
        if (!collision.gameObject.CompareTag("Player")) return;

        // サーバーでのみ切り替え処理
        if (isServer)
        {
            Vector3 currentPos = collision.transform.position;
            Quaternion currentRot = collision.transform.rotation;

            // プレイヤー切り替え
            playerManager.ActivatePlayer(playerTypeIndex, currentPos, currentRot);
        }
    }
}
