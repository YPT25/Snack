using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SweetParentSetter : NetworkBehaviour
{
    /// <summary>
    /// クライアントに呼び出され、指定されたNetworkIdentityを持つオブジェクトを親として設定する。
    /// </summary>
    /// <param name="parentIdentity">親として設定するオブジェクトのNetworkIdentity</param>
    [ClientRpc]
    public void RpcSetParent(NetworkIdentity parentIdentity)
    {
        // サーバーがホストの場合、既にサーバー側で親が設定されている可能性があるため、重複設定を避ける
        // ただし、NetworkServer.Spawn後、ClientRpcが呼ばれる前にクライアント側で親がルートになっている可能性もある
        // ここでは、isServer && isClient の場合はスキップせず、クライアント側でのみSetParentを実行する
        // if (isServer && isClient) return; // ホストの場合はスキップ (サーバー側で設定済みのため)

        if (parentIdentity != null)
        {
            // クライアント側で親オブジェクトのTransformを取得し、自身の子に設定
            transform.SetParent(parentIdentity.transform);

            // 親を設定した後、ローカル座標や回転をリセットして親に対する相対位置を調整する
            //transform.localPosition = Vector3.zero; // 親の原点に配置
            transform.localRotation = Quaternion.identity; // 親と同じ回転
            transform.localScale = Vector3.one; // 親と同じスケール (必要に応じて)
        }
        else
        {
            Debug.LogWarning("RpcSetParent: parentIdentity is null on client. Cannot set parent.");
        }
    }
}
