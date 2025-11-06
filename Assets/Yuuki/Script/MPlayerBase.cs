using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MPlayerBase : EnemyBase
{
    [Header("プレイヤー共通設定")]
    [SerializeField] protected float mouseSensitivity = 3.0f;
    [SerializeField] protected float rotationSmooth = 10f;
    [SerializeField] private Sprite m_respawnIcon;

    protected Vector3 m_inputDir;
    protected Rigidbody m_rb;
    protected float yaw;
    protected float pitch;
    protected Camera cam;
    private RespawnManager respawnManager;

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();

        if (isLocalPlayer)
        {
            cam = Camera.main;
            respawnManager = FindObjectOfType<RespawnManager>();
        }
    }

    public override void Update()
    {
        base.Update();
        if (!isLocalPlayer) return;

        HandleInput();
        HandleCamera();

        if (GetHp() <= 0 && isServer)
            Die();
    }

    protected virtual void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        m_inputDir = new Vector3(h, 0, v).normalized;

        if (Input.GetMouseButtonDown(0))
            CmdAttackInput();
    }

    [Command]
    private void CmdAttackInput()
    {
        OnAttackInput();
    }

    protected virtual void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        Move();
    }

    protected virtual void HandleCamera()
    {
        if (cam == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 2f, -4f);
        cam.transform.position = transform.position + offset;
        cam.transform.LookAt(transform.position + Vector3.up * 1.5f);

        Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
    }

    protected virtual void Move()
    {
        if (m_rb == null || cam == null) return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();
        Vector3 moveDir = (camForward * m_inputDir.z + camRight * m_inputDir.x).normalized;

        float speed = GetMoveSpeed();
        Vector3 velocity = moveDir * speed;
        m_rb.velocity = new Vector3(velocity.x, m_rb.velocity.y, velocity.z);
    }

    [Server]
    public override void Die()
    {
        Debug.Log($"{name} が死亡しました。");
        TargetShowRespawnUI(connectionToClient);
        base.Die();
    }

    [TargetRpc]
    private void TargetShowRespawnUI(NetworkConnection target)
    {
        if (respawnManager != null)
            respawnManager.OnPlayerDeath();
    }

    protected virtual void OnAttackInput()
    {
        Debug.Log($"{name} が攻撃入力");
    }

    public Sprite GetRespawnIcon() => m_respawnIcon;
}