using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerManagement : MonoBehaviour
{
    public GameObject Players;
    private List<GameObject> PlayerObjList;

    public void LinkUpdate()
    {
        PlayersActivate playersActivate = Players.GetComponent<PlayersActivate>();
        PlayerObjList = new List<GameObject>(playersActivate.Players);
        LinkPlayer();
    }

    private void LinkPlayer()
    {
        PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
        Array.Sort(photonPlayers);
        PlayerControl playerStat;
        int n = photonPlayers.Length;
        int i=0, j=0, p=0;
        for(p=0;photonPlayers[p]!=PhotonNetwork.player;p++);
        j = p;
        for(i=0;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                playerStat = PlayerObjList[i].GetComponent<PlayerControl>();
                for(;photonPlayers[j]==null;j=(j+1)%n);
                playerStat.SetPlayer(photonPlayers[j]);
                j=(j+1)%n;
            }
        }
    }
}
