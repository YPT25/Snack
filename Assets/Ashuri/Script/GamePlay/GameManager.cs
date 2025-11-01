using Mirror;             // Mirror�l�b�g���[�N�@�\���g�p
using Mirror.Examples.MultipleMatch; // Mirror�̃T���v���@�\�i�K�v�ɉ����āj
using System.Collections; // �R���[�`�����g�p���邽��
using TMPro;              // TextMeshPro���g�p���邽��
using UnityEngine;        // Unity��{�@�\

/// <summary>
/// Mirror�p�̃Q�[���i�s�Ǘ��N���X
/// �E���Ԍo�߂ɂ��Q�[���I������
/// �E�X�R�A�\���ʒm���s���iUI������ResultUIScore�ɔC����j
/// </summary>
public class GameManager : NetworkBehaviour
{
    // ===============================
    // �Q�[�����Ԋ֘A�̐ݒ�
    // ===============================

    [Header("�Q�[���S�ʂ̎��Ԑݒ�")]
    [Tooltip("���݂̎c�莞�ԁB�T�[�o�[����N���C�A���g�֓�������܂��B")]
    [SyncVar(hook = nameof(OnTimeChange))]
    public float remainingGameTime; // �c�莞�ԁi�T�[�o�[���N���C�A���g�����j

    [Header("�Q�[�������ݒ�")]
    [Tooltip("�Q�[���J�n���̏������ԁi�b�P�ʁj")]
    [SerializeField] public float initialGameTime = 180f; // �Q�[���̍��v���ԁi�b�j

    [Header("�Q�[���J�n�O�J�E���g�_�E��")]
    [Tooltip("�Q�[���J�n�O�̃J�E���g�_�E���b��")]
    [SerializeField] public float preGameCountdownTime = 3f; // �J�n�O�J�E���g�_�E��

    // ===============================
    // �Q�[���i�s��Ԃ̊Ǘ�
    // ===============================

    [Header("�Q�[�����")]
    [Tooltip("�Q�[�����J�n���ꂽ���ǂ���")]
    [SyncVar(hook = nameof(OnGameStartChanged))]
    public bool gameStarted = false; // �Q�[���J�n�t���O

    // ===============================
    // �V���O���g���C���X�^���X�̐ݒ�
    // ===============================
    public static GameManager Instance { get; private set; }

    // ===============================
    // �T�[�o�[�J�n���̏���
    // ===============================
    public override void OnStartServer()
    {
        // �e�N���X�̊J�n���������s
        base.OnStartServer();

        // �V���O���g���o�^�i����̂݁j
        if (Instance == null) Instance = this;

        // �c�莞�Ԃ�������
        remainingGameTime = initialGameTime;

        // �Q�[���J�n�O�̃J�E���g�_�E�����J�n
        StartCoroutine(ServerPreGameCountdown());
    }

    // ===============================
    // �N���C�A���g�J�n���̏���
    // ===============================
    public override void OnStartClient()
    {
        // �e�N���X�̊J�n���������s
        base.OnStartClient();

        // �N���C�A���g�����V���O���g���o�^
        if (Instance == null) Instance = this;
    }

    // ===============================
    // SyncVar�t�b�N�F�c�莞�ԕύX���ɌĂ΂��
    // ===============================
    void OnTimeChange(float _oldTime, float _newTime)
    {
        // �c�莞�Ԃ�0�ȉ��ɂȂ�����I���������Ăԁi�T�[�o�[�̂݁j
        if (isServer && _newTime <= 0f)
        {
            EndGame();
        }
    }

    // ===============================
    // SyncVar�t�b�N�F�Q�[���J�n��Ԃ��ω������Ƃ�
    // ===============================
    void OnGameStartChanged(bool _oldValue, bool _newValue)
    {
        // �N���C�A���g���ŃQ�[���J�n���Ƀ��O�o��
        if (_newValue)
        {
            Debug.Log("Game Started! (Client)");
        }
    }

    // ===============================
    // �T�[�o�[���F�Q�[���J�n�O�J�E���g�_�E��
    // ===============================
    IEnumerator ServerPreGameCountdown()
    {
        // �J�E���g�_�E���̎c�莞�Ԃ�ݒ�
        float countdown = preGameCountdownTime;

        // �J�E���g�_�E����0�ɂȂ�܂ŌJ��Ԃ�
        while (countdown > 0)
        {
            // �c��b����\��
            Debug.Log($"Game starts in {Mathf.Ceil(countdown)}");

            // 1�b�҂�
            yield return new WaitForSeconds(1f);

            // �c�莞�Ԃ�1���炷
            countdown -= 1f;
        }

        // �T�[�o�[�ŃQ�[���J�n��\��
        Debug.Log("Game Started! (Server)");

        // �Q�[���J�n�t���O��ON
        gameStarted = true;

        // �Q�[�����ԃJ�E���g�_�E���J�n
        StartCoroutine(ServerCountdownCoroutine());
    }

    // ===============================
    // �T�[�o�[���F�Q�[�����̎��ԃJ�E���g�_�E��
    // ===============================
    IEnumerator ServerCountdownCoroutine()
    {
        // �c�莞�Ԃ�0���傫���ԌJ��Ԃ�
        while (remainingGameTime > 0)
        {
            // 1�b�ҋ@�i�������׌y���j
            yield return new WaitForSeconds(1f);

            // �c�莞�Ԃ�1�b���炷
            remainingGameTime -= 1f;

            // ���̒l�ɂȂ�Ȃ��悤�ɕ␳
            if (remainingGameTime < 0)
                remainingGameTime = 0;
        }

        // �c�莞�Ԃ�0�ɂȂ�����I������
        EndGame();
    }

    // ===============================
    // �T�[�o�[���F�Q�[���I������
    // ===============================
    [Server]
    void EndGame()
    {
        // �T�[�o�[�ŃQ�[���I�������O�\��
        Debug.Log("Time's up! (Server) Game Over!");

        // �Q�[���i�s�t���O��OFF
        gameStarted = false;

        // SweetScore��T���Č��݃X�R�A���擾
        SweetScore sweetScore = FindObjectOfType<SweetScore>();
        float currentScore = sweetScore.currentScore;

        // �S�N���C�A���g�ɃX�R�A�\����ʒm
        ResultUIScore.Instance.RpcShowScore(currentScore);
    }

    // ===============================
    // �N���C�A���g��~���̏���
    // ===============================
    public override void OnStopClient()
    {
        // �e�N���X�̏I���������Ă�
        base.OnStopClient();

        // �V���O���g������
        if (Instance == this)
            Instance = null;
    }

    // ===============================
    // �T�[�o�[��~���̏���
    // ===============================
    public override void OnStopServer()
    {
        // �e�N���X�̏I���������Ă�
        base.OnStopServer();

        // �S�R���[�`�����~
        StopAllCoroutines();
    }
}
