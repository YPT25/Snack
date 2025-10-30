using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro���g�p���邽�߂ɕK�v

/// <summary>
/// �X�R�A�̕\���Ɖ��Z���Ǘ�����N���X�B
/// O�L�[�������ƃX�R�A��������B
/// </summary>
public class SweetScore : MonoBehaviour
{
    [Header("�X�R�A�\���p��TextMeshPro")]
    [Tooltip("�X�R�A��\������TextMeshProUGUI�R���|�[�l���g���w�肵�Ă��������B")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("�X�R�A�ݒ�")]
    [Tooltip("���݂̃X�R�A�B�Q�[���J�n����0�Ƀ��Z�b�g�����B")]
    [SerializeField] private int currentScore = 0;

    void Start()
    {
        // �����X�R�A��\���ɔ��f
        UpdateScoreDisplay();
    }

    void Update()
    {
        // O�L�[���������Ƃ��ɃX�R�A�����Z
        if (Input.GetKeyDown(KeyCode.O))
        {
            AddScore(10); // ��F10�_�ǉ�
        }
    }

    /// <summary>
    /// �X�R�A�����Z����֐��B
    /// </summary>
    /// <param name="amount">���Z����X�R�A�̒l�B</param>
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// �X�R�A����������֐��B
    /// </summary>
    /// <param name="amount">��������X�R�A�̒l�B</param>
    public void SubtractScore(int amount)
    {
        currentScore -= amount;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// TextMeshPro�ɃX�R�A�𔽉f����B
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
        else
        {
            Debug.LogWarning("Score TextMeshPro���A�T�C������Ă��܂���B");
        }
    }
}
