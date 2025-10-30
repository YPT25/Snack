using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror; // Mirror�̋@�\���g�p���邽�߂ɕK�v

public class GameStartButton : NetworkBehaviour // NetworkBehaviour���p��
{
    public string nextSceneName = "YourNextSceneName"; // �J�ڐ�̃V�[����

    //�V�[���J�ڂ��邩
    private bool isTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
        // �I�u�W�F�N�g���g���K�[�ł��邱�Ƃ��m�F�̃R�����g�A�E�g���������ACollision�p�ɏC��
        Collider comp = GetComponent<Collider>();
        if (comp != null && comp.isTrigger) // ���ύX�_�FisTrigger��ON���ƌx�����o��
        {
            Debug.LogWarning("Collider on " + gameObject.name + " is set to Trigger. For collision detection, 'Is Trigger' should be OFF. Please disable 'Is Trigger'.");
        }
        // Rigidbody���A�^�b�`����Ă��邩�m�F�i�R���W�����ɂ͕K�{�j
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Rigidbody is required for collision detection on " + gameObject.name + ". Please add a Rigidbody component.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // �v���C���[�����̃I�u�W�F�N�g�ɏՓ˂����Ƃ��ɌĂ΂��i�R���W�����p�j
    void OnCollisionEnter(Collision collision) // ���ύX�_�FOnCollisionEnter �ɕύX�A������ Collision �^��
    {
        // �T�[�o�[��ł̂ݏ��������s���A�N���C�A���g�֓���������
        if (!isServer) return; // �T�[�o�[�łȂ���Ώ������Ȃ�
        if (isTrigger) return;
        // ���ύX�_�F�Փ˂����I�u�W�F�N�g��GameObject���擾���A�^�O���m�F
        if (collision.gameObject.CompareTag("Player")) // �G�ꂽ�̂�Player�^�O�̃I�u�W�F�N�g���m�F
        {
            Debug.Log("Player collided with the button! Starting fade and scene transition.");
            // �S�ẴN���C�A���g�Ƀt�F�[�h�A�E�g�ƃV�[���J�ڂ��w������
            RpcRequestSceneChange();
            //�g���K�[�𔭓�������
            isTrigger = true;
        }
    }

    // �N���C�A���g�Ńt�F�[�h�A�E�g�ƃV�[���J�ڂ��J�n����RPC
    [ClientRpc]
    void RpcRequestSceneChange()
    {
        if (FadeManager.Instance != null)
        {
            // �t�F�[�h�A�E�g�ƃV�[���J�ڂ��J�n
            // �R���[�`���̓��m�r�w�C�r�A���炵�����s�ł��Ȃ����߁AFadeManager��Instance����Ăяo��
            StartCoroutine(FadeManager.Instance.FadeOutAndLoadScene(nextSceneName));
        }
        else
        {
            Debug.LogError("FadeManager.Instance not found! Make sure FadeManager is on an active Canvas and has been initialized.");
            // �t�F�[�h�}�l�[�W���[��������Ȃ��ꍇ�ł��A�V�[���J�ڂ����͎��݂�i�t�F�[�h�Ȃ��j
            if (isServer)
            {
                NetworkManager.singleton.ServerChangeScene(nextSceneName);
            }
        }
    }
}