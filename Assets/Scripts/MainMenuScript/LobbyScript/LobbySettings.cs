using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySettings : MonoBehaviour
{
    public GameObject SettingsMenu;
    public GameObject Blur;
    public GameObject SettingsScreen;

    private GameObject DDOL;

    private void Start()
    {
        SettingsMenu.SetActive(false);
        Blur.SetActive(false);
    }
    
    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))        //뒤로가기
        {
            OnClick_SettingsMenuBtn();
        }
    }

    public void OnClick_SettingsMenuBtn()
    {
        SettingsMenu.SetActive(!SettingsMenu.activeSelf);
        Blur.SetActive(SettingsMenu.activeSelf);
    }

    public void OnClick_MainSettings()
    {
        SettingsScreen.SetActive(true);
    }

    public void OnClick_SignOut()
    {
        PhotonNetwork.Disconnect();

        GameObject SIOMObj = GameObject.Find("SocketIOManager");
        SocketIOManager SIOM = SIOMObj.GetComponent<SocketIOManager>();
        SIOM.SignOutFromMenu();
        //  PlayerNetwork.Instance.firebaseAuth.SignOut();

        DDOL = GameObject.Find("DDOL");
        Destroy(DDOL);

        SceneManager.LoadScene(0);
    }

    public void OnClick_Resume()
    {
        SettingsMenu.SetActive(false);
        Blur.SetActive(false);
    }

    public void OnClick_QuitGame()
    {
        Application.Quit();
    }
}
