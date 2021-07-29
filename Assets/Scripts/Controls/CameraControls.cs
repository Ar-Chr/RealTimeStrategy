using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private float panSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private int pixelsToSideForPanning;
    [SerializeField] private Transform minPositionTransform;
    [SerializeField] private Transform maxPositionTransform;

    private Vector3 minPosition => minPositionTransform.position;
    private Vector3 maxPosition => maxPositionTransform.position;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if (!Application.isFocused)
            return;

        if (Keyboard.current.escapeKey.isPressed)
            Cursor.lockState = CursorLockMode.None;

        Vector3 panDirection = Vector2.zero;

        panDirection += GetHorizontalPan() * transform.right;
        panDirection += GetVerticalPan() * (Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.forward);

        Pan(panDirection * panSpeed * Time.deltaTime);

        Zoom(Mouse.current.scroll.ReadValue().y * zoomSpeed * Time.deltaTime);

        Clamp();
    }

    private void OnApplicationFocus(bool isFocused)
    {
        Cursor.lockState = isFocused ? CursorLockMode.Confined : CursorLockMode.None;
    }

    private int GetHorizontalPan()
    {
        return GetPan(Mouse.current.position.ReadValue().x, Screen.width);
    }

    private int GetVerticalPan()
    {
        return GetPan(Mouse.current.position.ReadValue().y, Screen.height);
    }

    private int GetPan(float checkValue, int checkAgainst)
    {
        var mousePos = Mouse.current.position.ReadValue();
        if (mousePos.x < 0 || mousePos.x > Screen.width ||
            mousePos.y < 0 || mousePos.y > Screen.height)
            return 0;

        if (0 <= checkValue && checkValue <= pixelsToSideForPanning)
            return -1;

        if (checkAgainst - pixelsToSideForPanning <= checkValue && checkValue <= checkAgainst)
            return 1;

        return 0;
    }

    private void Pan(Vector3 value)
    {
        transform.Translate(value, Space.World);
    }

    private void Zoom(float value)
    {
        Vector3 movement = transform.forward * value;
        Vector3 newPosition = transform.position + movement;

        if (newPosition.y > maxPosition.y + 1e-3)
        {
            float neededMovementPart = (maxPosition.y - transform.position.y) / movement.y;
            movement *= neededMovementPart;
        }

        if (newPosition.y < minPosition.y - 1e-3)
        {
            float neededMovementPart = (minPosition.y - transform.position.y) / movement.y;
            movement *= neededMovementPart;
        }

        transform.Translate(movement, Space.World);
    }

    private void Clamp()
    {
        Vector3 position;

        position.x = Mathf.Max(transform.position.x, minPosition.x);
        position.y = Mathf.Max(transform.position.y, minPosition.y);
        position.z = Mathf.Max(transform.position.z, minPosition.z);

        position.x = Mathf.Min(transform.position.x, maxPosition.x);
        position.y = Mathf.Min(transform.position.y, maxPosition.y);
        position.z = Mathf.Min(transform.position.z, maxPosition.z);

        transform.position = position;
    }
}
