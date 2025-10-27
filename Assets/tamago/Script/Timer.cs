using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Image minutesOnesImage; // ���̈�̈ʗpImage
    public Image secondsImage; // �b�̏\�̈ʗpImage
    public Image secondsOnesImage; // �b�̈�̈ʗpImage
    public Image pictureImage;
    public Image warningImage1; // �x���p��Image�i0:30����\���j
    public Image warningImage2;
    public Image warningImage3;
    public Image pictureImage2;
    public float countdownTime = 180f; // 3���i180�b�j
    private float timeRemaining;

    public float blinkDuration = 0.2f; // �_�ł̊Ԋu

    // �X�v���C�g�V�[�g�̃X�v���C�g���i�[����z��
    public Sprite[] numberSprites; // 0����9�̃X�v���C�g
    public Sprite[] numberSprites1; // �x���p�̃X�v���C�g�i�قȂ�F�j

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

            // �^�C�}�[�̕\�����X�V
            if (timeRemaining <= 30f)
            {
                UpdateTimerDisplay1();

            }
            else
            {
                UpdateTimerDisplay();
            }
        }

        // �x���摜�̕\��
        if (timeRemaining <= 30f)
        {
            warningImage1.gameObject.SetActive(true); // �x���摜��\��
            warningImage2.gameObject.SetActive(true);
            warningImage3.gameObject.SetActive(true);
            pictureImage2.gameObject.SetActive(true);
            minutesOnesImage.gameObject.SetActive(false);
            secondsImage.gameObject.SetActive(false);
            secondsOnesImage.gameObject.SetActive(false);
            pictureImage.gameObject.SetActive(false);

        }
        else
        {
            // �x���摜���\��
            warningImage1.gameObject.SetActive(false);
            warningImage2.gameObject.SetActive(false);
            warningImage3.gameObject.SetActive(false);
            pictureImage2.gameObject.SetActive (false);
        }
    }

    // �^�C�}�[�̕\�����X�V���郁�\�b�h
    private void UpdateTimerDisplay()
    {
        // �c�莞�Ԃ𕪂ƕb�ɕ���
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // ���̈�̈ʂ��擾
        int onesMinutes = minutes % 10; // ��̈�

        // ������\�����邽�߂̃X�v���C�g���擾
        int tensSeconds = seconds / 10; // �b�̏\�̈�
        int onesSeconds = seconds % 10;  // �b�̈�̈�

        // �X�v���C�g��ݒ�
        minutesOnesImage.sprite = numberSprites[onesMinutes]; // ���̈�̈�
        secondsImage.sprite = numberSprites[tensSeconds]; // �b�̏\�̈�
        secondsOnesImage.sprite = numberSprites[onesSeconds]; // �b�̈�̈�
    }

    private void UpdateTimerDisplay1()
    {
        // �c�莞�Ԃ𕪂ƕb�ɕ���
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // ���̈�̈ʂ��擾
        int onesMinutes = minutes % 10; // ��̈�

        // ������\�����邽�߂̃X�v���C�g���擾
        int tensSeconds = seconds / 10; // �b�̏\�̈�
        int onesSeconds = seconds % 10;  // �b�̈�̈�

        // �X�v���C�g��ݒ�
        warningImage1.sprite = numberSprites1[onesMinutes]; // ���̈�̈�
        warningImage2.sprite = numberSprites1[tensSeconds]; // �b�̏\�̈�
        warningImage3.sprite = numberSprites1[onesSeconds]; // �b�̈�̈�
    }
}