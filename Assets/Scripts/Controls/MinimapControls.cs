using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MinimapControls : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private Camera minimapCamera;
    
    private float mapScale => minimapCamera.orthographicSize;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            mousePos,
            null,
            out Vector2 localPoint))
        {
            return;
        }

        Vector2 lerp = new Vector2(
            localPoint.x / minimapRect.rect.width,
            localPoint.y / minimapRect.rect.height);

        Vector3 worldPoint = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            0,
            Mathf.Lerp(-mapScale, mapScale, lerp.y))
            + minimapCamera.transform.position.WithY(0);

        float zOffset = mainCamera.transform.position.y /
            Mathf.Tan(mainCamera.transform.eulerAngles.x * Mathf.Deg2Rad);

        Vector3 offset = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * Vector3.back * zOffset;

        Vector3 newCameraPosition = worldPoint
            .WithY(mainCamera.transform.position.y) +
            offset;

        mainCamera.transform.position = newCameraPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            MoveCamera();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            MoveCamera();
        }
    }
}
