using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserNameInputManager : MonoBehaviour
{
    [Header("UI 参照")]
    [Tooltip("名前入力欄")]
    [SerializeField] private TMP_InputField nameInput;

    [Tooltip("仮ボタン")]
    [SerializeField] private Button _testButton;

    // プレイヤーネーム
    private string _playerName = "PlayerName";

    // Start is called before the first frame update
    void Start()
    {
        _testButton.onClick.AddListener(PushButton);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PushButton()
    {
        // 名前を取得
        _playerName = nameInput.text;

        Debug.Log(_playerName);
    }
}
