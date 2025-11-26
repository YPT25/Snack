using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// プレイヤーPrefabをまとめて管理するクラス
/// ゲーム開始時にNormalPlayer（index 0）を初期位置で表示
/// NetworkStartPositionを使って順番通りにスポーン
/// </summary>
public class PlayerManager_Ashuri : NetworkBehaviour
{
    [Header("プレイヤーPrefab（子オブジェクト）")]
    [Tooltip("Normal/Gun/HummerのプレイヤーをPlayerManagerの子オブジェクトとしてアタッチしてください")]
    public List<GameObject> playerPrefabs = new List<GameObject>();

    /// <summary>
    /// Awakeで子オブジェクトを自動でリスト化し、初期状態を非表示にする
    /// </summary>
    private void Awake()
    {
        // 子オブジェクトをリスト化
        playerPrefabs.Clear();
        foreach (Transform child in transform)
        {
            playerPrefabs.Add(child.gameObject);
        }

        // 最初は全て非表示に設定
        foreach (GameObject player in playerPrefabs)
        {
            player.SetActive(false);
        }
    }

    /// <summary>
    /// サーバーが開始したタイミングでNormalPlayerを初期位置にスポーン
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // NetworkStartPositionを順番通りに取得
        NetworkStartPosition[] startPositions = FindObjectsOfType<NetworkStartPosition>();

        Vector3 startPos = Vector3.zero;           // 初期位置
        Quaternion startRot = Quaternion.identity; // 初期回転

        if (startPositions.Length > 0)
        {
            // 最初のNetworkStartPositionを使用
            startPos = startPositions[0].transform.position;
            startRot = startPositions[0].transform.rotation;
        }

        // NormalPlayer（index 0）を表示
        ActivatePlayer(0, startPos, startRot);
    }

    /// <summary>
    /// 指定したタイプのプレイヤーのみをアクティブにし、その他を非表示にする
    /// サーバーで呼ぶことを前提
    /// </summary>
    /// <param name="typeIndex">playerPrefabsのインデックス</param>
    /// <param name="position">プレイヤーのスポーン位置</param>
    /// <param name="rotation">プレイヤーの初期回転</param>
    [Server]
    public void ActivatePlayer(int typeIndex, Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            GameObject player = playerPrefabs[i];
            bool isActive = (i == typeIndex);

            // アクティブ／非アクティブを切り替え
            player.SetActive(isActive);

            // アクティブにするプレイヤーは位置と回転を設定
            if (isActive)
            {
                player.transform.position = position;
                player.transform.rotation = rotation;
            }
        }

        // クライアント側にも同じ状態を同期
        RpcUpdateClientPlayers(typeIndex, position, rotation);
    }

    /// <summary>
    /// クライアント側でプレイヤーを同期
    /// サーバー側から呼ばれるClientRpc
    /// </summary>
    /// <param name="typeIndex">アクティブにするプレイヤーのインデックス</param>
    /// <param name="position">プレイヤーの座標</param>
    /// <param name="rotation">プレイヤーの回転</param>
    [ClientRpc]
    private void RpcUpdateClientPlayers(int typeIndex, Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            GameObject player = playerPrefabs[i];
            bool isActive = (i == typeIndex);

            // 表示／非表示切り替え
            player.SetActive(isActive);

            // アクティブなプレイヤーは座標・回転を反映
            if (isActive)
            {
                player.transform.position = position;
                player.transform.rotation = rotation;
            }
        }
    }
}
