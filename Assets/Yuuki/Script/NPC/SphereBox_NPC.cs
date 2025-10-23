using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SphereBox �� NPC�iAI����L�����j�p�N���X
/// �ENPCBase ���p��
/// �E�^�[�Q�b�g���߂Â�����u�O���ɓ]�����ēˌ��v�U�����s��
/// </summary>
public class SphereBox_NPC : NPCBase
{
    // �U������p�R���C�_�[
    [Header("�U������p�R���C�_�[�iisTrigger�����j")]
    [SerializeField] private Collider m_attackCollider;

    [Header("�ˌ��̎������ԁi�b�j")]
    [SerializeField] private float m_rollDuration = 1.0f;

    [Header("�ˌ��̗�")]
    [SerializeField] private float m_rollForce = 15f;

    private bool m_isAttacking = false;

    public override void Start()
    {
        base.Start();

        if (m_attackCollider != null)
        {
            m_attackCollider.enabled = false;
        }
    }

    public override void Update()
    {
        base.Update();

        // �U�������݂�iNPCBase��TryAttack���I�[�o�[���C�h�j
        TryAttack();
    }

    /// <summary>
    /// NPC�̍U�����菈��
    /// �^�[�Q�b�g����苗���ȓ��Ȃ�ˌ��J�n
    /// </summary>
    protected override void TryAttack()
    {
        if (m_isAttacking) return;
        if (m_target == null) return;

        float dist = Vector3.Distance(transform.position, m_target.position);
        // SphereBox�͂��L�߂̋�������ˌ�
        if (dist < 3.0f)
        {
            StartCoroutine(RollAttackCoroutine());
        }
    }

    /// <summary>
    /// �ˌ��U���̃R���[�`��
    /// �E�O���ɗ͂�������
    /// �E�U������ON/OFF
    /// </summary>
    private IEnumerator RollAttackCoroutine()
    {
        m_isAttacking = true;

        Debug.Log($"{name} ���]�����ēˌ��I�iNPC�j");

        if (m_attackCollider != null) m_attackCollider.enabled = true;

        if (m_rb != null)
        {
            m_rb.AddForce(transform.forward * m_rollForce, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(m_rollDuration);

        if (m_attackCollider != null) m_attackCollider.enabled = false;

        m_isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;

        CharacterBaseY target = other.GetComponent<CharacterBaseY>();
        if (target != null && target != this)
        {
            Attack(target);
        }
    }
}
