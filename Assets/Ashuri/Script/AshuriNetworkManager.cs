using UnityEngine; // Unity�̊�{�@�\
using UnityEngine.SceneManagement; // �V�[���Ǘ�
using Mirror; // Mirror�l�b�g���[�N
using System.Collections; // �R���[�`���g�p

/// <summary>
/// Mirror��NetworkManager���g�������J�X�^���N���X
/// �v���C���[�Ǘ� + �t�F�[�h�t���V�[���J�ڋ@�\��ǉ�
/// </summary>
public class AshuriNetworkManager : NetworkManager
{
    [Header("�v���C���[�I�u�W�F�N�g")]
    [Tooltip("1�l�`�[���̃I�u�W�F�N�g")]
    public GameObject playerPrefab1;
    [Tooltip("3�l�`�[���̃I�u�W�F�N�g")]
    public GameObject playerPrefab2;

    // ���̃v���C���[�ԍ�
    private int nextPlayerNumber = 1;

    /// <summary>
    /// �N���C�A���g���T�[�o�[�ɐڑ����āu�v���C���[��ǉ��v����Ƃ��ɌĂ΂��
    /// Mirror�����C�x���g
    /// </summary>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerobj;
        Player_Tanabe playerScript_Tanabe;

        // 1�l�ڂ�playerPrefab1�A����ȍ~��playerPrefab2
        if (nextPlayerNumber == 1)
        {
            playerobj = Instantiate(playerPrefab1);
        }
        else
        {
            playerobj = Instantiate(playerPrefab2);
        }

        // �v���C���[�X�N���v�g�ɔԍ������蓖��
        playerScript_Tanabe = playerobj.GetComponent<Player_Tanabe>();
        playerScript_Tanabe.playerNumber = nextPlayerNumber;

        Debug.Log($"�v���C���[�ԍ�{playerScript_Tanabe.playerNumber}���Q�����܂���");

        // Mirror�Ƀv���C���[��o�^
        NetworkServer.AddPlayerForConnection(conn, playerobj);

        // ���̃v���C���[�ԍ��𑝂₷
        nextPlayerNumber++;
    }

    /// <summary>
    /// �v���C���[���ؒf�����Ƃ�
    /// </summary>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// �T�[�o�[���~�����Ƃ��ɌĂ΂��
    /// </summary>
    public override void OnStopServer()
    {
        base.OnStopServer();
        // �V�����Z�b�V�����p�ɔԍ����Z�b�g
        nextPlayerNumber = 1;
    }

   
}
