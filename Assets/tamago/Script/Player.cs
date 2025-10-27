using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // �v���C���[�̈ړ����x

    void Update()
    {
        // ���͂��擾
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; // A/D �܂��� ��/�E���L�[
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime; // W/S �܂��� ��/�����L�[

        // �v���C���[�̈ʒu���X�V
        transform.Translate(moveX, 0, moveZ);
    }
}
