using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageMover : MonoBehaviour
{
    private RectTransform rectTransform;
    private float moveSpeed = 200.0f; // �ő呬�x�i�s�N�Z��/�b�j
    private float bounceHeight = 50.0f; // ���˂鍂��
    private float bounceDuration = 0.5f; // ���˂鎞��
    private bool isMoving = false;

    // ������͈͂̐ݒ�
    private float minX = 209.9f;
    private float maxX = 269.9f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // �����ʒu����ʂ̉E�[�ɐݒ�
        rectTransform.anchoredPosition = new Vector2(239.9f, rectTransform.anchoredPosition.y);
    }

    private void Update()
    {
        // �X�y�[�X�L�[�������ꂽ�瓮���n�߂�
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            StartCoroutine(Move());
        }
    }

    private System.Collections.IEnumerator Move()
    {
        isMoving = true;

        // ���Ɉړ�
        float elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            float t = elapsedTime / bounceDuration;
            float xMovement = -moveSpeed * Time.deltaTime; // ���Ɉړ�
            float bounceOffset = Mathf.Sin(t * Mathf.PI) * bounceHeight; // X�����ɒ��˂铮��

            // ���݂̈ʒu���X�V�i�͈͓��ɂ��邩�`�F�b�N�j
            float newX = rectTransform.anchoredPosition.x + xMovement + bounceOffset;
            if (newX < minX)
            {
                newX = minX; // �ŏ��͈͂𒴂��Ȃ�
            }
            rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��~
        yield return new WaitForSeconds(0.5f); // �����ҋ@

        // �E�ɏ���������
        elapsedTime = 0f;

        while (elapsedTime < bounceDuration / 2)
        {
            float t = elapsedTime / (bounceDuration / 2);
            float xMovement = moveSpeed * Time.deltaTime; // �E�Ɉړ�
            float bounceOffset = Mathf.Sin(t * Mathf.PI) * (bounceHeight / 2); // ���������˂�

            // ���݂̈ʒu���X�V�i�͈͓��ɂ��邩�`�F�b�N�j
            float newX = rectTransform.anchoredPosition.x + xMovement + bounceOffset;
            if (newX > maxX)
            {
                newX = maxX; // �ő�͈͂𒴂��Ȃ�
            }
            rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��~
        yield return new WaitForSeconds(1.0f); // �ҋ@

        // ���ɖ߂�
        elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            float t = elapsedTime / bounceDuration;
            float xMovement = -moveSpeed * Time.deltaTime; // ���Ɉړ�
            float bounceOffset = Mathf.Sin(-t * Mathf.PI) * bounceHeight; // X�����ɒ��˂铮��

            // ���݂̈ʒu���X�V�i�͈͓��ɂ��邩�`�F�b�N�j
            float newX = rectTransform.anchoredPosition.x + xMovement + bounceOffset;
            if (newX < minX)
            {
                newX = minX; // �ŏ��͈͂𒴂��Ȃ�
            }
            rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��~
        isMoving = false; // ������I��
    }
}
