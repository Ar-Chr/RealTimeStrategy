using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [Space(height: 16, order = 0)]
    [Header("RTSNetworkManager Fields", order = 1)]
    [Space(height: 4, order = 2)]
    [SerializeField] private bool useSteam;
    [SerializeField][Range(1, 4)] private int minimumPlayersToStart;
    [Space]
    [SerializeField] private GameObject mainBuildingPrefab;
    [SerializeField] private Abstract_GameEnder gameEnder;
    [SerializeField] private Color[] teamColors;

    public bool UseSteam => useSteam;

    private List<RTSPlayer> players = new List<RTSPlayer>();
    public IReadOnlyList<RTSPlayer> Players => players.AsReadOnly();

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool gameInProgress;

    public void AddPlayer(RTSPlayer player) => players.Add(player);
    public void RemovePlayer(RTSPlayer player) => players.Remove(player);

    private CSteamID lobbyId;

    #region Steam Callbacks

    protected Callback<LobbyCreated_t> lobbyCreated;

    #endregion

    #region Server

    public override void OnStartServer()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(HandleLobbyCreated);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (gameInProgress)
            conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        Debug.Log($"Player with connection id {conn.connectionId} connected. Now have {numPlayers}/{maxConnections} players.");
        
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        players.Add(player);

        SetPlayerName(player);
        player.SetColor(teamColors[numPlayers - 1]);

        player.SetPartyOwner(players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Map_"))
        {
            Abstract_GameEnder gameEnderInstance = Instantiate(gameEnder);
            NetworkServer.Spawn(gameEnderInstance.gameObject);

            foreach (RTSPlayer player in players)
            {
                SpawnPlayerStartingArmy(player.connectionToClient);
            }
        }
    }

    public override void OnStopServer()
    {
        players.Clear();

        gameInProgress = false;
    }

    public void StartGame()
    {
        if (players.Count < minimumPlayersToStart)
            return;

        gameInProgress = true;

        ServerChangeScene("Map_Test");
    }

    private void SetPlayerName(RTSPlayer player)
    {
        StartCoroutine(TrySetSteamName(player));
    }

    private IEnumerator TrySetSteamName(RTSPlayer player)
    {
        string steamName;

        do
        {
            if (numPlayers == 0)
                steamName = SteamFriends.GetPersonaName().ToString();
            else
                steamName = SteamFriends.GetFriendPersonaName(
                    SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, numPlayers));

            if (steamName == "")
                yield return new WaitForSecondsRealtime(0.2f);
        } while (steamName == "");

        player.SetName(steamName);
    }

    private void HandleLobbyCreated(LobbyCreated_t callback)
    {
        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
    }

    [Server]
    private void SpawnPlayerStartingArmy(NetworkConnection playerConnection)
    {
        Transform startPosition = GetStartPosition();

        var mainBuildingInstance = Instantiate(
            mainBuildingPrefab,
            startPosition.position,
            startPosition.rotation);

        NetworkServer.Spawn(mainBuildingInstance, playerConnection);
    }

    #endregion

    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        players.Clear();
    }

    #endregion
}
