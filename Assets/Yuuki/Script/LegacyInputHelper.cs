using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LegacyInputHelper
{
    // パッド用 DeadZone（小さめに）
    public static float padDeadZone = 0.02f;

    // パッド用の感度乗数（右スティックは小さく出ることが多いため増幅）
    public static float padSensitivityMultiplier = 20f;

    // Pad Y を反転したい場合は true（PS/Xbox の場合、上下符号が違うことがある）
    public static bool invertPadY = true;

    // ------- 移動入力（キーボード & パッド） -------
    public static Vector2 GetMoveAxis()
    {
        float x = Input.GetAxis("Horizontal Pad");
        float z = Input.GetAxis("Vertical Pad");

        float keyX = Input.GetAxisRaw("Horizontal");
        float keyZ = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(keyX) > 0.1f) x = keyX;
        if (Mathf.Abs(keyZ) > 0.1f) z = keyZ;

        // デッドゾーン（移動は少し大きめでも良い）
        if (Mathf.Abs(x) < 0.15f) x = 0;
        if (Mathf.Abs(z) < 0.15f) z = 0;

        return new Vector2(x, z);
    }

    // ------- カメラ操作（マウス + 右スティック） -------
    // 戻り値は「未スケーリングの軸値」（MPlayerBase 側で mouseSensitivity を掛ける想定）
    public static Vector2 GetLookAxis()
    {
        // マウス（または既に設定された Camera X/Y）
        float lookX = Input.GetAxis("Camera X"); // マウス/キーボード系
        float lookY = Input.GetAxis("Camera Y");

        // パッドの右スティック
        float padX = Input.GetAxis("CameraPad X");
        float padY = Input.GetAxis("CameraPad Y");

        // デッドゾーン処理（非常に小さい振れは無視）
        if (Mathf.Abs(padX) < padDeadZone) padX = 0f;
        if (Mathf.Abs(padY) < padDeadZone) padY = 0f;

        // パッドが動いているならパッド優先で、2乗カーブ + 感度乗数をかける
        if (Mathf.Abs(padX) > 0f || Mathf.Abs(padY) > 0f)
        {
            // 2乗カーブで小さい入力を扱いやすくする（符号維持）
            float px = padX * Mathf.Abs(padX);
            float py = padY * Mathf.Abs(padY);

            // Y反転オプション
            if (invertPadY) py = -py;

            // 増幅して返す（MPlayerBase 側でさらに mouseSensitivity を掛ける想定）
            return new Vector2(px * padSensitivityMultiplier, py * padSensitivityMultiplier);
        }

        // パッド無動作 → マウスを返す（マウスはそのまま返す）
        return new Vector2(lookX, lookY);
    }

    // ------- 攻撃 -------
    public static bool GetAttackDown()
    {
        return Input.GetMouseButtonDown(0) || Input.GetButtonDown("Attack");
    }

    // ------- エイム -------
    public static bool GetAim()
    {
        if (Input.GetMouseButton(1)) return true;
        if (Mathf.Abs(Input.GetAxis("Aiming Pad")) > 0.5f) return true;
        return false;
    }
}