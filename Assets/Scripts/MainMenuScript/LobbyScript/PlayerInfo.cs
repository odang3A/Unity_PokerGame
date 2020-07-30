using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public Text PlayerNameText;
    public Text PlayerTokensText;
    public GameObject levelObj;
    public Text PlayerLevelText;
    public Slider PlayerExpSlider;
    
    private SocketIOManager socketIOManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject SIOMObj = GameObject.Find("SocketIOManager");
        socketIOManager = SIOMObj.GetComponent<SocketIOManager>();

        if(!PlayerNetwork.Instance.isGuest)
        {
            socketIOManager.playerStats(PlayerNetwork.Instance.PlayerName);
        }
        
        PlayerNameText.text = PlayerNetwork.Instance.PlayerName;
    }

    public void setPlayerStats(JSONObject stats)
    {
        levelObj.SetActive(true);
        PlayerTokensText.text = "Chips: " + stats["tokens"];
        PlayerLevelText.text = "Lv." + stats["level"];
        int i=10, level=1, ci=0;
        for(i=10, level=1;int.Parse(stats["exp"].ToString())-i>=0;i+=(i+5)) ci = i;
        PlayerExpSlider.maxValue = i;
        PlayerExpSlider.minValue = ci;
        PlayerExpSlider.value = int.Parse(stats["exp"].ToString().Trim('"'));
    }
}
