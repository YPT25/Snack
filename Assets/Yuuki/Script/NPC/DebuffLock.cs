using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffLock : NetworkBehaviour
{
    [SyncVar] public uint attachedNpcNetId;

    [Server]
    public bool TryLock(uint npcNetId)
    {
        if (attachedNpcNetId != 0) return false;
        attachedNpcNetId = npcNetId;
        return true;
    }

    [Server]
    public void Unlock(uint npcNetId)
    {
        if (attachedNpcNetId == npcNetId)
            attachedNpcNetId = 0;
    }
}
