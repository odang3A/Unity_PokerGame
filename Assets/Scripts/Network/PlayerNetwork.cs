using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


public class PlayerNetwork : MonoBehaviour
{
    public static PlayerNetwork Instance;
    public string PlayerName;
    public Text InputPlayerName;
    private PhotonView PhotonView;
    private int PlayersInGame = 0;
    private GameObject GameControl;

    public bool isGuest = true;

    public char User;

    private void Awake()
    {
        Instance = this;
        
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public void setPlayerNetwork()
    {
        PlayerName = InputPlayerName.text;

        PhotonView = GetComponent<PhotonView>();
    }

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "GamePlayScene")
        {
            if(PhotonNetwork.isMasterClient)
                MasterLoadedGame();
            else
                NonMasterLoadedGame();
        }
    }

    private void MasterLoadedGame()
    {
        PhotonView.RPC("RPC_LoadedGameScene", PhotonTargets.MasterClient, PhotonNetwork.player);
        PhotonView.RPC("RPC_LoadGameOthers", PhotonTargets.Others);
    }

    private void NonMasterLoadedGame()
    {
        PhotonView.RPC("RPC_LoadedGameScene", PhotonTargets.MasterClient, PhotonNetwork.player);
    }

    [PunRPC]
    private void RPC_LoadGameOthers()
    {
        PhotonNetwork.LoadLevel(2);
    }

    [PunRPC]
    private void RPC_LoadedGameScene(PhotonPlayer photonPlayer)
    {
        PlayersInGame++;
        if(PlayersInGame == PhotonNetwork.playerList.Length)
        {
            print("All players are in the game scene");
            PhotonView.RPC("RPC_StartGame", PhotonTargets.All);
            PlayersInGame = 0;
        }
    }

    [PunRPC]
    private void RPC_StartGame()
    {
        GameControl = GameObject.Find("GameControl");
        GameControl gameControl = GameControl.GetComponent<GameControl>();
        gameControl.StartGame();
    }
}
