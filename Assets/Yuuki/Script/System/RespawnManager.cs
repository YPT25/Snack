using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ���X�|�[�����̃L�����I��UI��\�����A
/// �I�΂ꂽ���W���[��ID��PlayerBase�ɑ���B
/// </summary>
public class RespawnManager : MonoBehaviour
{
    [Header("UI�֘A")]
    [SerializeField] private GameObject respawnUI;
    [SerializeField] private Button buttonPrefab;

    private MPlayerBase currentPlayer;

    public void ShowRespawnUI(MPlayerBase player)
    {
        currentPlayer = player;
        if (respawnUI == null) return;

        // �����{�^���폜
        foreach (Transform t in respawnUI.transform)
            Destroy(t.gameObject);

        // ���F3�L����
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