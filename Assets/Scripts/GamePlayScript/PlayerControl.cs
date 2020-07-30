using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public PhotonPlayer PhotonPlayer { get; private set; }
    public Text Name;
    public Text TokenText;
    private int TokenInt;
    public GameObject[] CardObj = new GameObject[3];
    public GameObject IsBetting;
    private bool die;
    private bool disable;
    public GameObject ShowDisable;
    private Text DisableType;
    public GameObject ShowWinner;
    public Sprite cardBackSprite;

    public Slider BetTimer;

    private bool readyCard;
    public bool isReadyCard(){    return readyCard;   }
    public void setReadyCard(bool b){   readyCard = b;  }

    private bool readyToBet;
    public bool isReadyToBet(){     return readyToBet;    }
    public void setReadyToBet(bool b){     readyToBet = b;  }

    private bool readyToCompare;
    public bool isReadyToCompare(){     return readyToCompare;  }
    public void setReadyToCompare(bool b){      readyToCompare = b;     }

    private bool readyToChoose;
    public bool isReadyToChoose(){      return readyToChoose;   }
    public void setReadyToChoose(bool b){       readyToChoose = b;      }

    private bool skip;
    public bool isSkip(){       return skip;    }
    public void setSkip(bool b){    skip = b;   }

    private bool readyForNext;
    public bool isReadyForNext(){   return readyForNext;    }
    public void setReadyForNext(bool b){    readyForNext = b;   }

    public void SetPlayer(PhotonPlayer photonPlayer)
    {
        PhotonPlayer = photonPlayer;
        Name.text = PhotonPlayer.NickName;
        TokenInt = 100;
        TokenText.text = TokenInt.ToString();
        DisableType = ShowDisable.transform.Find("Text").GetComponent<Text>();
    }

    public int GetTokenInt()
    {
        return TokenInt;
    }

    public void SetCard(int i, Sprite cardSprite)
    {
        SpriteRenderer CardSprite = CardObj[i].GetComponent<SpriteRenderer>();
        CardSprite.sprite = cardSprite;
    }
    
    public void moveCard()
    {
        Vector3 pos = CardObj[2].transform.localPosition;
        pos.x += 20;
        CardObj[2].transform.localPosition = pos;
    }

    public void showCard(Sprite cardSprite)
    {
        SpriteRenderer CardSprite = CardObj[2].GetComponent<SpriteRenderer>();
        CardSprite.sprite = cardSprite;
    }

    public void showRest(int i, Sprite cardSprite)
    {
        SpriteRenderer CardSprite = CardObj[i].GetComponent<SpriteRenderer>();
        CardSprite.sprite = cardSprite;
    }
    
    public void activeIsBetting(bool b)
    {
        IsBetting.SetActive(b);

        StartCoroutine(timer());
    }

    private IEnumerator timer()
    {
        BetTimer.value = BetTimer.maxValue;
        while(BetTimer.value > 0 && IsBetting.activeSelf)
        {
            yield return new WaitForSeconds(0.01f);

            BetTimer.value -= 0.001f;
        }
    }

    public bool isDie()     //다음 베팅할 사람이 다이인지 확인
    {
        return die;
    }

    public void setPlayerAsDie(bool b)
    {
        die = b;
        DisableType.text = "Die";
        ShowDisable.SetActive(die);
    }

    public bool isDisable()           //파산인지 확인
    {
        return disable;
    }

    public void setPlayerAsBroke()
    {
        disable = true;
        DisableType.text = "Broke";
        ShowDisable.SetActive(true);
    }

    public void setPlayerAsDisconnected()
    {
        disable = true;
        DisableType.text = "...";
        ShowDisable.SetActive(true);
    }

    public void playerHasBet(int b)     //베팅한 만큼 차감
    {
        TokenInt -= b;
        TokenText.text = TokenInt.ToString();
    }

    public void setPlayerAllIn()
    {
        TokenText.text = "All In";
        TokenText.color = Color.red;
    }

    public bool checkPlayerAllIn()
    {
        return TokenInt == 0;
    }

    public void WinnerActive(bool b)
    {
        ShowWinner.SetActive(b);
    }

    public void clearCard()
    {
        int i=0;
        for(i=0;i<3;i++)
        {
            SpriteRenderer CardSprite = CardObj[i].GetComponent<SpriteRenderer>();
            CardSprite.sprite = cardBackSprite;
            if(i==2)
            {
                Vector3 pos = CardObj[i].transform.localPosition;
                pos.x = 40;
                CardObj[i].transform.localPosition = pos;
            }
        }
    }

    public void ShowDeck(int[] deck, Sprite[] cardSprite)
    {
        int i=0, j=0;
        for(i=0;i<3;i++)
        {
            for(j=0;j<5;j++)
            {
                SpriteRenderer CardSprite = CardObj[i].GetComponent<SpriteRenderer>();
                if(CardSprite.sprite == cardSprite[deck[j]])
                {
                    CardSprite.color = new Color(1, 1, 1, 1);
                    break;
                }
                CardSprite.color = new Color(100/255f, 100/255f, 100/255f, 1);
            }
        }
    }

    public void HideDeck()
    {
        int i=0;
        for(i=0;i<3;i++)
        {
            SpriteRenderer CardSprite = CardObj[i].GetComponent<SpriteRenderer>();
            CardSprite.color = new Color(1, 1, 1, 1);
        }
    }

    public void tokenTextWhite()
    {
        TokenText.color = Color.white;
    }
}
