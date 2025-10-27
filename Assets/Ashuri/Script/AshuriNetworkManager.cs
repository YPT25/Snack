using UnityEngine; // Unity�̊�{�I�ȋ@�\���g�p
using UnityEngine.SceneManagement; // �V�[���Ǘ��@�\���g�p
using Mirror; // Mirror�l�b�g���[�N�@�\���g�p
using System.Collections; // �R���[�`�����g�p

// Mirror��NetworkManager���g������J�X�^���N���X
public class AshuriNetworkManager : NetworkManager
{
    [Header("�v���C���[�I�u�W�F�N�g")]
    [Tooltip("1�l�`�[���̃I�u�W�F�N�g")]
    public GameObject playerPrefab1;
    [Tooltip("3�l�`�[���̃I�u�W�F�N�g")]
    public GameObject playerPrefab2;

    private int nextPlayerNumber = 1;

    /// <summary>
    /// �N���C�A���g���T�[�o�[�ɐڑ����āu�v���C���[��ǉ��v����Ƃ��ɌĂ΂��B
    /// Mirror�������I�ɌĂяo���T�[�o�[���̃C�x���g�֐��B
    /// </summary>
    /// <param name="conn">
    /// �T�[�o�[�ɐڑ����Ă����N���C�A���g�̐ڑ����B
    /// Mirror���Ǘ�����uNetworkConnectionToClient�v�I�u�W�F�N�g�ł��B
    /// </param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerobj;

        PlayerScript_Ashuri playerScript;

        PlayerControl_Tanabe playerScript_Tanabe;

        // 1�l�ڂ�playerPrefab1�A����ȍ~��playerPrefab2
        if (nextPlayerNumber == 1)
        {
            playerobj = Instantiate(playerPrefab1);

            // Instantiate�����I�u�W�F�N�g����PlayerScript���擾
            playerScript_Tanabe = playerobj.GetComponent<PlayerControl_Tanabe>();
            //playerScript_Tanabe.playerNumber = nextPlayerNumber;
        }
        else
        {
            playerobj = Instantiate(playerPrefab2);

            // Instantiate�����I�u�W�F�N�g����PlayerScript���擾
            playerScript = playerobj.GetComponent<PlayerScript_Ashuri>();
            playerScript.playerNumber = nextPlayerNumber;

            Debug.Log($"�v���C���[�ԍ�{playerScript.playerNumber}���Q�����܂���");
        }

        // Mirror�ɓo�^
        NetworkServer.AddPlayerForConnection(conn, playerobj);

