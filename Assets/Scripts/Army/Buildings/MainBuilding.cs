using Mirror;
using System;
using UnityEngine;

public class MainBuilding : Building
{
    public static event Action<MainBuilding> ServerOnMainBuildingSpawned;
    public static event Action<MainBuilding> ServerOnMainBuildingDespawned;

    public override void OnStartServer()
    {
        base.OnStartServer();

        ServerOnMainBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        ServerOnMainBuildingDespawned?.Invoke(this);
    }
}
