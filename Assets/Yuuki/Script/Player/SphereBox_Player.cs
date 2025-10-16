using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SphereBox �̃v���C���[����p�N���X
/// �EMPlayerBase ���p��
/// �E���N���b�N�Łu�O���ɓ]�����ēˌ�����v�U�������s
/// �E�U������ Rigidbody �ɗ͂������Ĉړ����A�U������R���C�_�[��ON�ɂ���
/// </summary>
public class SphereBox_Player : MPlayerBase
{
    // �U������R���C�_
    [Header("�U������p�R���C�_�[�iisTrigger�����j")]
    [SerializeField] private Collider m_attackCollider;
    // �ˌ����鎞��
    [Header("�ˌ��̎������ԁi�b�j")]
    [SerializeField] private float m_rollDuration = 1.0f;
    // �ˌ����ɗ^����O���ւ̗�
    [Header("�ˌ��̗�")]
    [SerializeField] private float m_rollForce = 15f;
    // �U�����t���O
    private bool m_isAttacking = false; 

    public override void Start()
    {
        base.Start();
        //�I�u�W�F�N�g�̐F��ԂɕύX����
        GetComponent<Renderer>().material.color = Color.blue;
        // �U������͏�����Ԃ�OFF
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }
    }

    /// <summary>
    /// �v���C���[�U�����́i���N���b�N�j���̏���
    /// </summary>
    protected override void OnAttackInput()
    {
        if (!m_isAttacking)
        {
            StartCoroutine(RollAttackCoroutine());
        }
    }

    /// <summary>
    /// �]�����ēˌ�����U���̃R���[�`��
    /// �E�O���ɗ͂�������
    /// �E�U������ON/OFF
    /// </summary>
    private IEnumerator RollAttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} ���]�����ēˌ��I�iPlayer�j");

        // �U������ON
        if (m_attackCollider != null) m_attackCollider.enabled = true;

        // Rigidbody �ɑO���֗͂�������
        if (m_rb != null)
        {
            m_rb.AddForce(transform.forward * m_rollForce, ForceMode.Impulse);
        }

        // ��莞�ԓˌ�
        yield return new WaitForSeconds(m_rollDuration);

        // �U������OFF
        if (m_attackCollider != null) m_attackCollider.enabled = false;

        m_isAttacking = false;
    }

    /// <summary>
    /// �U������ɑ��肪���������̏���
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
        {
            // EnemyBase��Attack�𗘗p
            Attack(target); 
        }
    }
}
