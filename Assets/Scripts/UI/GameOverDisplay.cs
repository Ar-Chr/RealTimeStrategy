using TMPro;
using Mirror;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent;
    [SerializeField] private TMP_Text winnerNameText;

    private void Start()
    {
        Abstract_GameEnder.GameEnded += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Abstract_GameEnder.GameEnded -= ClientHandleGameOver;
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(RTSPlayer winner)
    {
        winnerNameText.text = $"{winner.Name} wins";
        gameOverDisplayParent.SetActive(true);
    }
}
