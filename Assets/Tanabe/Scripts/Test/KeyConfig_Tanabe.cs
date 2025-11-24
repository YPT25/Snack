using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionBinding
{
    public KeyCode keyboardKey;
    public KeyCode gamepadKey;  // JoystickButton系

    public KeyConfig_Tanabe.InputType currentType = KeyConfig_Tanabe.InputType.Keyboard;
}

[System.Serializable]
public class KeyConfig
{
    public ActionBinding MoveUp = new ActionBinding() { keyboardKey = KeyCode.W, gamepadKey = KeyCode.Joystick1Button4 };
    public ActionBinding MoveDown = new ActionBinding() { keyboardKey = KeyCode.S, gamepadKey = KeyCode.Joystick1Button6 };
    public ActionBinding MoveLeft = new ActionBinding() { keyboardKey = KeyCode.A, gamepadKey = KeyCode.Joystick1Button7 };
    public ActionBinding MoveRight = new ActionBinding() { keyboardKey = KeyCode.D, gamepadKey = KeyCode.Joystick1Button5 };

    public ActionBinding Attack = new ActionBinding() { keyboardKey = KeyCode.Space, gamepadKey = KeyCode.Joystick1Button0 };
}
public class KeyConfig_Tanabe : MonoBehaviour
{
    public enum InputType
    {
        Keyboard,
        Gamepad
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 今押された「任意のキー or パッドボタン」を調べる
    public KeyCode? GetAnyPressedKey()
    {
        // 1. キーボード／パッドの KeyCode 検索
        foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(code))
            {
                return code;
            }
        }

        return null;
    }

    // キーボード or パッドどっちが使われたか
    public bool GetAction(ActionBinding action)
    {
        if (action.currentType == InputType.Keyboard)
        {
            return Input.GetKey(action.keyboardKey);
        }
        else
        {
            return Input.GetKey(action.gamepadKey);
        }
    }
}
