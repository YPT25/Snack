using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// CannonBox NPC�iAI����t���j
/// �^�[�Q�b�g�Ɍ����ĉ������U�����s��
/// </summary>
public class CannonBox_NPC : NPCBase
{
    [Header("�e�ۃv���n�u�iRigidbody�t���j")]
    [SerializeField] private GameObject m_cannonBallPrefab;

    [Header("���ˈʒu�i�C�� Transform�j")]
    [SerializeField] private Transform m_firePoint;

    [Header("�˒�����")]
    [SerializeField] private float m_attackRange = 10f;

    [Header("�e��")]
    [SerializeField] private float m_shotSpeed = 15f;

    [Header("�U���Ԋu�i�b�j")]
    [SerializeField] private float m_attackCooldown = 2.0f;

    private bool m_isOnCooldown = false;

    public override void Update()
    {
        base.Update();

        if (m_target == null) return;

        // �U���\�����ɓ�������ˌ�
        float dist = Vector3.Distance(transform.position, m_target.position);
        if (!m_isOnCooldown && dist <= m_attackRange)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    /// <summary>
    /// �U���R���[�`���i�������ˌ��j
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;
        m_isOnCooldown = true;

        // �^�[�Q�b�g����������
        Vector3 lookPos = m_target.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // ���ˏ���
        FireProjectile();

        yield return new WaitForSeconds(m_attackCooldown);

        m_isAttacking = false;
        m_isOnCooldown = false;
    }

    /// <summary>
    /// ���ۂ̒e�����Ɣ���
    /// </summary>
    private void FireProjectile()
    {
        if (m_cannonBallPrefab == null || m_firePoint == null)
        {
            Debug.LogWarning($"{name}�F�e�ۂ܂��͔��ˈʒu���ݒ肳��Ă��܂���B");
            return;
        }

        GameObject ball = Instantiate(m_cannonBallPrefab, m_firePoint.position, m_firePoint.rotation);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb != null && m_target != null)
        {
            Vector3 dir = (m_target.position - m_firePoint.position).normalized;
            rb.velocity = dir * m_shotSpeed;
        }

        Debug.Log($"{name} ���C�e�𔭎ˁI");
    }
}
