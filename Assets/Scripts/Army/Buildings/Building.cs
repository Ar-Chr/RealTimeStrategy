using System;
using Mirror;
using UnityEngine;

public class Building : Creatable
{
    [Header(nameof(Building) + " Fields")]
    [SerializeField] private GameObject preview;
    public GameObject Preview => preview;

    [SerializeField] private int id = -1;
    public int Id => id;

    public Targeter Targeter { get; private set; }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        Targeter = GetComponent<Targeter>();
    }
}
