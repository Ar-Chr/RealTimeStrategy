using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnDeath : NetworkBehaviour
{
    [SerializeField] private Health health;

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDeath;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDeath;
    }

    private void ServerHandleDeath()
    {
        NetworkServer.Destroy(gameObject);
    }
}
