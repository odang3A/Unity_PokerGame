using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignIn_or_Register : MonoBehaviour
{
    public Image SelectSignIn;
    public GameObject SignIn;
    public Image SelectRegister;
    public GameObject Register;

    public Text SignInStatus;
    public Text RegisterStatus;

    public Sprite Eye_1;
    public Sprite Eye_2;

    public InputField SignInPasswordField;
    public Image SignInPasswordEye;

    public InputField RegisterPasswordField;
    public Image RegisterPasswordEye;

    public InputField RegisterConfirmPasswordField;
    public Image RegisterConfirmPasswordEye;

    public Button RegisterButton;

    public void OnClick_SelectSignIn()
    {
        SignIn.SetActive(true);
        SignInStatus.text = "";
        Register.SetActive(false);
        SelectSignIn.color = new Color(1, 1, 1, 50/255f);
        SelectRegister.color = new Color(1, 1, 1, 30/255f);
    }

    public void OnClick_SelectRegister()
    {
        Register.SetActive(true);
        RegisterStatus.text = "";
        SignIn.SetActive(false);
        SelectRegister.color = new Color(1, 1, 1, 50/255f);
        SelectSignIn.color = new Color(1, 1, 1, 30/255f);

        Check_ConfirmPassword();
    }

    public void Check_ConfirmPassword()
    {
        RegisterButton.interactable = RegisterPasswordField.text.Equals(RegisterConfirmPasswordField.text) && RegisterPasswordField.text != "";
    }
    
    public void Toggle_SingInPasswordPlainText()
    {
        if(SignInPasswordField.contentType == InputField.ContentType.Password)
        {
            SignInPasswordField.contentType = InputField.ContentType.Standard;
            SignInPasswordEye.sprite = Eye_2;
        }
        else
        {
            SignInPasswordField.contentType = InputField.ContentType.Password;
            SignInPasswordEye.sprite = Eye_1;
        }
        SignInPasswordField.ForceLabelUpdate();
    }

    public void Toggle_RegisterPasswordPlainText()
    {
        if(RegisterPasswordField.contentType == InputField.ContentType.Password)
        {
            RegisterPasswordField.contentType = InputField.ContentType.Standard;
            RegisterPasswordEye.sprite = Eye_2;
        }
        else
        {
            RegisterPasswordField.contentType = InputField.ContentType.Password;
            RegisterPasswordEye.sprite = Eye_1;
        }
        RegisterPasswordField.ForceLabelUpdate();
    }

    public void Toggle_RegisterConfirmPasswordPlainText()
    {
        if(RegisterConfirmPasswordField.contentType == InputField.ContentType.Password)
        {
            RegisterConfirmPasswordField.contentType = InputField.ContentType.Standard;
            RegisterConfirmPasswordEye.sprite = Eye_2;
        }
        else
        {
            RegisterConfirmPasswordField.contentType = InputField.ContentType.Password;
            RegisterConfirmPasswordEye.sprite = Eye_1;
        }
        RegisterConfirmPasswordField.ForceLabelUpdate();
    }
}
