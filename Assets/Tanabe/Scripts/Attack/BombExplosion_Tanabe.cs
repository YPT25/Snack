using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExplosion_Tanabe : MonoBehaviour
{
    [SerializeField] private float m_explosionRadius = 5f;   // 爆風の範囲
    [SerializeField] private float m_explosionForce = 5f;  // 爆風の強さ
    [SerializeField] private float m_upwardsModifier = 1f;   // 上方向の補正（持ち上がる感じ）

    private bool m_autoDestroy = false;
    private float m_destroyTimer = 2.0f;

    private Player_Tanabe m_player;

    private void Update()
    {
        if (!m_autoDestroy) { return; }
        m_destroyTimer -= Time.deltaTime;
        if(m_destroyTimer <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }

    public void Explode(bool _autoDestroy = false, float _destroyTimer = 2f, bool _isDamage = false, float _damage = 35f)
    {
        m_autoDestroy = _autoDestroy;
        m_destroyTimer = _destroyTimer;

        // 爆風の範囲内にあるコライダーを全部取る
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_explosionRadius);

        foreach (Collider hit in colliders)
        {
            if (hit.isTrigger) { continue; }

            if(_isDamage)
            {
                CharacterBase characterBase = hit.gameObject.GetComponent<CharacterBase>();
                if (characterBase != null)
                {
                    characterBase.Damage(_damage);
                }
            }

            this.ExplosionForce(hit);
        }
    }

    public void Explode(float _explosionRadius, float _explosionForce, float _upwardsModifier, bool _autoDestroy = false, float _destroyTimer = 2f, bool _isDamage = false, float _damage = 35f)
    {
        m_explosionRadius = _explosionRadius;
        m_explosionForce = _explosionForce;
        m_upwardsModifier = _upwardsModifier;

        this.Explode(_autoDestroy, _destroyTimer, _isDamage, _damage);
    }

    public void HammerExplode()
    {
        m_autoDestroy = true;
        m_destroyTimer = 2f;

        // 爆風の範囲内にあるコライダーを全部取る
        Collider[] colliders = Physics.OverlapSphere(transform.position, 15f);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null)
            {
                if (hit.gameObject.GetComponentInParent<Player_Tanabe>() != null)
                {
                    continue;
                }
                else
                {
                    // 爆風の力を加える
                    rb.AddExplosionForce(35f, transform.position, 15f, 6f, ForceMode.Impulse);
                }
            }
        }
    }

    private void ExplosionForce(Collider _hit)
    {
        Rigidbody rb = _hit.attachedRigidbody;
        if (rb != null)
        {
            if (_hit.gameObject.GetComponent<Player_Tanabe>() != null)
            {
                m_player = _hit.gameObject.GetComponent<Player_Tanabe>();
                m_player.SetIsHitBomb(true);
                // 爆風の力を加える
                rb.AddExplosionForce(m_explosionForce * 2.0f, transform.position, m_explosionRadius, m_upwardsModifier, ForceMode.Impulse);
            }
            else if (_hit.gameObject.GetComponentInParent<Player_Tanabe>() == null)
            {
                // 爆風の力を加える
                rb.AddExplosionForce(m_explosionForce * 5.0f, transform.position, m_explosionRadius, m_upwardsModifier, ForceMode.Impulse);
            }
        }
    }

    private void OnDestroy()
    {
        if (m_player != null)
        {
            m_player.SetIsHitBomb(false);
        }
    }

    // デバッグ用にギズモで範囲を可視化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_explosionRadius);
    }
}
