using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CannonBox �̒e��
/// �E�Փ˂�������Ƀ_���[�W
/// �E�����ɂ�������iEnemyBase �� m_canFriendlyFire �Ɉˑ��j
/// </summary>
public class CannonBall : MonoBehaviour
{
    [Header("�_���[�W��")]
    public int damage = 20;

    private void OnCollisionEnter(Collision collision)
    {
        CharacterBase target = collision.gameObject.GetComponent<CharacterBase>();
        if (target != null)
        {
            target.Damage(damage);
            Debug.Log($"{target.name} ���C�e���󂯂��I �_���[�W:{damage}");
        }

        Destroy(gameObject); // �g���̂�
    }
}
