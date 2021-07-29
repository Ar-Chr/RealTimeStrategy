using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamColorImage : Abstract_TeamColor
{
    [SerializeField] private Image image;

    public void Start()
    {
        image.color = teamColor;
    }
}
