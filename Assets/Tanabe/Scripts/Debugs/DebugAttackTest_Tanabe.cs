using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugAttackTest_Tanabe : NetworkBehaviour
{
    private CharacterBase m_parentCharacter;

    // Trigger�Փ˔��菈��
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (m_parentCharacter == null || other.isTrigger) { return; }

        // �L�����N�^�[�f�[�^�̎擾
        CharacterBase characterBase = other.GetComponent<CharacterBase>();
        // �L�����N�^�[�łȂ����return����
        if (characterBase == null || characterBase.GetCharacterType() != CharacterBase.CharacterType.ENEMY_TYPE) { return; }
        // �G����U���͂��擾���ă_���[�W�Ƃ��Čv�Z����
        characterBase.RpcDamage(m_parentCharacter.GetPower());
    }

    // �e�ƂȂ�L�����N�^�[�I�u�W�F�N�g�̐ݒ�
    public void SetParentCharacter(CharacterBase _characterBase)
    {
        m_parentCharacter = _characterBase;
    }

    // �e�ƂȂ�L�����N�^�[�I�u�W�F�N�g�̐ݒ�
    [Command]
    public void CmdSetParentCharacter(CharacterBase _characterBase)
    {
        m_parentCharacter = _characterBase;
    }
}
