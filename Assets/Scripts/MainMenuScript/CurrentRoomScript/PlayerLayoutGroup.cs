using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLayoutGroup : MonoBehaviour
{
    private PhotonView PhotonView;
    public Text GameStartText;
    public Text CloseRoomText;
    private bool ready = false;

    [SerializeField]
    private GameObject _playerListingPrefab;
    private GameObject PlayerListingPrefab
    {
        get { return _playerListingPrefab; }
    }

    private List<PlayerListing> _playerListings = new List<PlayerListing>();
    private List<PlayerListing> PlayerListings
    {
        get { return _playerListings; }
    }

    private void Update()
    {
        if(Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKey(KeyCode.Escape))        //뒤로가기
            {
                OnClickLeaveRoom();
            }
        }
    }

    //Called by photon whenever the master client is switched.
    private void OnMasterClientSwitched(PhotonPlayer newMasterClient)       //마스터 나가서 바뀌면
    {
        if(PhotonNetwork.player == newMasterClient)
        {
            GameStartText.text = "Start Game";
            GameStartText.color = Color.black;
            ready = false;
        }

        int index = PlayerListings.FindIndex(x => x.PhotonPlayer == newMasterClient);
        PlayerListings[index].ApplyPhotonPlayer(newMasterClient);
        PlayerListings[index].SetReady(false);
    }

    //Called by photon whenever you join a room.
    private void OnJoinedRoom()
    {
        PhotonView = PhotonView.Get(this);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        ready = false;
        GameStartText.color = Color.black;

        MainCanvasManager.Instance.CurrentRoomCanvas.transform.SetAsLastSibling();
        if(!PhotonNetwork.isMasterClient)
        {
            GameStartText.text = "Ready";
        }
        else
        {
            GameStartText.text = "Start Game";
        }

        PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
        int i=0;
        for(i=0;i<photonPlayers.Length;i++)
        {
            PlayerJoinedRoom(photonPlayers[i]);
        }
        PhotonView.RPC("RPC_UpdateByMaster", PhotonTargets.MasterClient, PhotonNetwork.player);
    }

    //Called by photon whenever you join the room.
    private void OnPhotonPlayerConnected(PhotonPlayer photonPlayer)
    {
        PlayerJoinedRoom(photonPlayer);

    }

    //Called by photon when a player leaves the room.
    private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
    {
        PlayerLeftRoom(photonPlayer);
    }

    private void PlayerJoinedRoom(PhotonPlayer photonPlayer)
    {
        if(photonPlayer == null)
            return;

        //preventing whenever a player join room messages is sent when the player is already in the room.
        PlayerLeftRoom(photonPlayer);

        GameObject playerListingObj = Instantiate(PlayerListingPrefab);
        playerListingObj.transform.SetParent(transform, false);

        PlayerListing playerListing = playerListingObj.GetComponent<PlayerListing>();
        playerListing.ApplyPhotonPlayer(photonPlayer);

        PlayerListings.Add(playerListing);
    }

    private void PlayerLeftRoom(PhotonPlayer photonPlayer)
    {
        int index = PlayerListings.FindIndex(x => x.PhotonPlayer == photonPlayer);
        if(index != -1)
        {
            Destroy(PlayerListings[index].gameObject);
            PlayerListings.RemoveAt(index);
        }
    }

    public void OnClick_CloseRoom()
    {
        if(PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.room.IsOpen = !PhotonNetwork.room.IsOpen;
            PhotonNetwork.room.IsVisible = PhotonNetwork.room.IsOpen;

            PhotonView.RPC("RPC_SetRoomState", PhotonTargets.All);
        }
    }

    [PunRPC]
    private void RPC_SetRoomState()
    {
        if(!PhotonNetwork.room.IsOpen)
        {
            CloseRoomText.color = Color.red;
            CloseRoomText.text = "Room State: Closed";
        }
        else if(PhotonNetwork.room.IsOpen)
        {
            CloseRoomText.color = Color.blue;
            CloseRoomText.text = "RoomState: Opened";
        }
    }

    public void OnClickLeaveRoom()
    {
        _playerListings = new List<PlayerListing>();
        PhotonNetwork.LeaveRoom();
    }

    private void OnLeftRoom()
    {
        MainCanvasManager.Instance.CurrentRoomCanvas.transform.SetAsFirstSibling();
    }

    public bool EveryoneReady()
    {
        int cnt = 0;
        foreach (PlayerListing playerListing in PlayerListings)
        {
            if(playerListing.GetReady())
                cnt++;
        }
        return cnt == PhotonNetwork.room.PlayerCount - 1;
    }

    public void OnClickReady()
    {
        ready = !ready;
        PhotonView.RPC("RPC_UpdateReady", PhotonTargets.All, PhotonNetwork.player, ready);
        if(ready)
            GameStartText.color = Color.green;
        else
            GameStartText.color = Color.black;
        
    }

    [PunRPC]
    private void RPC_UpdateReady(PhotonPlayer photonPlayer, bool Ready)
    {
        int index = PlayerListings.FindIndex(x => x.PhotonPlayer == photonPlayer);
        PlayerListings[index].SetReady(Ready);
        PlayerListings[index].ShowReady();
    }

    [PunRPC]
    private void RPC_UpdateByMaster(PhotonPlayer reqPlayer)
    {
        PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
        int i=0;
        for(i=0;i<photonPlayers.Length;i++)
        {
            if(photonPlayers[i].isMasterClient)
                continue;
            int index = PlayerListings.FindIndex(x => x.PhotonPlayer == photonPlayers[i]);
            PhotonView.RPC("RPC_UpdateReady", reqPlayer , photonPlayers[i], PlayerListings[index].GetReady());
        }
    }
}
