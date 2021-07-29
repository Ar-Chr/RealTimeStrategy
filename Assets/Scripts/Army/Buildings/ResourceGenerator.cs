using Mirror;
using System;
using UnityEngine;

public class ResourceGenerator : Building
{
    [SerializeField] private int resourcesPerTick;
    [SerializeField] private int secondsBetweenTicks;

    private RTSPlayer player;

    private float timer;

    public override void OnStartServer()
    {
        base.OnStartServer();

        Abstract_GameEnder.GameEnded += HandleGameEnded;
        Abstract_GameEnder.PlayerDied += HandlePlayerDied;

        player = connectionToClient.identity.GetComponent<RTSPlayer>();
        timer = 0;
    }

    [ServerCallback]
    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= secondsBetweenTicks)
        {
            player.AddResources(resourcesPerTick);
            timer -= secondsBetweenTicks;
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        Abstract_GameEnder.GameEnded -= HandleGameEnded;
        Abstract_GameEnder.PlayerDied -= HandlePlayerDied;
    }

    private void HandleGameEnded(RTSPlayer winner)
    {
        enabled = false;
    }

    private void HandlePlayerDied(RTSPlayer player)
    {
        if (connectionToClient != player.connectionToClient)
            return;

        enabled = false;
    }
}
