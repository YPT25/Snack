using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    // �ő�HP
    public float maxHP = 100f;
    // ���݂�HP
    public float currentHP;
    // ��񂠂���̌�����
    public float decreaseAmount = 20f;
    // �����ɂ����鎞��
    public float decreaseDuration = 1f; // 1�b�Ō���
    // �����ʒu�iHP�����^���̂Ƃ��̈ʒu�j
    public Vector2 initialPosition;

    // HP�o�[��RectTransform
    private RectTransform rectTransform;

    void Start()
    {
        // ���݂�HP���ő�HP�ŏ�����
        currentHP = maxHP;
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition; // �����ʒu��ۑ�
        UpdateHPBar();
    }

    void Update()
    {
        // �X�y�[�X�L�[�������ꂽ���m�F
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DecreaseHPOverTime(decreaseAmount, decreaseDuration));
        }
    }

    // HP������������������R���[�`��
    private IEnumerator DecreaseHPOverTime(float amount, float duration)
    {
        float startHP = currentHP;
        float targetHP = Mathf.Max(0, currentHP - amount); // HP��0�����ɂȂ�Ȃ��悤�ɂ���

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // �o�ߎ��Ԃ��X�V
            currentHP = Mathf.Lerp(startHP, targetHP, elapsed / duration); // ���`��Ԃ�HP������������
            UpdateHPBar(); // HP�o�[���X�V
            yield return null; // ���̃t���[���܂őҋ@
        }

        currentHP = targetHP; // �ŏI�I��HP��ݒ�
        UpdateHPBar(); // �ŏI�I��HP�o�[���X�V
    }

    // HP�o�[�̕\�����X�V���郁�\�b�h
    private void UpdateHPBar()
    {
        // ���݂�HP�Ɋ�Â��ă|�W�V�������X�V
        float normalizedHP = currentHP / maxHP; // 0����1�͈̔͂ɐ��K��
        rectTransform.anchoredPosition = new Vector2(initialPosition.x - (1 - normalizedHP) * 100, initialPosition.y); // ���ɉ����ĉ��ʒu�𒲐�
    }
}
