using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkImage : MonoBehaviour
{
    public Image targetImage; // �_�ł�����Image
    public float blinkDuration = 0.2f; // �_�ł̊Ԋu
    private Color originalColor;

    void Start()
    {
        // ���̐F��ۑ�
        originalColor = targetImage.color;
    }

    void Update()
    {
        // �X�y�[�X�L�[�������ꂽ�Ƃ��ɓ_�ł��J�n
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Blink());
        }
    }

    private IEnumerator Blink()
    {
        for (int i = 0; i < 2; i++) // 2��_��
        {
            // �A���t�@�l��0�ɂ���
            Color newColor = originalColor;
            newColor.a = 0.5f; // �����ɂ���
            targetImage.color = newColor;

            yield return new WaitForSeconds(blinkDuration); // �w�肵�����ԑҋ@

            // ���̐F�ɖ߂�
            targetImage.color = originalColor;

            yield return new WaitForSeconds(blinkDuration); // �w�肵�����ԑҋ@
        }
    }
}
