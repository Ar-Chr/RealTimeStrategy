using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelector : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaTransform;
    [SerializeField] private LayerMask selectablesLayerMask;

    private Camera mainCamera;
    private RTSPlayer player;

    public HashSet<Unit> SelectedUnits { get; } = new HashSet<Unit>();
    public HashSet<Building> SelectedBuildings { get; } = new HashSet<Building>();

    private Vector2 selectionBoxStartPoint;
    private bool selecting;

    #region Callbacks

    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Creatable.OnAuthorityCreatableDespawned += AuthorityHandleCreatableDespawned;
        Abstract_GameEnder.PlayerDied += HandlePlayerDied;
        Abstract_GameEnder.GameEnded += HandleGameEnded;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!Keyboard.current.shiftKey.isPressed)
                DeselectAll();

            StartSelectingArea();
        }
        if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (selecting)
            {
                selectionAreaTransform.gameObject.SetActive(false);
                ResolveSelection();
            }
        }
    }

    private void OnDestroy()
    {
        Creatable.OnAuthorityCreatableDespawned -= AuthorityHandleCreatableDespawned;
        Abstract_GameEnder.PlayerDied -= HandlePlayerDied;
        Abstract_GameEnder.GameEnded -= HandleGameEnded;
    }

    #endregion

    #region Area methods

    private void StartSelectingArea()
    {
        selecting = true;
        selectionBoxStartPoint = Mouse.current.position.ReadValue();
        selectionAreaTransform.gameObject.SetActive(true);
    }

    private void UpdateSelectionArea()
    {
        if (!selecting)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - selectionBoxStartPoint.x;
        float areaHeight = mousePosition.y - selectionBoxStartPoint.y;

        selectionAreaTransform.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        selectionAreaTransform.anchoredPosition = selectionBoxStartPoint + new Vector2(areaWidth, areaHeight) / 2;
    }

    #endregion

    #region Selection methods

    private void ResolveSelection()
    {
        if (!selecting)
            return;

        selecting = false;

        if (selectionAreaTransform.sizeDelta.sqrMagnitude == 0)
            SelectSingle();
        else
            SelectAllInArea();
    }

    private void SelectSingle()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectablesLayerMask))
            return;

        if (!hit.collider.TryGetComponent<Creatable>(out var creatable))
            return;

        if (!creatable.hasAuthority)
            return;

        Select(creatable);
    }

    private void SelectAllInArea()
    {
        Vector2 min = selectionAreaTransform.anchoredPosition - selectionAreaTransform.sizeDelta / 2;
        Vector2 max = selectionAreaTransform.anchoredPosition + selectionAreaTransform.sizeDelta / 2;

        foreach (Unit unit in player.MyUnits)
        {
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if (screenPosition.x >= min.x &&
                screenPosition.x <= max.x &&
                screenPosition.y >= min.y &&
                screenPosition.y <= max.y)
                Select(unit);
        }

        foreach (Building building in player.MyBuildings)
        {
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(building.transform.position);
            if (screenPosition.x >= min.x &&
                screenPosition.x <= max.x &&
                screenPosition.y >= min.y &&
                screenPosition.y <= max.y)
                Select(building);
        }
    }

    private void Select(Creatable creatable)
    {
        if (creatable is Unit unit)
            Select(unit);

        else if (creatable is Building building)
            Select(building);
    }

    private void Select(Unit unit)
    {
        if (SelectedUnits.Contains(unit))
            return;

        SelectedUnits.Add(unit);
        unit.Selectable.Select();
    }

    private void Select(Building building)
    {
        if (SelectedBuildings.Contains(building))
            return;

        SelectedBuildings.Add(building);
        building.Selectable.Select();
    }

    private void DeselectAll()
    {
        foreach (Unit unit in SelectedUnits)
            unit.Selectable.Deselect();

        foreach (Building building in SelectedBuildings)
            building.Selectable.Deselect();

        SelectedUnits.Clear();
        SelectedBuildings.Clear();
    }

    #endregion

    #region Handlers

    private void AuthorityHandleCreatableDespawned(Creatable creatable)
    {
        if (creatable is Unit unit)
            SelectedUnits.Remove(unit);

        else if (creatable is Building building)
            SelectedBuildings.Remove(building);
    }

    private void HandlePlayerDied(RTSPlayer player)
    {
        if (!player.hasAuthority)
            return;

        DeselectAll();
        enabled = false;
    }

    private void HandleGameEnded(RTSPlayer winner)
    {
        DeselectAll();
        enabled = false;
    }

    #endregion
}
