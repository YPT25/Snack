using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegacyInputHelper : MonoBehaviour
{
    // ------- 移動入力（キーボード & パッド） -------
    public static Vector2 GetMoveAxis()
    {
        float x = Input.GetAxis("Horizontal Pad");
        float z = Input.GetAxis("Vertical Pad");

        float keyX = Input.GetAxisRaw("Horizontal");
        float keyZ = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(keyX) > 0.1f) x = keyX;
        if (Mathf.Abs(keyZ) > 0.1f) z = keyZ;

        // デッドゾーン
        if (Mathf.Abs(x) < 0.15f) x = 0;
        if (Mathf.Abs(z) < 0.15f) z = 0;

        return new Vector2(x, z);
    }

    // ------- カメラ操作（マウス + 右スティック） -------
    public static Vector2 GetLookAxis()
    {
        float lookX = Input.GetAxis("Camera X");
        float lookY = Input.GetAxis("Camera Y");

        float padX = Input.GetAxis("CameraPad X");
        float padY = Input.GetAxis("CameraPad Y");

        // パッド右スティックが動いてるなら優先
        if (Mathf.Abs(padX) > 0.1f) lookX = padX;
        if (Mathf.Abs(padY) > 0.1f) lookY = padY;

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
        if (Mathf.Abs(Input.GetAxisRaw("Aiming Pad")) > 0.5f) return true;
        return false;
    }
}
