using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NormalBox �̃v���C���[����p�N���X
/// �EMPlayerBase ���p��
/// �E���N���b�N�Łu�O���ɓ|���U���v�����s
/// �E�U�����̂ݍU������p�R���C�_�[��L��������
/// </summary>
public class NormalBox_Player : MPlayerBase
{
    // �U������Ɏg���R���C�_�[�iInspector����ݒ�j
    [Header("�U������p�R���C�_�[�iisTrigger�����j")]
    [SerializeField] private Collider m_attackCollider;
    // �U�����肪�L���Ȏ���
    [Header("�U���������ԁi�b�j")]
    [SerializeField] private float m_attackDuration = 0.5f;
    // �U�������ǂ������Ǘ�
    private bool m_isAttacking = false; 

    public override void Start()
    {
        base.Start();
        SetEnemyType(EnemyType.TYPE_A);
        //�I�u�W�F�N�g�̐F��ԂɕύX����
        GetComponent<Renderer>().material.color = Color.red;
        // �U������͒ʏ�͖��������Ă���
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }

    }

    /// <summary>
    /// �v���C���[�̍U�����́i���N���b�N�j���ɌĂ΂��
    /// </summary>
    protected override void OnAttackInput()
    {
        // �U�����łȂ���΍U�����J�n
        if (!m_isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    /// <summary>
    /// �U����������莞�ԍs���R���[�`��
    /// �E�O�ɓ|��铮��
    /// �E�U������ON/OFF
    /// �E�I����ɏ�Ԃ�߂�
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} ���O���ɓ|��čU���I");

        // �O�ɓ|���A�j���[�V�����i���j
        transform.Rotate(Vector3.right * 30f);

        // �U�������L����
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = true;
        }

        // �U���������Ԃ����҂�
        yield return new WaitForSeconds(m_attackDuration);

        // �U������𖳌���
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }

        // ���̊p�x�ɖ߂�
        transform.Rotate(Vector3.left * 30f);

        m_isAttacking = false;
    }

    /// <summary>
    /// �U������R���C�_�[�ɒN�������������̏���
    /// �E�U�����̂ݗL��
    /// �E�������g�͑ΏۊO
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
        {
            // EnemyBase �� Attack ���Ăԁi�_���[�W�����͋��ʁj
            Attack(target);
        }
    }
}
