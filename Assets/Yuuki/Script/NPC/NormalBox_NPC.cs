using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NormalBox_NPC : NPCBase
{
    [Header("�U������p�R���C�_�[�iisTrigger�����j")]
    [SerializeField] private Collider m_attackCollider;
    [Header("�U���������ԁi�b�j")]
    [SerializeField] private float m_attackDuration = 0.5f;
    [Header("�U���Ԋu�i�b�j")]
    [SerializeField] private float m_attackCooldown = 1.0f;
    [Header("���G�ݒ�")]
    [SerializeField] private float m_detectRange = 10.0f;
    // ���G�͈�
    [SerializeField] private float m_attackRange = 2.0f;
    // �U������
    [Header("����|�C���g�ݒ�")]
    [SerializeField] private Transform[] m_waypoints;
    [SerializeField] private float m_patrolSpeed = 2.0f;

    [SerializeField] private float m_chaseSpeed = 3.5f;
    private int m_currentWaypoint = 0;
    // ���ݒǐՂ��Ă���v���C���[
    private bool m_isOnCooldown = false;
    // �U���N�[���^�C������
    public override void Start()
    {
        base.Start(); if (m_attackCollider != null) m_attackCollider.enabled = false;
    }
    public override void Update()
    {
        base.Update();
        if (m_isAttacking) return;
        // �U�����͈ړ���~
        if (m_target != null)
        {
            float dist = Vector3.Distance(transform.position, m_target.position);
            // ���G�͈͊O�Ȃ�^�[�Q�b�g����
            if (dist > m_detectRange * 1.5f) { m_target = null; return; }
            // �U���\�������Ȃ�ړ������U��
            if (dist <= m_attackRange)
            {
                if (!m_isOnCooldown) StartCoroutine(AttackCoroutine());
            }
            else
            {
                // �U���͈͊O�̎��̂ݒǐ�
                MoveTowards(m_target.position, m_chaseSpeed);
            }
        }
        else
        {
            // ���G���ă^�[�Q�b�g�T��
            FindTarget();
            // �^�[�Q�b�g���Ȃ���Ώ���
            if (m_target == null) Patrol();
        }
    }
    /// <summary> 
    /// /// �^�[�Q�b�g�T�� 
    /// /// </summary>
    private void FindTarget()
    {
        // �V�[�����̂��ׂĂ� CharacterBase ���擾
        CharacterBase[] characters = FindObjectsOfType<CharacterBase>();
        float nearest = Mathf.Infinity;
        Transform nearestHero = null;
        foreach (var c in characters)
        {
            // ����ENEMY_TYPE �̓X�L�b�v
            if (c == this || c.GetCharacterType() == CharacterBase.CharacterType.ENEMY_TYPE) continue;
            // Hero�����_��
            if (c.GetCharacterType() == CharacterBase.CharacterType.HERO_TYPE)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < m_detectRange && dist < nearest)
                {
                    nearest = dist; nearestHero = c.transform;
                }
            }
        }
        if (nearestHero != null)
        {
            m_target = nearestHero; Debug.Log($"{name} �� {m_target.name}�iHERO�j�𔭌��I");
        }
    }
    /// <summary> 
    /// /// ���񏈗� 
    /// /// </summary>
    private void Patrol()
    {
        if (m_waypoints == null || m_waypoints.Length == 0) return;
        Transform wp = m_waypoints[m_currentWaypoint];
        MoveTowards(wp.position, m_patrolSpeed);
        float dist = Vector3.Distance(transform.position, wp.position);
        if (dist < 1.0f)
        {
            // ���̏���|�C���g��
            m_currentWaypoint = (m_currentWaypoint + 1) % m_waypoints.Length;
        }
    }
    /// <summary> 
    /// /// �w������֌������Ĉړ�
    /// /// </summary>
    private void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        // �����␳
        if (dir.sqrMagnitude > 0.01f) transform.forward = dir;
    }
    /// <summary> 
    /// /// �U���A�N�V����
    /// /// </summary>
    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true; m_isOnCooldown = true;
        Debug.Log($"{name} ���U��������J�n�I");
        if (m_attackCollider != null) m_attackCollider.enabled = true;
        transform.Rotate(Vector3.right * 45f);
        yield return new WaitForSeconds(m_attackDuration);
        if (m_attackCollider != null) m_attackCollider.enabled = false;
        transform.Rotate(Vector3.left * 45f); m_isAttacking = false;
        // �U���N�[���^�C��
        yield return new WaitForSeconds(m_attackCooldown);
        m_isOnCooldown = false;
    }
    /// <summary>
    /// �U������ɑ��L�������������Ƃ� 
    /// /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!m_isAttacking) return;
        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;
        // ���w�c�͍U�����Ȃ�
        if (target.GetCharacterType() == CharacterBase.CharacterType.ENEMY_TYPE) return;
        target.Damage(GetPower());
        Debug.Log($"{name} �� {other.name} �� {GetPower()} �_���[�W�I �cHP:{target.GetHp()}");
    }
}