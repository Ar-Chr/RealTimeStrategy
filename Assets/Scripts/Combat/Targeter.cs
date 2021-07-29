using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    public Targetable Target { get; private set; }

    public override void OnStartServer()
    {
        Abstract_GameEnder.PlayerDied += ServerHandlePlayerDied;
    }

    public override void OnStopServer()
    {
        Abstract_GameEnder.PlayerDied -= ServerHandlePlayerDied;
    }

    [Command]
    public void CmdSetTarget(Targetable newTarget)
    {
        SetTarget(newTarget);
    }
    
    [Server]
    private void SetTarget(Targetable newTarget)
    {
        Target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        Target = null;
    }

    private void ServerHandlePlayerDied(RTSPlayer player)
    {
        if (connectionToClient != player.connectionToClient)
            return;

        ClearTarget();
    }
}
