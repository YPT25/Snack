using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NormalBoxタイプのプレイヤー挙動。
/// 左クリックで攻撃。WASDで移動。
/// </summary>
public class NormalBox_Module : PlayerModule
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private Collider attackCollider;
    private bool isAttacking = false;

    public override void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
            owner.StartCoroutine(AttackCoroutine());
    }

    public override void HandleMovement()
    {
        if (isAttacking) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v).normalized * moveSpeed;

        if (rb != null)
        {
            rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        if (attackCollider) attackCollider.enabled = true;

        transform.Rotate(Vector3.right * 30f);
        yield return new WaitForSeconds(attackDuration);
        transform.Rotate(Vector3.left * 30f);

        if (attackCollider) attackCollider.enabled = false;
        isAttacking = false;
    }
}