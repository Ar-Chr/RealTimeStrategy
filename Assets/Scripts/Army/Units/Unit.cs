using Mirror;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Unit : Creatable
{
    [SerializeField] private UnitMovement movement;
    [SerializeField] private Targeter targeter;

    public UnitMovement Movement => movement;
    public Targeter Targeter => targeter;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }
}
