using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinAsGuest : MonoBehaviour
{
    public GameObject SignInScreen;
    public GameObject JoinAsGuestScreen;
    public Text JoinAsGuestText;
    public Button JoinAsGuestBtn;

    private void Start()
    {
        // if(Application.platform != RuntimePlatform.Android)
        // {
        //     SignInScreen.SetActive(false);
        //     JoinAsGuestScreen.SetActive(true);
        //     JoinAsGuestText.text = "";
        //     JoinAsGuestBtn.interactable = false;
        // }
    }

    public void OnClick_JoinAsGuest()
    {
        SignInScreen.SetActive(!SignInScreen.activeSelf);
        JoinAsGuestScreen.SetActive(!JoinAsGuestScreen.activeSelf);
        if(SignInScreen.activeSelf)
        {
            JoinAsGuestText.text = "Join as Guest";
        }
        else
        {
            JoinAsGuestText.text = "Sign in";
        }
    }
}
