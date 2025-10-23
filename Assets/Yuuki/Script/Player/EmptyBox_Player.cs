using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyBox_Player : MPlayerBase
{
    protected override void OnAttackInput()
    {
        // �^�[�Q�b�g���擾�i���F�ڂ̑O�ɂ�����̂��^�[�Q�b�g�Ƃ���j
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2.0f))
        {
            CharacterBase target = hit.collider.GetComponent<CharacterBase>();
            if (target != null)
            {
                Attack(target);
            }
        }
    }

    public override void Attack(CharacterBase target)
    {
        base.Attack(target); // �_���[�W�����̓x�[�X�ɔC����i�K�v�Ȃ炱���ō폜�j

        // �S��������ǉ�
        RestrainComponent restrain = target.gameObject.AddComponent<RestrainComponent>();
        restrain.StartRestrain(10.0f); // 10�b�ԍS��
        Debug.Log($"{name} �� {target.name} ���S�������I");
    }
}
