using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// プレイヤーごとのマテリアルインデックスを管理するクラス
/// PlayerColorChanger と連携して、シーン遷移後も色を復元可能
/// </summary>
public class StatePlayer_Ashuri : NetworkBehaviour
{
    // ------------------------------
    // プレイヤーごとのマテリアル情報を保持
    // ------------------------------
    [Header("プレイヤー情報保持")]
    [Tooltip("プレイヤーごとのマテリアルインデックスを保持する辞書")]
    private Dictionary<NetworkConnectionToClient, int> playerMaterialIndex = new Dictionary<NetworkConnectionToClient, int>();

    // ----------------------------------------------------
    // Awake でオブジェクトを破棄させない
    // ----------------------------------------------------
    private void Awake()
    {
        // シーンを跨いでもオブジェクトを破棄しない
        DontDestroyOnLoad(gameObject);
    }

    // ----------------------------------------------------
    // プレイヤーのマテリアルを保存
    // ----------------------------------------------------
    /// <summary>
    /// プレイヤーが色を変更したときに呼ぶ
    /// </summary>
    /// <param name="conn">プレイヤーの接続</param>
    /// <param name="materialIndex">変更されたマテリアルのインデックス</param>
    public void SavePlayerMaterial(NetworkConnectionToClient conn, int materialIndex)
    {
        playerMaterialIndex[conn] = materialIndex;
        Debug.Log($"[StatePlayer] プレイヤー {conn.connectionId} のマテリアルを {materialIndex} に保存しました");
    }

    // ----------------------------------------------------
    // プレイヤーの保存されているマテリアルを取得
    // ----------------------------------------------------
    /// <summary>
    /// 保存されたマテリアルインデックスを取得
    /// </summary>
    /// <param name="conn">プレイヤーの接続</param>
    /// <returns>保存されたマテリアルインデックス、存在しなければ0</returns>
    public int GetSavedMaterial(NetworkConnectionToClient conn)
    {
        if (playerMaterialIndex.TryGetValue(conn, out int index))
        {
            return index;
        }
        return 0;
    }

    // ----------------------------------------------------
    // プレイヤーが切断したときに情報を削除
    // ----------------------------------------------------
    /// <summary>
    /// 切断されたプレイヤーの情報を辞書から削除
    /// </summary>
    /// <param name="conn">切断されたプレイヤーの接続</param>
    public void RemovePlayer(NetworkConnectionToClient conn)
    {
        if (playerMaterialIndex.ContainsKey(conn))
        {
            playerMaterialIndex.Remove(conn);
            Debug.Log($"[StatePlayer] プレイヤー {conn.connectionId} の情報を削除しました");
        }
    }

    // ----------------------------------------------------
    // 現在保持しているすべてのプレイヤー情報をクリア
    // ----------------------------------------------------
    /// <summary>
    /// ゲーム終了時やサーバー停止時に全情報をリセット
    /// </summary>
    public void ClearAllPlayers()
    {
        playerMaterialIndex.Clear();
        Debug.Log("[StatePlayer] 全プレイヤー情報をクリアしました");
    }


}
