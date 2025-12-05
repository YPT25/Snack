using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// プレイヤーごとのマテリアルやモデルの情報を管理するクラス
/// ・PlayerColorChanger から呼ばれて色を保存
/// ・WeaponChange（変身）などからモデル番号を保存
/// ・シーン遷移後も状態を維持できる
/// </summary>
public class StatePlayer_Ashuri : NetworkBehaviour
{
    // ----------------------------------------------------
    // プレイヤーの色インデックスを保存する辞書
    // ----------------------------------------------------
    [Header("プレイヤー情報保持")]
    [Tooltip("プレイヤーごとのマテリアルインデックスを保持する辞書")]
    private Dictionary<NetworkConnectionToClient, int> playerMaterialIndex
        = new Dictionary<NetworkConnectionToClient, int>();

    // ----------------------------------------------------
    // プレイヤーのモデル番号を保存する辞書
    // ----------------------------------------------------
    [Tooltip("変身後のプレイヤーモデル番号を保持する辞書")]
    private Dictionary<NetworkConnectionToClient, int> savedModel
        = new Dictionary<NetworkConnectionToClient, int>();

    // ----------------------------------------------------
    // このオブジェクトをシーンを跨いでも残す処理
    // ----------------------------------------------------
    private void Awake()
    {
        // 1つ上：シーン遷移しても削除されないようにする
        DontDestroyOnLoad(gameObject);
    }

    // ----------------------------------------------------
    // プレイヤーの色インデックスを保存する処理
    // ----------------------------------------------------
    public void SavePlayerMaterial(NetworkConnectionToClient conn, int materialIndex)
    {
        // 1つ上：Dictionary に色番号を上書き保存
        playerMaterialIndex[conn] = materialIndex;

        // 1つ上：保存したことをログ出力
        Debug.Log($"[StatePlayer] プレイヤー {conn.connectionId} のマテリアルを {materialIndex} に保存しました");
    }

    // ----------------------------------------------------
    // 保存されている色インデックスを取得する処理
    // ----------------------------------------------------
    public int GetSavedMaterial(NetworkConnectionToClient conn)
    {
        // 1つ上：保存されている色を探す。あれば返す。
        if (playerMaterialIndex.TryGetValue(conn, out int index))
            return index;

        // 1つ上：保存がなければ 0 (デフォルトカラー)
        return 0;
    }

    // ----------------------------------------------------
    // 切断したプレイヤーのデータを削除する処理
    // ----------------------------------------------------
    public void RemovePlayer(NetworkConnectionToClient conn)
    {
        // 1つ上：色データが残っていれば削除する
        if (playerMaterialIndex.ContainsKey(conn))
        {
            playerMaterialIndex.Remove(conn);
            Debug.Log($"[StatePlayer] プレイヤー {conn.connectionId} の色データを削除しました");
        }

        // 1つ上：モデルデータも削除する
        if (savedModel.ContainsKey(conn))
        {
            savedModel.Remove(conn);
            Debug.Log($"[StatePlayer] プレイヤー {conn.connectionId} のモデルデータを削除しました");
        }
    }

    // ----------------------------------------------------
    // 全プレイヤー情報をクリア（ゲーム終了・サーバー停止時）
    // ----------------------------------------------------
    public void ClearAllPlayers()
    {
        // 1つ上：すべての色データを消去
        playerMaterialIndex.Clear();

        // 1つ上：すべてのモデルデータを消去
        savedModel.Clear();

        // 1つ上：ログ出力
        Debug.Log("[StatePlayer] 全プレイヤー情報をクリアしました");
    }

    // ----------------------------------------------------
    // プレイヤーのモデル番号を保存（変身時など）
    // ----------------------------------------------------
    public void SavePlayerModel(NetworkConnectionToClient conn, int modelIndex)
    {
        // 1つ上：プレイヤーのモデル番号を保存
        savedModel[conn] = modelIndex;
    }

    // ----------------------------------------------------
    // 保存されているプレイヤーモデル番号を取得
    // ----------------------------------------------------
    public int GetSavedModel(NetworkConnectionToClient conn)
    {
        // 1つ上：保存されていれば返す
        if (savedModel.TryGetValue(conn, out int index))
            return index;

        // 1つ上：なければデフォルトモデル（0）
        return 0;
    }

    public bool HasSavedModel(NetworkConnectionToClient conn)
    {
        return savedModel.ContainsKey(conn);
    }

}
