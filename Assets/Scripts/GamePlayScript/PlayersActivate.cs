using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersActivate : MonoBehaviour
{
    public GameObject[] Players = new GameObject[8];

    public GameObject PlayerManagement;
    private PlayerManagement playerManagement;

    private void Awake()
    {
        playerManagement = PlayerManagement.GetComponent<PlayerManagement>();
    }

    public void Activate()
    {
        int playerCnt = PhotonNetwork.room.playerCount;
        switch(playerCnt)
        {
            case 2:
                Players[1].SetActive(false);
                Players[2].SetActive(false);
                Players[3].SetActive(false);
                Players[5].SetActive(false);
                Players[6].SetActive(false);
                Players[7].SetActive(false);
                break;
            case 3:
                Players[1].SetActive(false);
                Players[3].SetActive(false);
                Players[4].SetActive(false);
                Players[5].SetActive(false);
                Players[7].SetActive(false);
                break;
            case 4:
                Players[1].SetActive(false);
                Players[3].SetActive(false);
                Players[5].SetActive(false);
                Players[7].SetActive(false);
                break;
            case 5:
                Players[2].SetActive(false);
                Players[4].SetActive(false);
                Players[6].SetActive(false);
                break;
            case 6:
                Players[3].SetActive(false);
                Players[5].SetActive(false);
                break;
            case 7:
                Players[4].SetActive(false);
                break;
            case 8:
                break;
            default:
                break;
        }

        playerManagement.LinkUpdate();
    }
}
