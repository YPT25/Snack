using UnityEngine; // Unity��{�@�\
using Mirror; // Mirror�l�b�g���[�N
using TMPro; // TextMeshPro�pUI�\��

public class TimeText : NetworkBehaviour
{
    [Header("UI�Q�Əꏊ")]
    [Tooltip("�c�莞�Ԃ�\������TextMeshProUGUI�R���|�[�l���g�B")]
    [SerializeField]
    private TextMeshProUGUI timeDisplay;

    private GameManager gameManager; // GameManager�ւ̎Q��

    // ===============================
    // �N���C�A���g�J�n������
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();
        // �N���C�A���g���J�n���ꂽ�Ƃ���GameManager�ւ̎Q�Ƃ��擾
        TryFindGameManager();
        // �����\������x�X�V
        if (gameManager != null)
        {
            UpdateTimeDisplay(gameManager.remainingGameTime);
        }
        else
        {
            // GameManager��������Ȃ��ꍇ��0�ŏ����\��
            UpdateTimeDisplay(0);
        }
    }

    void Update()
    {
        // �N���C�A���g�̏ꍇ�̂�UI���X�V
        if (isClient)
        {
            // GameManager���܂��擾�ł��Ă��Ȃ��ꍇ�A���t���[���T��
            if (gameManager == null)
            {
                TryFindGameManager();
            }

            // GameManager��������Ύc�莞�Ԃ�\��
            if (gameManager != null)
            {
                UpdateTimeDisplay(gameManager.remainingGameTime);
            }
        }
    }

    // ===============================
    // GameManager��T�����\�b�h
    // ===============================
    private void TryFindGameManager()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            // Debug.LogWarning("TimeText: GameManager not found yet. Retrying next frame."); // �f�o�b�O�p
        }
    }

    // ===============================
    // ���ԕ\���X�V���\�b�h
    // ===============================
    void UpdateTimeDisplay(float timeToDisplay)
    {
        if (timeDisplay != null)
        {
            int minutes = Mathf.FloorToInt(timeToDisplay / 60);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timeDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // ===============================
    // �X�N���v�g���������FGameManager�Q�ƃN���A
    // ===============================
    void OnDisable()
    {
        gameManager = null;
    }
}
