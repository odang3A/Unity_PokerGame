using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCanvas : MonoBehaviour
{
    public GameObject AudioManagerObj;

    [SerializeField]
    private RoomLayoutGroup _roomLayoutGroup;
    private RoomLayoutGroup RoomLayoutGroup
    {
        get { return _roomLayoutGroup; }
    }

    public void OnClickJoinRoom(string roomName)
    {
        AudioManager audioManager = AudioManagerObj.GetComponent<AudioManager>();
        audioManager.Play_Click_Sound();

        if(PhotonNetwork.JoinRoom(roomName))
        {

        }
        else
        {
            print("Join room failed");
        }
    }
}
