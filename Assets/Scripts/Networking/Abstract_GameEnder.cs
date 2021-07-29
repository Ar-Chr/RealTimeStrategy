using System;
using Mirror;
using UnityEngine;

public abstract class Abstract_GameEnder : NetworkBehaviour
{
    public static event Action<RTSPlayer> GameEnded;
    public static event Action<RTSPlayer> PlayerDied;

    [Server]
    protected void InvokeGameEnded(RTSPlayer winner)
    {
        GameEnded?.Invoke(winner);
        RpcInvokeGameEnded(winner);
    }

    [ClientRpc]
    private void RpcInvokeGameEnded(RTSPlayer winner)
    {
        GameEnded?.Invoke(winner);
    }

    [Server]
    protected void InvokePlayerDied(RTSPlayer player)
    {
        PlayerDied?.Invoke(player);
        RpcInvokePlayerDied(player);
    }

    [ClientRpc]
    private void RpcInvokePlayerDied(RTSPlayer player)
    {
        PlayerDied?.Invoke(player);
    }
}
