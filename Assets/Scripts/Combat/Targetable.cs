using Mirror;
using UnityEngine;

public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint;

    public Transform AimAtPoint => aimAtPoint;
}
