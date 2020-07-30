using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelCreateRoom : MonoBehaviour
{
    public GameObject CreateRoomSection;
    public GameObject SelectRoomEnterType;

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))        //뒤로가기
        {
            OnClick_CancelCreateRoom();
        }

    }

    public void OnClick_CancelCreateRoom()
    {
        CreateRoomSection.SetActive(false);
        SelectRoomEnterType.SetActive(true);
    }
}
