using Mirror;     // Mirror�l�b�g���[�N�@�\
using TMPro;      // TextMeshPro���g�p
using UnityEngine; // Unity�̊�{�N���X�g�p

/// <summary>
/// �Q�[���I����̃X�R�AUI���Ǘ�����N���X
/// GameManager����Ăяo�����UI��\�����A�Q�[�����~������
/// </summary>
public class ResultUIScore : NetworkBehaviour
{
    [Header("�X�R�AUI�֘A")]
    [Tooltip("�X�R�A��\������TextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI scoreText; // �X�R�A�e�L�X�g

    [Tooltip("�X�R�A�p�l���i��\�����\���؂�ւ��j")]
    [SerializeField] private GameObject scorePanel; // �X�R�A�p�l���I�u�W�F�N�g

    // �V���O���g���C���X�^���X��ێ�
    public static ResultUIScore Instance { get; private set; }

    // ===============================
    // �N���C�A���g�J�n���̏���
    // ===============================
    public override void OnStartClient()
    {
        // �e�N���X�̏������Ă�
        base.OnStartClient();

        // �V���O���g���o�^�i�d���h�~�j
        if (Instance == null) Instance = this;

        // �Q�[���J�n���̓X�R�A�p�l�����\���ɂ��Ă���
        if (scorePanel != null)
            scorePanel.SetActive(false);
    }

    // ===============================
    // �N���C�A���gRPC�F�X�R�A��S�N���C�A���g�ɕ\��
    // ===============================
    [ClientRpc]
    public void RpcShowScore(float finalScore)
    {
        // �N���C�A���g�ŃX�R�A�\�����J�n
        ShowScore(finalScore);
    }

    // ===============================
    // �N���C�A���g���F�X�R�AUI�\������
    // ===============================
    public void ShowScore(float finalScore)
    {
        // �f�o�b�O���O��\��
        Debug.Log("Game Over! Showing Score (Client)");

        // ���ׂẴv���C���[���擾
        Player_Tanabe[] players = FindObjectsOfType<Player_Tanabe>();

        // �X�R�A�p�l����\��
        if (scorePanel != null)
            scorePanel.SetActive(true);

        // �X�R�A�e�L�X�g�����݂���ꍇ
        if (scoreText != null)
        {
            // �X�R�A�\���p����������
            string allScores = "";

            // �e�v���C���[�̃X�R�A��ǉ�
            for (int i = 0; i < players.Length; i++)
            {
                Player_Tanabe p = players[i];
                allScores += $"Player{p.playerNumber}: {p.m_sweetScore}\n";
            }

            // �`�[���S�̃X�R�A���Ō�ɒǉ�
            allScores += $"\nYour team Score: {finalScore}";

            // �e�L�X�g�ɔ��f
            scoreText.text = allScores;
        }

        // �S�N���C�A���g�ŃQ�[�����~������
        Time.timeScale = 0f;
    }

    // ===============================
    // �N���C�A���g��~���F�V���O���g������
    // ===============================
    public override void OnStopClient()
    {
        // �e�N���X�̒�~�������Ă�
        base.OnStopClient();

        // �o�^�ς݂Ȃ����
        if (Instance == this)
            Instance = null;
    }
}
