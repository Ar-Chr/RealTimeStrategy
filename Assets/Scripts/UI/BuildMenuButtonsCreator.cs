using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuButtonsCreator : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private BuildingButton buildingButtonPrefab;
    [Space]
    [SerializeField] private List<Building> availableBuildings;

    private void Awake()
    {
        BuildingButton[] existingButtons =
            transform.GetComponentsInChildren<BuildingButton>();

        int i = 0;
        foreach (BuildingButton button in existingButtons)
        {
            button.SetBuilding(availableBuildings[i]);
            i++;
        }
        for (; i < availableBuildings.Count; i++)
        {
            Instantiate(buildingButtonPrefab, grid.transform)
                .SetBuilding(availableBuildings[i]);
        }
    }
}
