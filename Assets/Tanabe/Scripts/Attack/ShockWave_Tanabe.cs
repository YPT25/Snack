using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave_Tanabe : MonoBehaviour
{
    private float m_explosionForce = 7.3f;  // ”š•—‚Ì‹­‚³
    private float m_upwardsModifier = 1f;   // ã•ûŒü‚Ì•â³iŽ‚¿ã‚ª‚éŠ´‚¶j

    private float m_waveTimer = 0.0f;
    private bool m_isFall = false;
    private float m_wavePower = 1.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<Player_Tanabe>() != null) { return; }

        CharacterBase characterBase = other.gameObject.GetComponent<CharacterBase>();
        if (characterBase != null && !other.isTrigger)
        {
            characterBase.Damage(10);
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            // ”š•—‚Ì—Í‚ð‰Á‚¦‚é
            rb.AddExplosionForce(m_explosionForce, transform.position, this.transform.localScale.x, m_upwardsModifier, ForceMode.Impulse);
        }
    }
}
