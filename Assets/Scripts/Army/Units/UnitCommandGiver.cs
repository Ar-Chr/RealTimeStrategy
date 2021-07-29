using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private UnitSelector selector;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        Abstract_GameEnder.PlayerDied += HandlePlayerDied;
        Abstract_GameEnder.GameEnded += HandleGameEnded;
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
                return;

            if (selector.SelectedUnits.Count > 0)
            {
                if (hit.collider.TryGetComponent(out Targetable target))
                {
                    if (!target.hasAuthority)
                    {
                        Target(target);
                        return;
                    }
                }
                TryMove(hit.point);
            }
            else if (selector.SelectedBuildings.Count > 0)
            {
                foreach (Building building in selector.SelectedBuildings)
                {
                    if (building.TryGetComponent<UnitSpawner>(out var spawner))
                        spawner.SetGatherPoint(hit.point);
                }
            }
        }
    }

    private void OnDestroy()
    {
        Abstract_GameEnder.PlayerDied -= HandlePlayerDied;
        Abstract_GameEnder.GameEnded -= HandleGameEnded;
    }

    private bool TryMove(Vector3 destination)
    {
        bool everyoneMoved = true;
        foreach (Unit unit in selector.SelectedUnits)
        {
            everyoneMoved &= unit.Movement.TryMove(destination);
        }

        return everyoneMoved;
    }

    private void Target(Targetable target)
    {
        foreach (Unit unit in selector.SelectedUnits)
        {
            unit.Targeter.CmdSetTarget(target);
        }
    }

    private void SetGatherPoint(Vector3 point)
    {
        foreach (Unit unit in selector.SelectedUnits)
        {
            if (unit.TryGetComponent<UnitSpawner>(out var spawner))
                spawner.SetGatherPoint(point);
        }

        foreach (Building building in selector.SelectedBuildings)
        {
            if (building.TryGetComponent<UnitSpawner>(out var spawner))
                spawner.SetGatherPoint(point);
        }
    }

    private void HandlePlayerDied(RTSPlayer player)
    {
        if (!player.hasAuthority)
            return;

        enabled = false;
    }

    private void HandleGameEnded(RTSPlayer winner)
    {
        enabled = false;
    }
}
