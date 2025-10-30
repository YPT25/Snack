using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// リスポーン時のキャラ選択UIを表示し、
/// 選ばれたモジュールIDをPlayerBaseに送る。
/// </summary>
public class RespawnManager : MonoBehaviour
{
    [Header("UI関連")]
    [SerializeField] private GameObject respawnUI;
    [SerializeField] private Button buttonPrefab;

    private MPlayerBase currentPlayer;

    public void ShowRespawnUI(MPlayerBase player)
    {
        currentPlayer = player;
        if (respawnUI == null) return;

        // 既存ボタン削除
        foreach (Transform t in respawnUI.transform)
            Destroy(t.gameObject);

        // 仮：3キャラ
        string[] names = { "NormalBox", "HeavyBox", "SpeedBox" };
        for (int i = 0; i < names.Length; i++)
        {
            var btn = Instantiate(buttonPrefab, respawnUI.transform);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = names[i];
            int id = i;
            btn.onClick.AddListener(() => OnCharacterSelected(id));
        }

        respawnUI.SetActive(true);
    }

    private void OnCharacterSelected(int id)
    {
        respawnUI.SetActive(false);
        if (currentPlayer != null)
        {
            currentPlayer.CmdChangeModule(id);
        }
    }
}