using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectType : MonoBehaviour
{
    public GameObject SelectRoomEnterType;
    public GameObject JoinRoomSection;
    public GameObject CreateRoomSection;

    public void OnSelect_JoinRoom()
    {
        SelectRoomEnterType.SetActive(false);
        JoinRoomSection.SetActive(true);
    }
    public void OnSelect_CreateRoom()
    {
        SelectRoomEnterType.SetActive(false);
        CreateRoomSection.SetActive(true);
    }
}
