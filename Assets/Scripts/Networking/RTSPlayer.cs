using GD.MinMaxSlider;
using Mirror;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Building[] buildings;
    [Space]
    [SerializeField] private int initialResources;
    [SerializeField] private LayerMask buildingBlockLayerMask;
    [MinMaxSlider(0, 256)]
    [SerializeField] private Vector2 minMaxRange;


    [SyncVar(hook = nameof(ClientHandleResourcesChanged))]
    private int resources;

    [SyncVar(hook = nameof(ClientHandleNameChanged))]
    private string playerName;
    [SyncVar(hook = nameof(ClientHandleColorChanged))]
    private Color teamColor;
    private bool isPartyOwner;

    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public int Resources => resources;
    public string Name => playerName;
    public Color TeamColor => teamColor;
    public bool IsPartyOwner => isPartyOwner;

    public ReadOnlyCollection<Unit> MyUnits => myUnits.AsReadOnly();
    public ReadOnlyCollection<Building> MyBuildings => myBuildings.AsReadOnly();


    public event Action<int> ClientResourcesChanged;
    public static event Action<bool> AuthorityPartyOwnerChanged;
    public static event Action ClientNameChanged;
    public static event Action ClientColorChanged;

    #region Server

    #region Callbacks

    public override void OnStartServer()
    {
        DontDestroyOnLoad(gameObject);

        Creatable.OnServerCreatableSpawned += ServerHandleCreatableSpawned;
        Creatable.OnServerCreatableDespawned += ServerHandleCreatableDespawned;

        resources = initialResources;
    }

    public override void OnStopServer()
    {
        Creatable.OnServerCreatableSpawned -= ServerHandleCreatableSpawned;
        Creatable.OnServerCreatableDespawned -= ServerHandleCreatableDespawned;
    }

    #endregion

    #region Handlers

    private void ServerHandleCreatableSpawned(Creatable creatable)
    {
        if (creatable.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        if (creatable is Unit unit)
            myUnits.Add(unit);

        else if (creatable is Building building)
            myBuildings.Add(building);
    }

    private void ServerHandleCreatableDespawned(Creatable creatable)
    {
        if (creatable.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        if (creatable is Unit unit)
            myUnits.Remove(unit);

        else if (creatable is Building building)
            myBuildings.Remove(building);
    }

    #endregion

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner)
            return;

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    #region Player identity/display

    [Server]
    public void SetName(string newName)
    {
        playerName = newName;
    }

    [Server]
    public void SetColor(Color newColor)
    {
        teamColor = newColor;
    }

    [Server]
    public void SetPartyOwner(bool value)
    {
        isPartyOwner = value;
        TargetSetPartyOwner(value);
    }

    #endregion

    #region Resource management

    [Server]
    public void AddResources(int value)
    {
        resources += value;
    }

    [Server]
    public void SubtractResources(int value)
    {
        resources -= value;
    }

    #endregion

    #region Building placement

    [Command]
    public void CmdTryPlaceBuilding(int id, Vector3 position)
    {
        Building buildingToPlace = GetBuildingById(id);
        if (buildingToPlace == null)
            return;

        if (resources < buildingToPlace.Cost)
            return;

        if (!CanPlace(buildingToPlace.GetComponent<BoxCollider>(), position))
            return;

        SubtractResources(buildingToPlace.Cost);

        GameObject buildingInstance =
            Instantiate(buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);

        NetworkServer.Spawn(buildingInstance, connectionToClient);
    }

    private Building GetBuildingById(int id)
    {
        return buildings.First(b => b.Id == id);
    }

    public bool CanPlace(BoxCollider collider, Vector3 position)
    {
        if (Physics.CheckBox(
            position + collider.bounds.center,
            collider.size / 2,
            Quaternion.identity,
            buildingBlockLayerMask))
            return false;

        bool withinMaxRange = false;
        foreach (Building building in myBuildings)
        {
            float sqrDistance = (building.transform.position - position).sqrMagnitude;
            if (sqrDistance < minMaxRange.x * minMaxRange.x)
                return false;

            if (sqrDistance <= minMaxRange.y * minMaxRange.y)
                withinMaxRange = true;
        }

        if (!withinMaxRange)        
            return false;

        return true;
    }

    #endregion

    #endregion

    #region Client

    #region Callbacks

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        if (!NetworkServer.active)
            ((RTSNetworkManager)NetworkManager.singleton).AddPlayer(this);
    }

    public override void OnStartAuthority()
    {
        if (!NetworkServer.active)
        {
            Creatable.OnAuthorityCreatableSpawned += AuthorityHandleCreatableSpawned;
            Creatable.OnAuthorityCreatableDespawned += AuthorityHandleCreatableDespawned;
        }
    }

    public override void OnStopClient()
    {
        if (isClientOnly && hasAuthority)
        {
            Creatable.OnAuthorityCreatableSpawned -= AuthorityHandleCreatableSpawned;
            Creatable.OnAuthorityCreatableDespawned -= AuthorityHandleCreatableDespawned;
        }

        if (!NetworkServer.active)
            ((RTSNetworkManager)NetworkManager.singleton).RemovePlayer(this);
    }

    #endregion

    [TargetRpc]
    private void TargetSetPartyOwner(bool value)
    {
        AuthorityPartyOwnerChanged?.Invoke(value);
    }

    #region Handlers

    private void AuthorityHandleCreatableSpawned(Creatable creatable)
    {
        if (creatable is Unit unit)
            myUnits.Add(unit);

        else if (creatable is Building building)
            myBuildings.Add(building);
    }

    private void AuthorityHandleCreatableDespawned(Creatable creatable)
    {
        if (creatable is Unit unit)
            myUnits.Remove(unit);

        else if (creatable is Building building)
            myBuildings.Remove(building);
    }

    private void ClientHandleResourcesChanged(int oldValue, int newValue)
    {
        ClientResourcesChanged?.Invoke(newValue);
    }

    private void ClientHandleNameChanged(string oldName, string newName)
    {
        ClientNameChanged?.Invoke();
    }

    private void ClientHandleColorChanged(Color oldColor, Color newColor)
    {
        ClientColorChanged?.Invoke();
    }

    #endregion

    #endregion
}
