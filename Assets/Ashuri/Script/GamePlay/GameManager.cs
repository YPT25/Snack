using System.Collections; // �R���[�`���p
using UnityEngine; // Unity��{�@�\
using Mirror; // Mirror�l�b�g���[�N
using TMPro; // UI�\���p�i�K�v�ɉ����āj

public class GameManager : NetworkBehaviour
{
    [Header("�Q�[���S�ʂ̎��Ԑݒ�")]
    [Tooltip("���݂̎c�莞�ԁB�T�[�o�[����N���C�A���g�֓�������܂��B")]
    [SyncVar(hook = nameof(OnTimeChange))]
    public float remainingGameTime; // ���݂̃Q�[���c�莞�ԁi�T�[�o�[�����p�j

    [Header("�Q�[�������ݒ�")]
    [Tooltip("�Q�[���J�n���̏������� (�b�P��)")]
    [SerializeField]
    private float initialGameTime = 180f; // �Q�[�����Ԃ̏����l�i�ύX�\�j

    [Header("�Q�[���J�n�O�J�E���g�_�E��")]
    [Tooltip("�Q�[���J�n�O�̃J�E���g�_�E������ (�b�P��)")]
    [SerializeField]
    private float preGameCountdownTime = 3f; // �ŏ���3�b�J�E���g�_�E���i�ύX�\�j

    [Header("�Q�[�����")]
    [Tooltip("�Q�[�����J�n���ꂽ���ǂ���")]
    [SyncVar(hook = nameof(OnGameStartChanged))]
    private bool gameStarted = false; // �Q�[���J�n�t���O�i�T�[�o�[����N���C�A���g�ɓ����j

    // �v���p�e�B�F���̃X�N���v�g����Q�Ɖ\
    public float GetRemainingTime => remainingGameTime; // �c�莞�Ԃ��擾
    public bool IsGameStarted => gameStarted; // �Q�[���J�n�t���O���擾

    // �V���O���g���C���X�^���X
    public static GameManager Instance { get; private set; } // GameManager�̗B��C���X�^���X

    // ===============================
    // �T�[�o�[�J�n������
    // ===============================
    public override void OnStartServer()
    {
        base.OnStartServer();
        // �c�莞�Ԃ�������
        remainingGameTime = initialGameTime;
        // �Q�[���O�J�E���g�_�E���J�n
        StartCoroutine(ServerPreGameCountdown());
    }

    // ===============================
    // �N���C�A���g�J�n������
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (Instance == null)
        {
            // �V���O���g���o�^
            Instance = this;
        }
        else if (Instance != this)
        {
            // ���ɑ��݂���ꍇ�͌x��
            Debug.LogWarning("Duplicate GameManager found! Destroying this instance. (This should not happen)");
        }
    }

    // ===============================
    // SyncVar�t�b�N�F�c�莞�ԕύX��
    // ===============================
    void OnTimeChange(float _oldTime, float _newTime)
    {
        // �N���C�A���g����remainingGameTime���ς�����Ƃ��ɌĂ΂��
        // UI����TimeText�Ȃǂ�GetRemainingTime���Q�Ƃ��Ď����X�V����z��
    }

    // ===============================
    // SyncVar�t�b�N�F�Q�[���J�n�t���O�ύX��
    // ===============================
    void OnGameStartChanged(bool _oldValue, bool _newValue)
    {
        if (_newValue)
        {
            // �N���C�A���g�ŃQ�[���J�n���O
            Debug.Log("Game Started! (Client)");
            // �Q�[���J�n�ɉ����������͂����ɒǉ��\
        }
    }

    // ===============================
    // �T�[�o�[���F�Q�[���J�n�O�J�E���g�_�E��
    // ===============================
    IEnumerator ServerPreGameCountdown()
    {
        float countdown = preGameCountdownTime; // �J�E���g�_�E���p�ϐ��ɏ����l�ݒ�
        while (countdown > 0)
        {
            // �c��b�����O
            Debug.Log($"Game starts in {Mathf.Ceil(countdown)}");
            // 1�b�ҋ@
            yield return new WaitForSeconds(1f);
            // �J�E���g�_�E����1�b���炷
            countdown -= 1f;
        }

        // �T�[�o�[���ŃQ�[���J�n���O
        Debug.Log("Game Started! (Server)");
        // SyncVar�ŃN���C�A���g�ɃQ�[���J�n�ʒm
        gameStarted = true;
        // �Q�[���{�҃J�E���g�_�E���J�n
        StartCoroutine(ServerCountdownCoroutine());
    }

    // ===============================
    // �T�[�o�[���F�Q�[�����̎��ԃJ�E���g�_�E��
    // ===============================
    IEnumerator ServerCountdownCoroutine()
    {
        while (remainingGameTime > 0)
        {
            // �t���[�����Ƃɑҋ@
            yield return null;
            // �c�莞�Ԃ��t���[�������Z
            remainingGameTime -= Time.deltaTime;

            if (remainingGameTime < 0)
            {
                // ���̒l�ɂȂ�Ȃ��悤�␳
                remainingGameTime = 0;
            }
        }

        // �Q�[���I�����O
        Debug.Log("Time's up! (GameManager Server)");
        // �Q�[���I�������������ɒǉ��\
    }

    // ===============================
    // �N���C�A���g��~���F�V���O���g�����
    // ===============================
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (Instance == this)
        {
            // �C���X�^���X���
            Instance = null;
        }
    }
}
