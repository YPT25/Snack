using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTest : MonoBehaviour
{
    [Header("���̃I�u�W�F�N�g���^����_���[�W��")]
    public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {

            // CharacterBase���p�������N���X���擾
            CharacterBaseY player = other.GetComponent<CharacterBaseY>();
            if (player != null)
            {
                // �_���[�W�������Ăяo��
                player.Damage(damage);
                Debug.Log($"[DamageTestObject] {other.name} �� {damage} �_���[�W��^�����I �cHP:{player.GetHp()}");
            }
        }
}
