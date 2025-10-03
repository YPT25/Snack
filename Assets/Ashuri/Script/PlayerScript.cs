using UnityEngine; // Unity�G���W���@�\
using Mirror; // Mirror�l�b�g���[�L���O�@�\
using TMPro; // TextMeshPro���g�p���邽�߂ɒǉ�

// �l�b�g���[�N�Ή��̃v���C���[���[�u�����g
public class PlayerScript : NetworkBehaviour
{
    [Header("UI�̎Q�Əꏊ")] // UI�Q�Ƃ̃Z�N�V�����w�b�_�[
    [Tooltip("�v���C���[�̓���ɕ\������閼�O��TextMeshPro�R���|�[�l���g�B")] // playerNameText�̃c�[���`�b�v
    public TextMeshPro playerNameText; // TextMeshPro�Ɍ^��ύX
    [Tooltip("�v���C���[�̓���ɕ\���������UI�̃��[�gGameObject�B")] // floatingInfo�̃c�[���`�b�v
    public GameObject floatingInfo; // ����̏��UI

    private Material playerMaterialClone; // �v���C���[�}�e���A��

    [Header("�l�b�g���[�N�œ�������v���C���[���")] // �l�b�g���[�N���������v���C���[�f�[�^�̃Z�N�V�����w�b�_�[
    [Tooltip("�v���C���[�̖��O�B�T�[�o�[����N���C�A���g�֓�������܂��B")] // playerName�̃c�[���`�b�v
    [SyncVar(hook = nameof(OnNameChanged))] // ���O�ύX�𓯊�
    public string playerName;

    [Tooltip("�v���C���[�̐F�B�T�[�o�[����N���C�A���g�֓�������܂��B")] // playerColor�̃c�[���`�b�v
    [SyncVar(hook = nameof(OncolorChanged))] // �F�ύX�𓯊�
    public Color playerColor = Color.white;

    // ���O�ύX���ɌĂ΂��
    void OnNameChanged(string _oldName, string _newName)
    {
        if (playerNameText != null)
        {
            playerNameText.text = _newName;
        }
    }

    // �F�ύX���ɌĂ΂��
    void OncolorChanged(Color _oldColor, Color _NewColor)
    {
        if (playerNameText != null)
        {
            playerNameText.color = _NewColor; // ���O�̐F��ݒ�
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // �}�e���A�����N���[�����ĐF��ύX���A���̃}�e���A���𒼐ڕύX���Ȃ��悤�ɂ���
            // ����ɂ��APrefab�̃}�e���A�����ύX�����̂�h��
            playerMaterialClone = new Material(renderer.material);
            playerMaterialClone.color = _NewColor; // �v���C���[�̐F��ݒ�
            renderer.material = playerMaterialClone;
        }
    }

    // ���[�J���v���C���[�����ݒ�
    public override void OnStartLocalPlayer()
    {
        // �J�����ݒ�
        if (Camera.main != null)
        {
            Camera.main.transform.SetParent(transform); // �J�������v���C���[�̎q��
            Camera.main.transform.localPosition = new Vector3(0, 3, -10); // �J�����ʒu����
            Camera.main.transform.localRotation = Quaternion.Euler(15, 0, 0); // �J�����̌����������������ɒ�������ꍇ
        }

        // ���[�J���v���C���[�̏ꍇ�A����̏��UI���\���ɂ���
        if (floatingInfo != null)
        {
            floatingInfo.SetActive(false); // ��������ǉ���
        }

        // �����_���Ȗ��O�ƐF�𐶐�
        string name = "Player" + Random.Range(100, 999);
        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        // �R�}���h��isLocalPlayer�ł���ꍇ�ɂ̂݌Ăׂ�
        // CmdSetupPlayer�̓��[�J���v���C���[���T�[�o�[�ɐݒ��v�����邽�߂̂��̂Ȃ̂ŁA���̂܂܂�OK
        CmdSetupPlayer(name, color);
    }

    // �T�[�o�[�Ŏ��s�����R�}���h
    [Command]
    public void CmdSetupPlayer(string _name, Color _color)
    {
        // �T�[�o�[���SyncVar�̒l��ݒ�
        playerName = _name;
        playerColor = _color;
    }

    // ���t���[���X�V
    void Update()
    {
        // ���[�J���v���C���[�̏ꍇ�̂݁A���͏������s��
        if (isLocalPlayer) // !isLocalPlayer �ł͂Ȃ� isLocalPlayer
        {
            float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f; // ���E��]����
            float moveY = Input.GetAxis("Vertical") * Time.deltaTime * 4f; // �O��ړ�����

            transform.Rotate(0, moveX, 0); // Y����]
            transform.Translate(0, 0, moveY); // Z���ړ�
        }
        else // ���̃v���C���[�̏ꍇ�AUI���J�����̕��Ɍ�������
        {
            if (floatingInfo != null && Camera.main != null)
            {
                // �J�����̕������������邱�ƂŁA��ɕ������v���C���[�Ɍ�����悤�ɂ���
                floatingInfo.transform.LookAt(Camera.main.transform);
                // �������ALookAt��Y���𔽓]�����Ă��܂����Ƃ�����̂ŁA��萳�m�ɂ͈ȉ��̂悤�ɂ���
                floatingInfo.transform.forward = -Camera.main.transform.forward; // �J�����Ƃ͋t��������������
            }
        }
    }
}