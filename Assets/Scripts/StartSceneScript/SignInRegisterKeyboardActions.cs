using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignInRegisterKeyboardActions : MonoBehaviour
{
    public GameObject SObj;
    public InputField SID;
    public InputField SPW;
    public GameObject RObj;

    public GameObject SIRMObj;
    private SignInRegisterManager SIRM;

    public InputField[] RIFArr = new InputField[4];

    // Start is called before the first frame update
    void Start()
    {
        SIRM = SIRMObj.GetComponent<SignInRegisterManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)){
            if(SObj.activeSelf){
                if(SID.isFocused)
                    SPW.Select();
                else
                    SID.Select();
            }
            else {
                bool shift = Input.GetKey(KeyCode.LeftShift)
                            ||Input.GetKey(KeyCode.RightShift);

                int i = Array.FindIndex(RIFArr, l => l.isFocused);
                i = ( i + ((shift)?3:1) ) % 4;
                
                RIFArr[i].Select();
            }
        }

        if(Input.GetKeyDown(KeyCode.Return)){
            if(SObj.activeSelf)
                SIRM.OnClick_SignIn();
            else
                SIRM.OnClick_Register();
        }
            

    } // Update()
}
