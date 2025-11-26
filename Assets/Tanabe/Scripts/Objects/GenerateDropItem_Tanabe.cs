using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDropItem_Tanabe : NetworkBehaviour
{
    [Header("ドロップするアイテムの種類"), SerializeField] private GameObject[] m_itemPrefabs;
    [Header("ドロップする数"), SerializeField] private int m_dropItemCount;

    // アイテムをドロップする関数
    public void DropItems()
    {
        for (int i = 0; i < m_dropItemCount; i++)
        {
            this.GenerateItem();
        }
    }

    // アイテムの生成してランダムな方向に飛ばす処理
    private void GenerateItem()
    {
        GameObject obj = Instantiate(m_itemPrefabs[Random.Range(0, m_itemPrefabs.Length)]);
        obj.transform.position = this.transform.position;
        // セットパーツを上方向に飛ばす処理
        Vector3 moveVector = new Vector3((float)UnityEngine.Random.Range(-10, 11) * 0.1f, 3.0f, (float)UnityEngine.Random.Range(-10, 11) * 0.1f);
        this.CmdAddForce_Item(obj.GetComponent<ItemStateMachine>(), moveVector.normalized * 5.0f, ForceMode.Impulse);
        NetworkServer.Spawn(obj);
    }

    // アイテムの移動処理(サーバーのみ)
    [Command]
    private void CmdAddForce_Item(ItemStateMachine _item, Vector3 _moveForce, ForceMode _forceMode)
    {
        _item.GetRigidbody().AddForce(_moveForce, _forceMode);
        this.RpcAddForce_Item(_item, _moveForce, _forceMode);
    }

    // アイテムの移動処理(クライアントのみ)
    [ClientRpc]
    private void RpcAddForce_Item(ItemStateMachine _item, Vector3 _moveForce, ForceMode _forceMode)
    {
        _item.GetRigidbody().AddForce(_moveForce, _forceMode);
    }

}
