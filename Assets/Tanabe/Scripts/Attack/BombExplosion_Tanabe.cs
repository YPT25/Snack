using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BombExplosion_Tanabe : NetworkBehaviour
{
    [SerializeField] private float m_explosionRadius = 5f;   // 爆風の範囲
    [SerializeField] private float m_explosionForce = 5f;  // 爆風の強さ
    [SerializeField] private float m_upwardsModifier = 1f;   // 上方向の補正（持ち上がる感じ）

    private bool m_autoDestroy = false;
    private float m_destroyTimer = 2.0f;

    private List<Player_Tanabe> m_players = new List<Player_Tanabe>();

    [ServerCallback]
    private void Update()
    {
        if (!m_autoDestroy) { return; }
        m_destroyTimer -= Time.deltaTime;
        if(m_destroyTimer <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }

    [ClientRpc]
    private void RpcSetPlayersIsMove(Player_Tanabe _player, bool _flag)
    {
        if (_player != null)
        {
            _player.SetIsHitBomb(_flag);
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
                    characterBase.RpcDamage(_damage);
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

    [ServerCallback]
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
                    //rb.AddExplosionForce(35f, transform.position, 15f, 6f, ForceMode.Impulse);
                    this.RpcAddExplosionForce(rb.gameObject, 35f, transform.position, 15f, 6f, ForceMode.Impulse);
                }
            }
        }
    }

    [ServerCallback]
    private void ExplosionForce(Collider _hit)
    {
        Rigidbody rb = _hit.attachedRigidbody;
        if (rb != null)
        {
            if (_hit.gameObject.GetComponent<Player_Tanabe>() != null)
            {
                Player_Tanabe player = _hit.gameObject.GetComponent<Player_Tanabe>();
                player.SetIsHitBomb(true);
                m_players.Add(player);
                //this.RpcSetPlayersIsMove(m_player, true);
                // 爆風の力を加える
                //rb.AddExplosionForce(m_explosionForce * 2.0f, transform.position, m_explosionRadius, m_upwardsModifier, ForceMode.Impulse);
                this.RpcAddExplosionForce(rb.gameObject, m_explosionForce * 2.0f, transform.position, m_explosionRadius, m_upwardsModifier, ForceMode.Impulse);
            }
            else if (_hit.gameObject.GetComponentInParent<Player_Tanabe>() == null)
            {
                // 爆風の力を加える
                //rb.AddExplosionForce(m_explosionForce * 5.0f, transform.position, m_explosionRadius, m_upwardsModifier, ForceMode.Impulse);
                this.RpcAddExplosionForce(rb.gameObject, m_explosionForce * 5.0f, transform.position, m_explosionRadius, m_upwardsModifier, ForceMode.Impulse);
            }
        }
    }

    [Command]
    private void CmdAddExplosionForce(GameObject _gameObject, float _force, Vector3 _position, float _radius, float _upwardsModifier, ForceMode _forceMode)
    {
        //_gameObject.GetComponent<Rigidbody>().AddExplosionForce(_force, _position, _radius, _upwardsModifier, _forceMode);
        this.RpcAddExplosionForce(_gameObject, _force, _position, _radius, _upwardsModifier, _forceMode);
    }

    [ClientRpc]
    private void RpcAddExplosionForce(GameObject _gameObject, float _force, Vector3 _position, float _radius, float _upwardsModifier, ForceMode _forceMode)
    {
        _gameObject.GetComponent<Rigidbody>().AddExplosionForce(_force, _position, _radius, _upwardsModifier, _forceMode);
    }

    [ServerCallback]
    private void OnDestroy()
    {
        for (int i = 0; i < m_players.Count; i++)
        {
            if (m_players[i] != null)
            {
                m_players[i].SetIsHitBomb(false);
            }
        }
    }

    // デバッグ用にギズモで範囲を可視化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_explosionRadius);
    }
}
