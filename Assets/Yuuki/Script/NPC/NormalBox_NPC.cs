using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBox_NPC : NPCBase
{
    [Header("�U������p�R���C�_�[�iisTrigger�����j")]
    [SerializeField] private Collider m_attackCollider; // �U������Ɏg���R���C�_�[

    [Header("�U���������ԁi�b�j")]
    [SerializeField] private float m_attackDuration = 0.5f; // �U������������
    private bool m_isAttacking = false; // ���ݍU�������ǂ���

    public override void Start()
    {
        base.Start();
        //SetEnemyType(EnemyType.TYPE_A);
        // �U������͒ʏ�͖�����
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }
    }

    public override void Update()
    {
        base.Update();

        // �U���\�Ȃ�U�������݂�
        TryAttack();
    }

    /// <summary>
    /// �^�[�Q�b�g���U���\�����ɂ��邩�m�F���A�U�����J�n����
    /// </summary>
    protected override void TryAttack()
    {
        if (m_isAttacking) return; // �U�����Ȃ�X�L�b�v
        if (m_target == null) return; // �^�[�Q�b�g�����Ȃ��Ȃ�X�L�b�v

        float dist = Vector3.Distance(transform.position, m_target.position);
        if (dist < 2.0f) // ���̍U������
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    /// <summary>
    /// NPC�̍U���R���[�`��
    /// �E�O�ɓ|��铮��
    /// �E�U������ON/OFF
    /// �E�I����ɏ�Ԃ�߂�
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} ���O���ɓ|��čU���I�iNPC�j");

        // �i�ȈՁj�|��铮��
        transform.Rotate(Vector3.right * 90f);

        // �U������ON
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = true;
        }

        yield return new WaitForSeconds(m_attackDuration);

        // �U������OFF
        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }

        // ���̊p�x�ɖ߂�
        transform.Rotate(Vector3.left * 90f);

        m_isAttacking = false;
    }

    /// <summary>
    /// �U������ɑ��L���������������̏���
    /// �E�U�����̂ݗL��
    /// �E�������g�͑ΏۊO
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null && target != this)
        {
            Attack(target);
        }
    }
}
