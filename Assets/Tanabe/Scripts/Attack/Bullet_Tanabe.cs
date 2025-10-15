using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterBase;

public class Bullet_Tanabe : MonoBehaviour
{
    [SerializeField] private float m_speed = 40f;
    private Vector3 m_forward;
    private Rigidbody m_rb;
    [SerializeField] private float m_activeTime = 5;

    private float m_power;
    // �ђʒe
    private bool m_isPierce = false;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        m_activeTime -= Time.deltaTime;
        if (m_activeTime <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        m_rb.MovePosition(this.transform.position + m_forward * m_speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Bullet_Tanabe>() != null) { return; }
        //if(other.gameObject.GetComponent<Player>() != null)
        //{
        //    other.gameObject.GetComponent<Player>().SetHp(other.gameObject.GetComponent<Player>().GetHp() - 1);
        //}

        if (!m_isPierce || other.gameObject.layer == 3)
        {
            Destroy(this.gameObject);
        }

        // �L�����N�^�[�f�[�^�̎擾
        CharacterBase characterBase = other.GetComponent<CharacterBase>();
        // �L�����N�^�[�łȂ����return����
        if (characterBase == null || characterBase.GetCharacterType() != CharacterType.ENEMY_TYPE) { return; }
        // �G����U���͂��擾���ă_���[�W�Ƃ��Čv�Z����
        characterBase.Damage(m_power);

    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if(other.gameObject.GetComponent<Bullet>() != null) { return; }
    //    if(other.gameObject.GetComponent<Player>() != null)
    //    {
    //        other.gameObject.GetComponent<Player>().SetHp(other.gameObject.GetComponent<Player>().GetHp() - 1);
    //    }
    //    Destroy(this.gameObject);
    //}

    // �V���b�g�I�I�I�I�I
    public void Shot(float _power, Transform _gunHead)
    {
        m_power = _power;
        m_activeTime = 1f;
        m_forward = _gunHead.forward;
        this.transform.localPosition = _gunHead.transform.position + m_forward * 0.5f;

        this.GetComponent<MeshRenderer>().material.color = Color.gray;
    }

    // �V���b�g�K��
    public void ShotGun(float _power, Transform _gunHead, Vector3 _moveVector)
    {
        m_power = _power;
        this.transform.localScale = this.transform.localScale * 0.6f;
        m_activeTime = 0.4f;
        m_speed *= 0.7f;
        m_forward = _moveVector;
        this.transform.localPosition = _gunHead.transform.position + m_forward * 0.5f;

        this.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    // ��e
    public void SharpShot(float _power, Transform _gunHead)
    {
        m_isPierce = true;
        m_power = _power;
        m_activeTime = 3f;
        m_forward = _gunHead.forward;
        this.transform.localPosition = _gunHead.transform.position + m_forward * 0.5f;

        this.GetComponent<MeshRenderer>().material.color = Color.magenta;
    }
}
