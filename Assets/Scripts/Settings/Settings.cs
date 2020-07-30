using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public GameObject SettingsScreenObj;

    public Slider MasterSlider;
    public Text MasterVolText;
    public Slider BGMSlider;
    public Text BGMVolText;
    public Slider ButtonSlider;
    public Text ButtonVolText;

    private string path;

    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            path = Application.persistentDataPath + "/Settings";
        }
        else 
        {
            path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            path += "/Settings";
        }

        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            path += "/settings.txt";

            OnClick_Default();
        }
        else
        {
            path += "/settings.txt";
        }

        ReadFile();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))        //뒤로가기
        {
            OnClick_Close();
        }
    }

    public void OnChangeMasterVol() { MasterVolText.text = MasterSlider.value.ToString(); }
    public void OnChangeBGMVol() { BGMVolText.text = BGMSlider.value.ToString(); }
    public void OnChangeButtonVol() { ButtonVolText.text = ButtonSlider.value.ToString(); }

    public void OnClick_Default()
    {
        DefaultSettings();
        MasterSlider.value = 50;
        BGMSlider.value = 50;
        ButtonSlider.value = 50;
    }
    private void DefaultSettings()
    {
        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine("0.5");
        sw.WriteLine("0.5");
        sw.WriteLine("0.5");
        sw.Flush();
        sw.Close();
    }

    public void OnClick_Close()
    {
        WriteFile();

        SettingsScreenObj.SetActive(false);
    }

    public void WriteFile()
    {
        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine(MasterVolText.text);
        sw.WriteLine(BGMVolText.text);
        sw.WriteLine(ButtonVolText.text);
        sw.Flush();
        sw.Close();
    }

    private void ReadFile()
    {
        StreamReader sr = new StreamReader(path);

        string input = "";
        int i=0;

        for(i=0;i<3;i++)
        {
            input = sr.ReadLine();
            if(input == null) { break; }

            switch(i)
            {
                case 0:
                    MasterSlider.value = float.Parse(input);
                    break;
                case 1:
                    BGMSlider.value = float.Parse(input);
                    break;
                case 2:
                    ButtonSlider.value = float.Parse(input);
                    break;
                default:
                    break;
            }
            
        }
        sr.Close();
    }
}