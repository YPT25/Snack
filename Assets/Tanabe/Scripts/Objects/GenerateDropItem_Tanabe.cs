using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDropItem_Tanabe : NetworkBehaviour
{
    [Header("ドロップするアイテムの種類")]
    [Tooltip("ランダムで生成されるアイテムのプレハブ")]
    [SerializeField] private GameObject[] m_itemPrefabs;

    [Header("ドロップする数")]
    [Tooltip("特に指定がない場合にドロップする個数")]
    [SerializeField] private int m_dropItemCount;

    // ------------------------------
    // アイテムをドロップする関数
    // ------------------------------
    [Server]
    public void DropItems(int _dropCount = 0)
    {
        // 引数が0なら、設定された個数を使用する
        if (_dropCount == 0)
        {
            _dropCount = m_dropItemCount;
        }

        // 指定された回数分アイテムを生成する
        for (int i = 0; i < _dropCount; i++)
        {
            // アイテム生成処理を呼び出す
            GenerateItem();
        }
    }

    // ------------------------------
    // アイテムを生成して飛ばす処理（サーバー専用）
    // ------------------------------
    [Server]
    private void GenerateItem()
    {
        // ランダムでアイテムのプレハブを選択する
        GameObject prefab = m_itemPrefabs[Random.Range(0, m_itemPrefabs.Length)];

        // アイテムを生成する
        GameObject obj = Instantiate(prefab);

        // 生成位置をこのオブジェクトの位置に合わせる
        obj.transform.position = transform.position;

        // NetworkServer に登録して全クライアントに同期する
        NetworkServer.Spawn(obj);

        // Rigidbody を取得する
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        // Rigidbody が存在しない場合は処理しない
        if (rb == null)
        {
            return;
        }

        // ランダムな飛ぶ方向を作成する
        Vector3 randomDirection = new Vector3(
            Random.Range(-10, 11) * 0.1f,
            1.0f,
            Random.Range(-10, 11) * 0.1f
        );

        // 力を加える（上方向に少し強め）
        rb.AddForce(randomDirection.normalized * 5.0f, ForceMode.Impulse);
    }
}
