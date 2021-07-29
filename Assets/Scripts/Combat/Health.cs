using Mirror;
using System;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action<int, int> ClientOnHealthChanged;
    public event Action ServerOnDie;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        Abstract_GameEnder.PlayerDied += ServerHandlePlayerDied;
    }

    public override void OnStopServer()
    {
        Abstract_GameEnder.PlayerDied -= ServerHandlePlayerDied;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (currentHealth == 0)
            return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth != 0)
            return;

        ServerOnDie?.Invoke();
    }

    [Server]
    private void ServerHandlePlayerDied(RTSPlayer player)
    {
        if (connectionToClient != player.connectionToClient)
            return;

        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    #endregion
}
