using UnityEngine;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviour
{
    public PhotonPlayer PhotonPlayer { get; private set; }
    public GameObject ReadyTextObj;
    public GameObject Kick;

    private bool Ready = false;

    public bool GetReady()
    {
        return Ready;
    }

    public void SetReady(bool ready)
    {
        Ready = ready;
    }

    [SerializeField]
    private Text _playerName;
    private Text PlayerName
    {
        get { return _playerName; }
    }

    public void ApplyPhotonPlayer(PhotonPlayer photonPlayer)
    {
        Text ReadyText = ReadyTextObj.GetComponent<Text>();
        PhotonPlayer = photonPlayer;
        PlayerName.text = PhotonPlayer.NickName;
        if(photonPlayer.isMasterClient)
        {
            ReadyText.text = "Master";
            ReadyTextObj.SetActive(true);
        }
    }

    public void ShowReady()
    {
        ReadyTextObj.SetActive(Ready);
    }

    public void OnClickPlayer()
    {
        if(PhotonNetwork.isMasterClient && !PhotonPlayer.isMasterClient)
        {
            Kick.SetActive(true);
        }
    }

    public void OnClick_KickBtn()
    {
        PhotonNetwork.CloseConnection(PhotonPlayer);
    }

    public void OnClick_Cancle()
    {
        Kick.SetActive(false);
    }
}
