using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer_Tanabe : NetworkBehaviour
{
    public enum SoundNum
    {
        SHOT,
    }

    public enum BGMNum
    {
        BGM1,
    }

    [SerializeField] private AudioClip[] m_sounds;
    [SerializeField] private AudioClip[] m_bgms;
    private AudioSource m_audioSource;

    private Vector3 m_pos;
    private int m_soundCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    [ServerCallback]
    private void Update()
    {
        if(m_soundCount > 0)
        {
            this.RpcPlay3DSound(SoundNum.SHOT, m_pos);
            m_soundCount--;
        }
    }

    [Command]
    public void CmdPlay3DSound(SoundNum _index, Vector3 _position)
    {
        if (m_sounds.Length - 1 < (int)_index)
        {
            Debug.Log("その番号の音はない！！！！！！");
            return;
        }
        this.RpcPlay3DSound(_index, _position);
        m_pos = _position;
    }

    [ClientRpc]
    public void RpcPlay3DSound(SoundNum _index, Vector3 _position)
    {
        if (m_sounds.Length - 1 < (int)_index)
        {
            Debug.Log("その番号の音はない！！！！！！");
            return;
        }
        Debug.Log("PlaySound");
        AudioSource.PlayClipAtPoint(GetSound(_index), _position);
    }

    public AudioClip GetSound(SoundNum _index)
    {
        if (m_sounds.Length - 1 < (int)_index)
        {
            Debug.LogError("その番号の音はない！！！！！！");
            return null;
        }

        return m_sounds[(int)_index];
    }

    public void SetSoundCount(int _count)
    {
        m_soundCount = _count;
    }
}
