using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testTitlePlayerScript_Ashuri : MonoBehaviour
{
    [Header("ジャンプの強さ")]
    [Tooltip("この値を大きくすると高くジャンプします")]
    public float jumpForce = 5f;

    [Header("接地判定用")]
    [Tooltip("地面との判定に使うLayerを指定します")]
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private float checkRadius = 0.2f;

    [Tooltip("地面チェック用のTransformを指定します")]
    public Transform groundCheck;

    [Header("ジャンプタイミング")]
    [Tooltip("ジャンプする間隔（秒）")]
    [SerializeField] private float jumpTime = 2.0f;
    private float nowJumpTime = 0f;

    [Header("ターゲットポジション")]
    [Tooltip("移動先のポジション")]
    [SerializeField] private Transform targetPosition;

    [Header("移動速度")]
    [Tooltip("ジャンプ中に横方向に移動するスピード")]
    [SerializeField] private float moveSpeed = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 接地判定
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundLayer);

        // ジャンプタイミング計測
        nowJumpTime += Time.deltaTime;

        if (nowJumpTime >= jumpTime && isGrounded)
        {
            JumpTowardsTarget();
            nowJumpTime = 0f;
        }
    }

    void JumpTowardsTarget()
    {
        if (targetPosition == null) return;

        // 水平方向の移動ベクトル
        Vector3 horizontalDir = (targetPosition.position - transform.position);
        horizontalDir.y = 0; // 上方向は無視
        horizontalDir.Normalize();

        // 水平方向の速度を設定
        Vector3 horizontalVelocity = horizontalDir * moveSpeed;

        // 上方向のジャンプ力
        Vector3 jumpVelocity = Vector3.up * jumpForce;

        // Rigidbodyに一度で力を加える
        rb.velocity = jumpVelocity + horizontalVelocity;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (targetPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPosition.position);
        }
    }
}
