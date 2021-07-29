using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBuildingGameEnder : Abstract_GameEnder
{
    private List<MainBuilding> mainBuildings =
        new List<MainBuilding>();

    public override void OnStartServer()
    {
        MainBuilding.ServerOnMainBuildingSpawned +=
            ServerHandleMainBuildingSpawned;
        
        MainBuilding.ServerOnMainBuildingDespawned +=
            ServerHandleMainBuildingDespawned;
    }

    public override void OnStopServer()
    {
        MainBuilding.ServerOnMainBuildingSpawned -=
            ServerHandleMainBuildingSpawned;

        MainBuilding.ServerOnMainBuildingDespawned -=
            ServerHandleMainBuildingDespawned;
    }

    private void ServerHandleMainBuildingSpawned(MainBuilding mainBuilding)
    {
        mainBuildings.Add(mainBuilding);
    }

    private void ServerHandleMainBuildingDespawned(MainBuilding mainBuilding)
    {
        mainBuildings.Remove(mainBuilding);

        InvokePlayerDied(mainBuilding.connectionToClient.identity.GetComponent<RTSPlayer>());

        if (mainBuildings.Count == 1)
            InvokeGameEnded(mainBuildings[0].connectionToClient.identity.GetComponent<RTSPlayer>());
    }
}
