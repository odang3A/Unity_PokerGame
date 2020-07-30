using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCard : MonoBehaviour
{
    public GameObject[] cards = new GameObject[3];

    public void setCardSprite(int i, Sprite card)
    {
        SpriteRenderer CardSprite = cards[i].GetComponent<SpriteRenderer>();
        CardSprite.sprite = card;
    }
}
