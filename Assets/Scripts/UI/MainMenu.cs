using Mirror;
using UnityEngine;
using Steamworks;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPage;

    private RTSNetworkManager networkManager;

    private bool useSteam => networkManager.UseSteam;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Start()
    {
        networkManager = (RTSNetworkManager)NetworkManager.singleton;

        if (useSteam)
        {
            lobbyCreated = Callback<LobbyCreated_t>.Create(HandleLobbyCreated);
            lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(HandleLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(HandleLobbyEntered);
        }
    }

    public void HostLobby()
    {
        landingPage.SetActive(false);

        if (useSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        }
        else
        {
            NetworkManager.singleton.StartHost();
        }
    }

    private void HandleLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            landingPage.SetActive(true);
            return;
        }

        NetworkManager.singleton.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "HostAddress",
            SteamUser.GetSteamID().ToString());
    }

    private void HandleLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void HandleLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active)
            return;

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "HostAddress");

        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();

        landingPage.SetActive(false);
    }
}
