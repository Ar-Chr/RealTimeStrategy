using Mirror;
using System;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText;
    private RTSPlayer player;

    private void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        ClientHandleResourcesChanged(player.Resources);
        player.ClientResourcesChanged += ClientHandleResourcesChanged;
    }

    private void OnDestroy()
    {
        player.ClientResourcesChanged -= ClientHandleResourcesChanged;
    }

    private void ClientHandleResourcesChanged(int newResources)
    {
        resourcesText.text = $"Resources: {newResources}";
    }
}