        // �ԍ��𑝂₷
        nextPlayerNumber++;
    }

    /// <summary>
    /// �v���C���[���ޏo�i�ؒf�j�����Ƃ��ɌĂ΂��B
    /// Mirror��NetworkManager�̊���C�x���g���I�[�o�[���C�h���Ă��܂��B
    /// </summary>
    /// <param name="conn">�ؒf�����v���C���[�̐ڑ����</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // �x�[�X�N���X�iNetworkManager�j�̕W��������Ă�
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// �T�[�o�[���~�����Ƃ��ɌĂ΂�܂��B
    /// �v���C���[�ԍ��J�E���^�Ȃǂ����Z�b�g���Ă����܂��B
    /// </summary>
    public override void OnStopServer()
    {
        base.OnStopServer();

        // �ԍ����Z�b�g�i�V�����Z�b�V�����̂��߁j
        nextPlayerNumber = 1;
    }

    // ���̃X�N���v�g���̂ɂ�FadeSceneTransition�ւ̒��ڎQ�Ƃ͕s�v�ł�
    // �V���O���g���p�^�[���ŃA�N�Z�X���܂�

    // NetworkManager��Awake��Mirror�����Ŏg���邽�߁A���ʂȏ������Ȃ�����I�[�o�[���C�h���Ȃ�
    // ����Awake�ŉ����������ꍇ��base.Awake()���Ăяo��
    // protected override void Awake()
    // {
    //     base.Awake();
    //     Debug.Log("MyNetworkManager Awake custom logic.");
    // }


    // �Q�[���J�n�{�^���Ȃǂ������ꂽ�Ƃ��ɌĂяo�����\�b�h
    // �z�X�g�i�T�[�o�[���N���C�A���g�j�����r�[�V�[���ŌĂяo�����Ƃ�z��
    //public void StartGame()
    //{
    //    // �T�[�o�[���A�N�e�B�u�ŁA���݂̃V�[����"LobbyScene"�̏ꍇ�̂ݏ�����i�߂�
    //    if (NetworkServer.active && SceneManager.GetActiveScene().name == "testScene")
    //    {
    //        Debug.Log("MyNetworkManager: Host starting game and changing scene to GameScene.");

    //        // FadeSceneTransition�̃V���O���g���C���X�^���X�����݂��邩�m�F
    //        if (FadeSceneTransition.Instance != null)
    //        {
    //            Debug.Log("MyNetworkManager: Found FadeSceneTransition instance. Starting fade and scene load.");
    //            // �t�F�[�h�A�E�g�R���[�`�����J�n���A���ꂪ��������̂�҂��Ă���V�[���ύX���s��
    //            StartCoroutine(ServerChangeSceneWithFade("FirstScene"));
    //        }
    //        else
    //        {
    //            // FadeSceneTransition��������Ȃ��ꍇ�́A�t�F�[�h�Ȃ��Œ��ڃV�[����ύX
    //            Debug.LogWarning("MyNetworkManager: FadeSceneTransition.Instance not found. Changing scene without fade.");
    //            ServerChangeScene("FirstScene"); // Mirror�̃T�[�o�[���V�[���ύX���\�b�h���Ăяo��
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("MyNetworkManager: StartGame called, but not as active server in LobbyScene, or conditions not met.");
    //    }
    //}

    //// �t�F�[�h�A�E�g�𔺂���Mirror�̃T�[�o�[�V�[����ύX����R���[�`��
    //private IEnumerator ServerChangeSceneWithFade(string newSceneName)
    //{
    //    // FadeSceneTransition�̃C���X�^���X�����݂���ꍇ�̂݃t�F�[�h�A�E�g�������s��
    //    if (FadeSceneTransition.Instance != null)
    //    {
    //        // FadeOut�R���[�`�������s���A��������܂őҋ@
    //        yield return StartCoroutine(FadeSceneTransition.Instance.FadeOut());
    //        Debug.Log("MyNetworkManager: Fade out completed for server scene change.");
    //    }

    //    // �t�F�[�h�A�E�g�������i�܂��̓X�L�b�v�j������A�T�[�o�[�ɑ΂��ĐV�����V�[���ւ̕ύX���w��
    //    // ����ɂ��A�ڑ�����Ă��邷�ׂẴN���C�A���g�ɂ��V�����V�[���ւ̃��[�h���ʒm�����
    //    ServerChangeScene(newSceneName);

    //    // �N���C�A���g���ł̃t�F�[�h�C�������́A�e�N���C�A���g��OnClientChangeScene���\�b�h�ŏ��������
    //}

    //// �N���C�A���g���T�[�o�[����̃V�[���ύX�ʒm���󂯎�����ۂɌĂяo�����R�[���o�b�N
    //public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool isCustomHandling)
    //{
    //    // NetworkManager�̃f�t�H���g�̏������Ăяo���i���ɏd�v�j
    //    base.OnClientChangeScene(newSceneName, sceneOperation, isCustomHandling);
    //    Debug.Log($"MyNetworkManager: Client received scene change to: {newSceneName}. Operation: {sceneOperation}");

    //    // �N���C�A���g���Ńt�F�[�h�C���������J�n
    //    if (FadeSceneTransition.Instance != null)
    //    {
    //        Debug.Log("MyNetworkManager: Client starting fade in after receiving scene change notification.");
    //        StartCoroutine(FadeSceneTransition.Instance.FadeIn()); // FadeIn�R���[�`�����Ăяo��
    //    }
    //    else
    //    {
    //        Debug.LogWarning("MyNetworkManager: FadeSceneTransition.Instance not found on client. No fade in applied.");
    //    }
    //}

    //// �N���C�A���g���V�����V�[���̃��[�h�����������ۂɌĂяo�����R�[���o�b�N
    //// Mirror�̃o�[�W�����ɂ���Ă͂��̃��\�b�h�̈������قȂ�ꍇ�����邪�A
    //// �ŐV�łł�NetworkConnection conn�������Ɏ��̂��W��
    //public override void OnClientSceneChanged()
    //{
    //    // NetworkManager�̃f�t�H���g�̏������Ăяo��
    //    base.OnClientSceneChanged();

    //    // �����ŃN���C�A���g���̏����������A�v���C���[�I�u�W�F�N�g�̃X�|�[���AUI�̍X�V�Ȃǂ��s�����Ƃ��ł���
    //    // ��: �N���C�A���g���̃v���C���[�L�����N�^�[���X�|�[������Ă��Ȃ��ꍇ�A�����ŃX�|�[����v������RPC���T�[�o�[�ɑ���
    //}

    //// �T�[�o�[���V�����V�[���̃��[�h�����������ۂɌĂяo�����R�[���o�b�N
    //public override void OnServerSceneChanged(string newSceneName)
    //{
    //    // NetworkManager�̃f�t�H���g�̏������Ăяo��
    //    base.OnServerSceneChanged(newSceneName);
    //    Debug.Log($"MyNetworkManager: Server finished loading new scene: {newSceneName}");

    //    // �����ŃT�[�o�[���̏����������AAI�̔z�u�A�Q�[�����W�b�N�̊J�n�Ȃǂ��s�����Ƃ��ł���
    //    // �Ⴆ�΁A�V�����V�[���Ńv���C���[���X�|�[�������邽�߂̏����Ȃ�
    //}
}
