using System.Collections; // �R���[�`���g�p�̂���
using UnityEngine;        // Unity��{�@�\
using Mirror;             // Mirror�l�b�g���[�N�@�\
using TMPro;              // TextMeshProUI�\���p

/// <summary>
/// Mirror�p�̃Q�[���i�s�Ǘ��N���X
/// �E���Ԍo�߂ɂ��Q�[���I������
/// �E�X�R�A�\��UI�̐���
/// </summary>
public class GameManager : NetworkBehaviour
{
    // ===============================
    // �Q�[�����Ԋ֘A�̐ݒ�
    // ===============================

    [Header("�Q�[���S�ʂ̎��Ԑݒ�")]
    [Tooltip("���݂̎c�莞�ԁB�T�[�o�[����N���C�A���g�֓�������܂��B")]
    [SyncVar(hook = nameof(OnTimeChange))] // �l�ύX���ɌĂ΂��t�b�N��ݒ�
    public float remainingGameTime; // �c�莞�ԁi�T�[�o�[���N���C�A���g�����j

    // �Q�[���J�n���̏������ԁi�b�P�ʁj
    [Header("�Q�[�������ݒ�")]
    [SerializeField] public float initialGameTime = 180f;

    // �Q�[���J�n�O�̃J�E���g�_�E�����ԁi�b�P�ʁj
    [Header("�Q�[���J�n�O�J�E���g�_�E��")]
    [SerializeField] public float preGameCountdownTime = 3f;

    // ===============================
    // �Q�[���i�s��Ԃ̊Ǘ�
    // ===============================

    [Header("�Q�[�����")]
    [Tooltip("�Q�[�����J�n���ꂽ���ǂ���")]
    [SyncVar(hook = nameof(OnGameStartChanged))] // �ύX���ɃN���C�A���g�����������s
    public bool gameStarted = false; // �Q�[���J�n�t���O

    // ===============================
    // �X�R�A�\���pUI�ݒ�
    // ===============================

    [Header("�X�R�AUI�֘A")]
    [Tooltip("�Q�[���I����ɃX�R�A��\������TextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI scoreText; // �X�R�A��\������UI�e�L�X�g
    [Tooltip("�X�R�A�\���p�l���i��\�����\���ɐ؂�ւ���j")]
    [SerializeField] private GameObject scorePanel; // �X�R�AUI�p�l��

    // ���݂̃X�R�A��ێ�����ϐ�
    private int currentScore = 0;

    // �V���O���g���C���X�^���X�i�ǂ�����ł��Q�Ƃł���j
    public static GameManager Instance { get; private set; }

    // ===============================
    // �T�[�o�[�J�n���̏���
    // ===============================
    public override void OnStartServer()
    {
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
        base.OnStartClient();

        // �V���O���g���o�^�i�N���C�A���g���j
        if (Instance == null) Instance = this;

        // �X�R�A�p�l�����ŏ��͔�\���ɐݒ�
        if (scorePanel != null)
            scorePanel.SetActive(false);
    }

    // ===============================
    // SyncVar�t�b�N�F���ԕύX���ɌĂ΂��
    // ===============================
    void OnTimeChange(float _oldTime, float _newTime)
    {
        // �c�莞�Ԃ�0�ȉ��ɂȂ�����Q�[���I�����������s�i�T�[�o�[���̂݁j
        if (isServer && _newTime <= 0f)
        {
            EndGame();
        }
    }

    // ===============================
    // SyncVar�t�b�N�F�Q�[���J�n��Ԃ��ω������Ƃ��ɌĂ΂��
    // ===============================
    void OnGameStartChanged(bool _oldValue, bool _newValue)
    {
        // �N���C�A���g���ŃQ�[�����J�n���ꂽ�Ƃ��ɌĂ΂��
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
        // �J�E���g�_�E���b����ݒ�
        float countdown = preGameCountdownTime;

        // �J�E���g�_�E����0�ɂȂ�܂Ń��[�v
        while (countdown > 0)
        {
            // �c��b�������O�ɕ\��
            Debug.Log($"Game starts in {Mathf.Ceil(countdown)}");

            // 1�b�ҋ@
            yield return new WaitForSeconds(1f);

            // �J�E���g�_�E����1�b���炷
            countdown -= 1f;
        }

        // �T�[�o�[���ŃQ�[���J�n���O���o��
        Debug.Log("Game Started! (Server)");

        // �Q�[���J�n�t���O��L����
        gameStarted = true;

        // �Q�[�����̎��ԃJ�E���g�_�E�����J�n
        StartCoroutine(ServerCountdownCoroutine());
    }

    // ===============================
    // �T�[�o�[���F�Q�[�����̎c�莞�ԃJ�E���g�_�E��
    // ===============================
    IEnumerator ServerCountdownCoroutine()
    {
        // 1�b���ƂɎ��Ԃ����炵�ē������s��
        while (remainingGameTime > 0)
        {
            // 1�b�ҋ@�iSyncVar�̃l�b�g���[�N���ׂ��y���j
            yield return new WaitForSeconds(1f);

            // �c�莞�Ԃ�1�b���炷
            remainingGameTime -= 1f;

            // �c�莞�Ԃ����ɂȂ�Ȃ��悤�␳
            if (remainingGameTime < 0)
                remainingGameTime = 0;
        }

        // ���Ԑ؂�ɂȂ�����Q�[���I�����������s
        EndGame();
    }


    // ===============================
    // �T�[�o�[���F�Q�[���I������
    // ===============================
    [Server]
    void EndGame()
    {
        // �Q�[���I�������O�o��
        Debug.Log("Time's up! (Server) Game Over!");

        // �Q�[���i�s�t���O���I�t��
        gameStarted = false;

        //�X�R�A���擾
        SweetScore sweetScore = FindObjectOfType<SweetScore>();
        currentScore = sweetScore.currentScore;

        // �S�N���C�A���g�ɃQ�[���I���ƃX�R�A�\����ʒm
        RpcShowScore(currentScore);
    }

    // ===============================
    // �N���C�A���g���F�X�R�AUI��\�����鏈��
    // ===============================
    [ClientRpc]
    void RpcShowScore(int finalScore)
    {
        // �N���C�A���g�Ń��O�o��
        Debug.Log("Game Over! Showing Score (Client)");

        // �X�R�A�p�l����\��
        if (scorePanel != null)
            scorePanel.SetActive(true);

        // �X�R�A�e�L�X�g�ɍŏI�X�R�A��\��
        if (scoreText != null)
            scoreText.text = $"Your team Score: {finalScore}";

        // �Q�[���S�̂��ꎞ��~
        Time.timeScale = 0f;
    }

    // ===============================
    // �T�[�o�[���F�X�R�A���Z�����i�O������Ăяo���j
    // ===============================
    [Server]
    public void AddScore(int value)
    {
        // ���݂̃X�R�A�ɉ��Z
        currentScore += value;
    }

    // ===============================
    // �N���C�A���g��~���F�V���O���g��������
    // ===============================
    public override void OnStopClient()
    {
        base.OnStopClient();

        // ���g���C���X�^���X�Ȃ���
        if (Instance == this)
            Instance = null;
    }

    // ===============================
    // �T�[�o�[��~���F�S�R���[�`�����~
    // ===============================
    public override void OnStopServer()
    {
        base.OnStopServer();

        // ���쒆�̃R���[�`�������ׂĒ�~
        StopAllCoroutines();
    }
}
