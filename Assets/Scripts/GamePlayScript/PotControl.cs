using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotControl : MonoBehaviour
{
    private int potInt;
    private Text potText;
    private int currBet;

    private void Start()
    {
        potText = GetComponent<Text>();
        currBet = 0;
    }

    public void AddToPot(int b)
    {
        potInt += b;
        potText.text = "Pot: " + potInt.ToString();
    }

    public int GetCurrBet()
    {
        return currBet;
    }

    public void SetCurrBetToZero()
    {
        currBet = 0;
    }

    public void RaiseBetTo(int b)
    {
        currBet = b;
    }

    public int getPot()
    {
        return potInt;
    }

    public void initPot()
    {
        potInt = 0;
        currBet = 0;
        potText.text = "Pot: " + potInt.ToString();
    }
}
