using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer1 : MonoBehaviour
{
    public Image minutesImage; // ���̕\���pImage�i1�̈ʁj
    public Image secondsImage; // �b�̏\�̈ʗpImage
    public Image secondsOnesImage; // �b�̈�̈ʗpImage
    public float countdownTime = 180f; // 3���i180�b�j
    private float timeRemaining;

    // �X�v���C�g�V�[�g�̃X�v���C�g���i�[����z��
    public Sprite[] numberSprites; // 0����9�̃X�v���C�g

    void Start()
    {
        timeRemaining = countdownTime;
        UpdateTimerDisplay();
    }

    void Update()
    {
        // �^�C�}�[�̃J�E���g�_�E��
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // �^�C�}�[��0�����ɂȂ����ꍇ��0�ɃN���b�v
            if (timeRemaining < 0)
            {
                timeRemaining = 0;
            }

            UpdateTimerDisplay();
        }
    }

    // �^�C�}�[�̕\�����X�V���郁�\�b�h
    private void UpdateTimerDisplay()
    {
        // �c�莞�Ԃ𕪂ƕb�ɕ���
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // ���̈�̈ʂ��擾
        int onesMinutes = minutes % 10;  // ��̈�

        // ������\�����邽�߂̃X�v���C�g���擾
        int tensSeconds = seconds / 10;   // �b�̏\�̈�
        int onesSeconds = seconds % 10;    // �b�̈�̈�

        // �X�v���C�g��ݒ�
        minutesImage.sprite = numberSprites[onesMinutes]; // ���̈�̈�
        secondsImage.sprite = numberSprites[tensSeconds]; // �b�̏\�̈�
        secondsOnesImage.sprite = numberSprites[onesSeconds]; // �b�̈�̈�

        // �ύX: �c�莞�Ԃ�30�b�ȉ��̂Ƃ��ɉ摜��ԂɕύX
        if (timeRemaining <= 30f) // �ύX: �c�莞�Ԃ�30�b�ȉ��̏ꍇ
        {
            minutesImage.color = Color.red; // ���̕\���pImage��ԂɕύX
            secondsImage.color = Color.red; // �b�̏\�̈ʗpImage��ԂɕύX
            secondsOnesImage.color = Color.red; // �b�̈�̈ʗpImage��ԂɕύX
        }
        else
        {
            // �c�莞�Ԃ�30�b��葽���ꍇ�͌��̐F�ɖ߂�
            minutesImage.color = Color.white; // ���̐F
            secondsImage.color = Color.white; // ���̐F
            secondsOnesImage.color = Color.white; // ���̐F
        }
    }
}
