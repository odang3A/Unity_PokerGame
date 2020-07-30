using UnityEngine;

public class CurrentRoomCanvas : MonoBehaviour
{
    public GameObject PlayerLayoutGroupObj;
    
    public void OnClickStartGame()
    {
        PlayerLayoutGroup playerLayoutGroup = PlayerLayoutGroupObj.GetComponent<PlayerLayoutGroup>();
        if(!PhotonNetwork.isMasterClient)
        {
            playerLayoutGroup.OnClickReady();
            print("Clicked ready");
        }
        else
        {
            if(playerLayoutGroup.EveryoneReady()&&PhotonNetwork.room.PlayerCount>1)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;
                PhotonNetwork.LoadLevel(2);
            }
            else
                print("Not ready yet");
        }
    }

    
}
