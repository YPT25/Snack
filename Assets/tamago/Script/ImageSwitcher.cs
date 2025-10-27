using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    public Image[] images; // 3��ImageUI���i�[����z��
    public Sprite newSprite; // �ύX��̃X�v���C�g
    private int currentIndex = 0; // ���݂̃C���f�b�N�X

    void Start()
    {
        // ������Ԃł́A���ׂĂ�ImageUI�����̃X�v���C�g�ɐݒ�
        foreach (var image in images)
        {
            image.sprite = image.sprite; // ���̃X�v���C�g��ݒ�
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ���݂�ImageUI��V�����X�v���C�g�ɐ؂�ւ�
            if (currentIndex < images.Length)
            {
                images[currentIndex].sprite = newSprite; // �V�����X�v���C�g�ɕύX
                currentIndex++; // �C���f�b�N�X�𑝉�
            }
        }
    }
}
