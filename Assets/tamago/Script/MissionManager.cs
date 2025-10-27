using System.Collections;
using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public TMP_Text missionText; // �~�b�V�����̕\���pText
    public string[] missions; // �~�b�V�����̔z��
    public float missionDuration = 60f; // �~�b�V�����̎��ԁi1���j
    private float timeRemaining;
    private bool missionActive = false; // �~�b�V�������A�N�e�B�u���ǂ���
    private int currentMissionIndex = 0; // ���݂̃~�b�V�����C���f�b�N�X
    private bool spacePressed = false; // �X�y�[�X�L�[�������ꂽ���ǂ���

    void Start()
    {
        StartNextMission();
    }

    void Update()
    {
        // �X�y�[�X�L�[�������ꂽ�ꍇ�Ƀt���O�𗧂Ă�
        if (Input.GetKeyDown(KeyCode.Space) && missionActive)
        {
            spacePressed = true; // �X�y�[�X�L�[�������ꂽ
        }
    }

    void StartNextMission()
    {
        if (currentMissionIndex < missions.Length)
        {
            missionText.text = missions[currentMissionIndex];
            timeRemaining = missionDuration;
            missionActive = true; // �~�b�V�������A�N�e�B�u�ɂ���
            spacePressed = false; // �X�y�[�X�L�[�̉����t���O�����Z�b�g
            StartCoroutine(MissionCycle()); // �R���[�`�����J�n
        }
        else
        {
            missionText.text = "All missions completed!";
        }
    }

    IEnumerator MissionCycle()
    {
        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            timeRemaining--;
        }

        // 1���o�ߌ�ɃX�y�[�X�������ꂽ���m�F
        if (spacePressed)
        {
            // �X�y�[�X��������Ă����ꍇ�A���̃~�b�V�����ɐi��
            currentMissionIndex++;
            StartNextMission(); // ���̃~�b�V�������J�n
        }
        else
        {
            // �X�y�[�X��������Ă��Ȃ������ꍇ�A�Q�[���I�[�o�[
            GameOver();
        }
    }

    void GameOver()
    {
        missionText.text = "Game Over!";
        // �Q�[���I�[�o�[�����������ɒǉ�
        Debug.Log("Game Over!"); // �f�o�b�O���b�Z�[�W
    }
}