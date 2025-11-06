using UnityEngine;

public static class PlayerNameHolder
{
    // ----------------------------------------------
    // プレイヤー名を保持する変数（ゲーム全体で共有）
    // ----------------------------------------------
    public static string PlayerName;

    // ----------------------------------------------
    // プレイヤー名をセットするメソッド
    // ----------------------------------------------
    public static void SetPlayerName(string name)
    {
        PlayerName = name;
    }

    // ----------------------------------------------
    // プレイヤー名を取得するメソッド
    // ----------------------------------------------
    public static string GetPlayerName()
    {
        return PlayerName;
    }

    // ----------------------------------------------
    // プレイヤー名が設定されているか確認するメソッド
    // ----------------------------------------------
    public static bool HasPlayerName()
    {
        return !string.IsNullOrEmpty(PlayerName);
    }
}
