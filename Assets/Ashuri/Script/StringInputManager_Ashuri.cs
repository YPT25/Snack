using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StringInputManager_Ashuri : MonoBehaviour
{
    [Header("文字")]
    [Tooltip("英語のボタン")]
    [SerializeField] private List<Button> englishButtons = new List<Button>();

    [Header("入力欄")]
    [Tooltip("文字を表示する TMP_InputField")]
    [SerializeField] private TMP_InputField inputField;

    // 最大入力文字数
    [Header("設定")]
    [Tooltip("入力できる最大文字数")]
    [SerializeField] private int maxCharacterCount = 3;

    // 起動時に各ボタンにイベントを登録する
    private void Start()
    {
        // すべてのボタンにクリックイベントを設定
        for (int i = 0; i < englishButtons.Count; i++)
        {
            int index = i; // ループ変数をキャプチャ
            englishButtons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    // ボタンが押されたときの処理
    private void OnButtonClicked(int index)
    {
        // もし26番目のボタンが押されたら、一文字削除
        if (index == 26)
        {
            // 現在の文字を取得
            string currentText = inputField.text;

            // 文字が1文字以上ある場合のみ削除
            if (currentText.Length > 0)
            {
                // 最後の1文字を削除して再設定
                inputField.text = currentText.Substring(0, currentText.Length - 1);
            }

            // 処理を終了
            return;
        }

        // 現在の入力内容を取得
        string text = inputField.text;

        // 最大文字数を超えていたら追加しない
        if (text.Length >= maxCharacterCount)
            return;

        // ボタンのテキスト（例："A"など）を取得
        string buttonText = englishButtons[index].GetComponentInChildren<TMP_Text>().text;

        // 入力欄に文字を追加
        inputField.text = text + buttonText;
    }
}
