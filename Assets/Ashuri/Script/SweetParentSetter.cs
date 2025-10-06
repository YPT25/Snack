using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SweetParentSetter : NetworkBehaviour
{
    /// <summary>
    /// �N���C�A���g�ɌĂяo����A�w�肳�ꂽNetworkIdentity�����I�u�W�F�N�g��e�Ƃ��Đݒ肷��B
    /// </summary>
    /// <param name="parentIdentity">�e�Ƃ��Đݒ肷��I�u�W�F�N�g��NetworkIdentity</param>
    [ClientRpc]
    public void RpcSetParent(NetworkIdentity parentIdentity)
    {
        // �T�[�o�[���z�X�g�̏ꍇ�A���ɃT�[�o�[���Őe���ݒ肳��Ă���\�������邽�߁A�d���ݒ�������
        // �������ANetworkServer.Spawn��AClientRpc���Ă΂��O�ɃN���C�A���g���Őe�����[�g�ɂȂ��Ă���\��������
        // �����ł́AisServer && isClient �̏ꍇ�̓X�L�b�v�����A�N���C�A���g���ł̂�SetParent�����s����
        // if (isServer && isClient) return; // �z�X�g�̏ꍇ�̓X�L�b�v (�T�[�o�[���Őݒ�ς݂̂���)

        if (parentIdentity != null)
        {
            // �N���C�A���g���Őe�I�u�W�F�N�g��Transform���擾���A���g�̎q�ɐݒ�
            transform.SetParent(parentIdentity.transform);

            // �e��ݒ肵����A���[�J�����W���]�����Z�b�g���Đe�ɑ΂��鑊�Έʒu�𒲐�����
            //transform.localPosition = Vector3.zero; // �e�̌��_�ɔz�u
            transform.localRotation = Quaternion.identity; // �e�Ɠ�����]
            transform.localScale = Vector3.one; // �e�Ɠ����X�P�[�� (�K�v�ɉ�����)
        }
        else
        {
            Debug.LogWarning("RpcSetParent: parentIdentity is null on client. Cannot set parent.");
        }
    }
}
