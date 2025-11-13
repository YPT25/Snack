using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// プレイヤー共通クラス（Mirror対応）
/// - すべてのプレイヤーキャラはこれを継承する
/// - HP管理、移動、攻撃、カメラ制御などを担当
/// - 死亡時にRespawnManagerを呼び出し、UIから再選択可能
/// </summary>
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

    private bool isInitialized = false;
    private bool isDead = false;

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();

        // ローカルプレイヤー専用の初期化
        if (isLocalPlayer)
        {
            cam = Camera.main;
            StartCoroutine(InitializeAfterDelay());
        }
    }

    private IEnumerator InitializeAfterDelay()
    {
        // RespawnManagerがSpawn後すぐには見つからない場合があるので少し待つ
        yield return new WaitForSeconds(0.3f);
        isInitialized = true;
        Debug.Log($"[MPlayerBase] {name} が初期化完了 (isLocalPlayer={isLocalPlayer})");
    }

    public override void Update()
    {
        base.Update();

        // 死亡中 or 未初期化 or 他人のプレイヤーなら処理しない
        if (!isLocalPlayer || !isInitialized || isDead)
            return;

        HandleInput();
        HandleCamera();

        if (GetHp() <= 0 && isServer)
            Die();
    }

    protected virtual void FixedUpdate()
    {
        if (!isLocalPlayer || !isInitialized || isDead) return;
        Move();
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

    protected virtual void HandleCamera()
    {
        if (cam == null) return;

        float mouseX = Input.GetAxis("Camera X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Camera Y") * mouseSensitivity;

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
        if (isDead) return;
        isDead = true;

        Debug.Log($"{name} が死亡しました。リスポーンUIを表示命令送信...");

        // 動きを止める
        RpcSetDeadState(true);

        // ✅ サーバー → クライアントへ「UIを出せ」と命令を送る
        TargetShowRespawnUI(connectionToClient);
    }

    [TargetRpc]
    private void TargetShowRespawnUI(NetworkConnection target)
    {
        Debug.Log("TargetShowRespawnUI：クライアントで呼び出し成功！");

        // ✅ クライアントで実際にUIを操作する
        if (RespawnManager.Instance != null)
        {
            Debug.Log("RespawnManager.Instance 発見、UI表示処理へ");
            RespawnManager.Instance.ShowRespawnUI();
        }
        else
        {
            Debug.LogWarning("RespawnManager.Instance が見つかりません。シーンにRespawnManagerPrefabがありますか？");
        }
    }

    [ClientRpc]
    protected void RpcSetDeadState(bool value)
    {
        if (value)
        {
            // 死亡時に動きを止める
            if (m_rb != null)
                m_rb.velocity = Vector3.zero;
        }
    }

    protected virtual void OnAttackInput()
    {
        Debug.Log($"{name} が攻撃入力");
    }

    public Sprite GetRespawnIcon() => m_respawnIcon;
}