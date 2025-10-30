using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI�𑀍삷�邽�߂ɕK�v
using Mirror; // Mirror�̋@�\���g�p���邽�߂ɕK�v
using UnityEngine.SceneManagement; // �V�[���Ǘ��̂��߂ɕK�v

public class FadeManager : NetworkBehaviour // NetworkBehaviour���p��
{
    public Image fadePanel; // �t�F�[�h�p��UI�p�l��
    public float fadeDuration = 1.5f; // �t�F�[�h�ɂ����鎞��

    public static FadeManager Instance { get; private set; } // �V���O���g���p�^�[��

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // �V�[���J�ڂ��Ă����̃I�u�W�F�N�g���j������Ȃ��悤�ɂ���i�K�v�ɉ����āj
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // �t�F�[�h�A�E�g�i��ʂ��Â��Ȃ�j���Ă���V�[�������[�h����R���[�`��
    public IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // �p�l���̐F���擾
        Color panelColor = fadePanel.color;
        // �A���t�@�l��0����1�֕ω�������
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            panelColor.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadePanel.color = panelColor;
            yield return null; // 1�t���[���҂�
        }
        panelColor.a = 1; // ���S�ɕs�����ɂ���
        fadePanel.color = panelColor;

        // �V�[���J�ځiMirror���g�p�j
        if (NetworkServer.active) // �T�[�o�[�̏ꍇ
        {
            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
        // �N���C�A���g�̏ꍇ�́A�T�[�o�[����̃V�[���ύX��҂��߁A�����ł͉������Ȃ�

        yield return new WaitForSeconds(0.5f); // �V�[�����[�h����������̂������҂�

        // �t�F�[�h�C���i��ʂ����邭�Ȃ�j
        StartCoroutine(FadeIn());
    }

    // �t�F�[�h�C���i��ʂ����邭�Ȃ�j�R���[�`��
    public IEnumerator FadeIn()
    {
        Color panelColor = fadePanel.color;
        // �A���t�@�l��1����0�֕ω�������
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            panelColor.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = 0; // ���S�ɓ����ɂ���
        fadePanel.color = panelColor;
    }
}