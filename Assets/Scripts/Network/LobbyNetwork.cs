using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyNetwork : MonoBehaviour
{
    public GameObject Connecting;
    public GameObject SelectRoomEnterType;
    public GameObject JoinRoomSection;
    public GameObject CreateRoomSection;

    public Text RoomCntText;
    public Text PlayerCntText;

    private void Start()
    {
        if(PhotonNetwork.connected)
            return;
        print("Connecting to server..");
        PhotonNetwork.ConnectUsingSettings("2.0.0");
    }

    private void OnConnectedToMaster()
    {
        print("Connected to master.");
        PhotonNetwork.automaticallySyncScene = false;
        PhotonNetwork.playerName = PlayerNetwork.Instance.PlayerName 
                + ((PlayerNetwork.Instance.isGuest) ? " (g)": "");

        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    private void OnJoinedLobby()
    {
        print("Joined lobby.");
        Connecting.SetActive(false);
        SelectRoomEnterType.SetActive(true);


        if(!PhotonNetwork.inRoom)
        {
            MainCanvasManager.Instance.LobbyCanvas.transform.SetAsLastSibling();
            JoinRoomSection.SetActive(false);
            CreateRoomSection.SetActive(false);

            StartCoroutine(CntUpdate());
        }
    }

    private IEnumerator CntUpdate()
    {
        while(!PhotonNetwork.inRoom)
        {
            RoomCntText.text = "RoomCnt: " + PhotonNetwork.countOfRooms.ToString();
            PlayerCntText.text = "PlayerCnt: " + PhotonNetwork.countOfPlayers.ToString();
            yield return new WaitForSeconds(5f);
        }
    }

    private void OnDisconnectedFromPhoton()
    {
        Text NetworkError = Connecting.GetComponent<Text>();
        NetworkError.color = Color.red;
        NetworkError.text = "Network Error";
        
        SelectRoomEnterType.SetActive(false);
        JoinRoomSection.SetActive(false);
        CreateRoomSection.SetActive(false);

        Connecting.SetActive(true);
        print("Dis form photon services");
        StartCoroutine(Reconnect());
    }

    private IEnumerator Reconnect()
    {
        int t=5;
        Text ReconnectText = Connecting.transform.Find("ReconnectText").GetComponent<Text>();

        while(t>=0)
        {
            ReconnectText.text = "Reconnection attempt in " + t;
            yield return new WaitForSeconds(1f);
            t--;
        }

        SceneManager.LoadScene(1);
    }
}
