using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPlayerBase : EnemyBase
{
    [Header("�v���C���[���ʐݒ�")]
    [Header("�}�E�X���x")]
    [SerializeField] protected float mouseSensitivity = 3.0f;
    [Header("�v���C���[��]�̕�ԑ��x")]
    [SerializeField] protected float rotationSmooth = 10f;   

    protected Vector3 m_inputDir;
    protected Rigidbody m_rb;
    protected float yaw;   // ����]
    protected float pitch; // �c��]
    protected Camera cam;
    private TestRespawn respawnManager;

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();
        cam = Camera.main;

        // RespawnManager�������擾
        respawnManager = FindObjectOfType<TestRespawn>();
        if (respawnManager == null)
        {
            Debug.LogWarning("[MPlayerBase] RespawnManager���V�[���ɑ��݂��܂���B");
        }
    }

    public override void Update()
    {
        base.Update();
        HandleInput();
        HandleCamera();
        if (GetHp() <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ���͏����iWASD / Shift / �}�E�X�j
    /// </summary>
    protected virtual void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        m_inputDir = new Vector3(h, 0, v).normalized;

        if (Input.GetMouseButtonDown(0))
        {
            OnAttackInput();
        }
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// �J��������ƃv���C���[��]
    /// </summary>
    protected virtual void HandleCamera()
    {
        if (cam == null) return;

        // �}�E�X���͂���]�p�ɔ��f
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -30f, 60f); // �㉺�����i���D�݂Œ����j

        // �v���C���[�𒆐S�ɃI�t�Z�b�g�����J�����ʒu���v�Z
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 2f, -4f);
        cam.transform.position = transform.position + offset;

        // �J�������v���C���[�Ɍ�����
        cam.transform.LookAt(transform.position + Vector3.up * 1.5f);

        // �v���C���[��Y��]�̓J�����̐��������ɒǏ]������
        Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
    }


    /// <summary>
    /// �ړ������i�J������j
    /// </summary>
    protected virtual void Move()
    {
        if (m_rb == null) return;

        // �J������ňړ��������v�Z
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();
        Vector3 moveDir = (camForward * m_inputDir.z + camRight * m_inputDir.x).normalized;

        // �ʏ푬�x or �_�b�V�����x
        float speed = GetMoveSpeed();
        // �ړ��K�p
        Vector3 velocity = moveDir * speed;
        m_rb.velocity = new Vector3(velocity.x, m_rb.velocity.y, velocity.z);
    }

    /// <summary>
    /// �v���C���[�̎��S�����i��j��j
    /// </summary>
    public override void Die()
    {
        // ���ʂ̎��S�����iEnemyBase����Destroy���Ă΂��O�Ɂj
        Debug.Log($"{name} �����S���܂����B");

        // RespawnManager���Ă�
        if (respawnManager != null)
        {
            respawnManager.OnPlayerDeath();
        }
        else
        {
            Debug.LogWarning("[MPlayerBase] RespawnManager��������Ȃ��������߁A���X�|�[���ł��܂���B");
        }

        // Destroy�͂����ōŌ�ɌĂԁi�e��Die�ł�Destroy�������Ă���Ȃ�s�v�j
        base.Die();
    }

    /// <summary>
    /// �U�����͎��̏����i�h���N���X�ŏ㏑������j
    /// </summary>
    protected virtual void OnAttackInput()
    {
        Debug.Log($"{name} ���U������");
    }
}
