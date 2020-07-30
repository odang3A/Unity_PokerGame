using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignInRegisterManager : MonoBehaviour
{
    public InputField SignInIdField;
    public InputField SingInpasswdField;
    public Text SignInStatus;
    public Button SignInBtn;
    public InputField RegisterIdField;
    public InputField RegisterNicknameField;
    public InputField RegisterPasswordField;
    public Text RegisterStatus;
    public Button RegisterBtn;
    private SocketIOManager socketIOManager;

    public GameObject SoRObj;
    public GameObject CPObj;    //create player

    public Text InputPlayerName;

    private string SignInAuth = null;

    private void Start()
    {
        GameObject SIOMObj = GameObject.Find("SocketIOManager");
        socketIOManager = SIOMObj.GetComponent<SocketIOManager>();
    }

    public void OnClick_SignIn()
    {
        string id = SignInIdField.text;
        string passwd = SingInpasswdField.text;
        SignInStatus.text = "";
        SignInBtn.interactable = false;
        socketIOManager.SignIn(id, passwd);
    }

    public void SignInSuccess(Dictionary<string, string> auth)
    {
        if(auth["isAble"] == "True"){
            SignInStatus.text = "Success";

            PlayerNetwork.Instance.isGuest = false;

            InputPlayerName.text = auth["mess"];

            CreatePlayer createPlayer = CPObj.GetComponent<CreatePlayer>();
            createPlayer.OnClick_CreatePlayer();
        }
        else {
            SignInStatus.text = auth["mess"];
            SignInBtn.interactable = true;
        }
    }

    public void OnClick_Register()
    {
        string id = RegisterIdField.text;
        string nick = RegisterNicknameField.text;
        string passwd = RegisterPasswordField.text;
        RegisterStatus.text = "";
        RegisterBtn.interactable = false;
        socketIOManager.Register(id, nick, passwd);
    }

    public void RegisterSuccess(Dictionary<string, string> auth)
    {
        if(auth["isAble"] == "True"){
            SignIn_or_Register SoR = SoRObj.GetComponent<SignIn_or_Register>();
            SoR.OnClick_SelectSignIn();
            SignInStatus.text = "Register Success";
        }
        else {
            RegisterStatus.text = auth["mess"];
        }
        RegisterBtn.interactable = true;
    }
}
