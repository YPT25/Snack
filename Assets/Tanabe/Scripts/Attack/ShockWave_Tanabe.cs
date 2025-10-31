using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShockWave_Tanabe : NetworkBehaviour
{
    private float m_explosionForce = 7.3f;  // ”š•—‚Ì‹­‚³
    private float m_upwardsModifier = 1f;   // ã•ûŒü‚Ì•â³iŽ‚¿ã‚ª‚éŠ´‚¶j

    private float m_waveTimer = 0.0f;
    [SyncVar] private bool m_isFall = false;
    [SyncVar] private float m_wavePower = 1.0f;

    private GameObject m_parentPlayer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
        m_waveTimer += Time.deltaTime;

        Vector3 scale = this.transform.localScale;
        if (m_isFall)
        {
            scale.x = m_waveTimer * (12.0f + m_wavePower);
            scale.z = m_waveTimer * (12.0f + m_wavePower);
        }
        else
        {
            scale.x = m_waveTimer * 12.0f;
            scale.z = m_waveTimer * 12.0f;
        }
        this.transform.localScale = scale;

        if (m_waveTimer >= 0.5f)
        {
            Destroy(this.gameObject);
        }
    }

    public void Fall(float _wavePower)
    {
        m_isFall = true;
        m_wavePower = _wavePower;
    }

    public void SetParentPlayer(GameObject _parentPlayer)
    {
        m_parentPlayer = _parentPlayer;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        CharacterBase characterBase = other.gameObject.GetComponent<CharacterBase>();
        if (other.GetComponent<NetworkIdentity>() == null || other.isTrigger || characterBase == null || characterBase.GetCharacterType() == CharacterBase.CharacterType.HERO_TYPE) { return; }

        if(other.gameObject == m_parentPlayer) { return; }

        if (characterBase.GetCharacterType() == CharacterBase.CharacterType.ENEMY_TYPE)
        {
            characterBase.RpcDamage(10);
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            // ”š•—‚Ì—Í‚ð‰Á‚¦‚é
            //rb.AddExplosionForce(m_explosionForce, transform.position, this.transform.localScale.x, m_upwardsModifier, ForceMode.Impulse);
            this.RpcAddExplosionForce(other.gameObject);
        }
    }

    [ClientRpc]
    private void RpcAddExplosionForce(GameObject _gameObject)
    {
        // ”š•—‚Ì—Í‚ð‰Á‚¦‚é
        _gameObject.GetComponent<Rigidbody>().AddExplosionForce(m_explosionForce, transform.position, this.transform.localScale.x, m_upwardsModifier, ForceMode.Impulse);
    }
}
