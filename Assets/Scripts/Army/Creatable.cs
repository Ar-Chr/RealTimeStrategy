using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Creatable : NetworkBehaviour
{
    [Header(nameof(Creatable) + " Fields")]
    [SerializeField] private Sprite icon;
    [SerializeField] private int cost;
    [SerializeField] private float creationTime;
    [SerializeField] private List<Renderer> teamColorRenderers;
    [SerializeField] private GameObject minimapDisplay;

    [SyncVar(hook = nameof(ClientHandleColorChanged))]
    protected Color color;

    public Sprite Icon => icon;
    public int Cost => cost;
    public float CreationTime => creationTime;

    public Selectable Selectable { get; private set; }

    public static event Action<Creatable> OnServerCreatableSpawned;
    public static event Action<Creatable> OnServerCreatableDespawned;

    public static event Action<Creatable> OnAuthorityCreatableSpawned;
    public static event Action<Creatable> OnAuthorityCreatableDespawned;

    public static event Action<Creatable> OnClientCreatableSpawned;
    public static event Action<Creatable> OnClientCreatableDespawned;

    private List<SpriteRenderer> minimapDisplayRenderers;

    private void Awake()
    {
        minimapDisplayRenderers = minimapDisplay
            .GetComponentsInChildren<SpriteRenderer>()
            .ToList();
    }

    #region Server

    public override void OnStartServer()
    {
        RTSPlayer owner = connectionToClient.identity.GetComponent<RTSPlayer>();
        color = owner.TeamColor;

        OnServerCreatableSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        OnServerCreatableDespawned?.Invoke(this);
    }

    #endregion

    #region Client
    
    public override void OnStartClient()
    {
        OnClientCreatableSpawned?.Invoke(this);
    }

    public override void OnStartAuthority()
    {
        Selectable = GetComponent<Selectable>();

        OnAuthorityCreatableSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        OnClientCreatableDespawned?.Invoke(this);

        if (hasAuthority)
            OnAuthorityCreatableDespawned?.Invoke(this);
    }

    [Client]
    public void ClientSetColor(Color newColor)
    {
        teamColorRenderers.ForEach(r => r.material.color = newColor);
        minimapDisplayRenderers.ForEach(r => r.color = newColor);
    }

    protected void ClientHandleColorChanged(Color oldColor, Color newColor)
    {
        ClientSetColor(newColor);
    }

    #endregion
}
