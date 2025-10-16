using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Ώۂ��u�S����ԁv�ɂ���R���|�[�l���g
/// �E�ړ���U�����͂𕕂���
/// �E��莞�Ԍ�ɉ�������A�����Ŕj�������
/// </summary>
public class RestrainComponent : MonoBehaviour
{
    private bool isRestrained = false;

    public void StartRestrain(float duration)
    {
        if (!isRestrained)
        {
            isRestrained = true;
            StartCoroutine(RestrainCoroutine(duration));
        }
    }

    private IEnumerator RestrainCoroutine(float duration)
    {
        // �ړ���U���𕕂��邽�߂̃t���O
        // TODO: Player/NPC ���� Update ���� isRestrained ���`�F�b�N���Ė�����
        Debug.Log($"{name} �͍S�����ꂽ�I");

        yield return new WaitForSeconds(duration);

        Debug.Log($"{name} �̍S�����������ꂽ");
        Destroy(this);
    }
}
