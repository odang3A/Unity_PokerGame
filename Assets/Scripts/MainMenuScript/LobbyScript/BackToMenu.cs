using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToMenu : MonoBehaviour
{
    public GameObject JoinRoomSection;
    public GameObject SelectRoomEnterType;
    
    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))        //뒤로가기
        {
            onClickBackToMenu();
        }
    }

    public void onClickBackToMenu()
    {
        JoinRoomSection.SetActive(false);
        SelectRoomEnterType.SetActive(true);
    }
}
