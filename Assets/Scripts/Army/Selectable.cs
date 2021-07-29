using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Selectable : NetworkBehaviour
{
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    [Client]
    public void Select()
    {
        if (!hasAuthority)
            return;

        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority)
            return;

        onDeselected?.Invoke();
    }
}
