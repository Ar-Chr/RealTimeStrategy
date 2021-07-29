using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private LayerMask floorMask;

    private Camera mainCamera;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer previewRendererInstance;
    private BoxCollider buildingCollider;

    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Initialize();
    }

    private void Update()
    {
        if (buildingPreviewInstance != null)
            UpdateBuildingPreview();
    }

    private void Initialize()
    {
        if (building == null)
            return;

        iconImage.sprite = building.Icon;
        priceText.text = building.Cost.ToString();
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            if (!buildingPreviewInstance.activeSelf)
                buildingPreviewInstance.SetActive(true);

            buildingPreviewInstance.transform.position = hit.point;

            Material[] materials = previewRendererInstance.materials;
            if (player.CanPlace(buildingCollider, hit.point))
            {
                foreach (Material material in materials)
                    material.color = Color.green;
            }
            else
            {
                foreach (Material material in materials)
                    material.color = Color.red;
            }
        }
        else
        {
            if (buildingPreviewInstance.activeSelf)
                buildingPreviewInstance.SetActive(false);
        }
        if (Mouse.current.rightButton.wasPressedThisFrame)
            Debug.Break();
    }

    public void SetBuilding(Building building)
    {
        this.building = building;
        Initialize();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        buildingPreviewInstance = Instantiate(building.Preview);
        previewRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        buildingCollider = building.GetComponent<BoxCollider>();

        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (buildingPreviewInstance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            player.CmdTryPlaceBuilding(building.Id, hit.point);
        }

        Destroy(buildingPreviewInstance);
    }
}
