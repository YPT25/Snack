using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CannonBox �v���C���[�i�C���U�������j
/// �U�����ɒe��Prefab�𐶐����đO���ɔ�΂�
/// </summary
public class CannonBox_Player : MPlayerBase
{
    [Header("�e�ۃv���n�u")]
    public GameObject cannonBallPrefab;

    [Header("�e�۔��ˈʒu")]
    public Transform firePoint;
    [Header("�e�۔��ˑ��x")]
    public float fireSpeed;

    protected override void OnAttackInput()
    {
        Attack(null); // �ߐڑΏۂ͕s�v
    }

    public override void Attack(CharacterBaseY target)
    {
        if (cannonBallPrefab == null || firePoint == null) return;

        GameObject ball = Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * fireSpeed; // ���ˑ��x
        }

        Debug.Log($"{name} ���C���𔭎ˁI");
    }
}
