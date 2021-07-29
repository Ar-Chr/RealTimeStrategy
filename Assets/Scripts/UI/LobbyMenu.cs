using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUi;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text[] playerTexts;
    [Space]
    [SerializeField] private string emptySlotName;
    [SerializeField] private Color emptySlotColor;

    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += ClientHandleConnected;

        RTSPlayer.AuthorityPartyOwnerChanged += AuthorityHandlePartyOwnerChanged;
        RTSPlayer.ClientNameChanged += ClientHandleNameChanged;
        RTSPlayer.ClientColorChanged += ClientHandleColorChanged;

        foreach (var playerText in playerTexts)
        {
            playerText.text = emptySlotName;
            playerText.color = emptySlotColor;
        }
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= ClientHandleConnected;

        RTSPlayer.AuthorityPartyOwnerChanged -= AuthorityHandlePartyOwnerChanged;
        RTSPlayer.ClientNameChanged -= ClientHandleNameChanged;
        RTSPlayer.ClientColorChanged -= ClientHandleColorChanged;
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    private void ClientHandleConnected()
    {
        lobbyUi.SetActive(true);
    }

    private void AuthorityHandlePartyOwnerChanged(bool isPartyOwner)
    {
        startGameButton.gameObject.SetActive(isPartyOwner);
    }

    private void ClientHandleNameChanged()
    {
        var players = ((RTSNetworkManager)NetworkManager.singleton).Players;

        for (int i = 0; i < players.Count; i++)
        {
            playerTexts[i].text = players[i].Name;
        }
        for (int i = players.Count; i < playerTexts.Length; i++)
        {
            playerTexts[i].text = emptySlotName;
        }
    }

    private void ClientHandleColorChanged()
    {
        var players = ((RTSNetworkManager)NetworkManager.singleton).Players;

        for (int i = 0; i < players.Count; i++)
        {
            playerTexts[i].color = players[i].TeamColor;
        }
        for (int i = players.Count; i < playerTexts.Length; i++)
        {
            playerTexts[i].color = emptySlotColor;
        }
    }
}
