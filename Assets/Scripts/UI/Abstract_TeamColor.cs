using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Abstract_TeamColor : MonoBehaviour
{
    private RTSPlayer player;

    protected Color teamColor
    {
        get
        {
            if (player == null)
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

            return player.TeamColor;
        }
    }
}
