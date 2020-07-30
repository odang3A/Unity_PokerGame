using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource_BGM;

    public AudioSource audioSource_Click;
    public AudioClip audioClip_Click;

    public AudioSource audioSource_FlipPage;
    public AudioClip audioClip_FlipPage;
    
    public Slider MasterSlider;
    public Slider BGMSlider;
    public Slider ButtonSlider;

    private void Start()
    {
        Play_BGM();
    }

    private void Play_BGM()
    {
        audioSource_BGM.Play();
    }

    public void Play_Click_Sound()
    {
        audioSource_Click.PlayOneShot(audioClip_Click);
    }

    public void Play_FlipPage_Sound()
    {
        audioSource_FlipPage.PlayOneShot(audioClip_FlipPage);
    }

    public void OnChange_MasterVol()
    {
        AudioListener.volume = MasterSlider.value / 100;
    }

    public void OnChange_BGMVol()
    {
        audioSource_BGM.volume = BGMSlider.value / 100f;
    }

    public void OnChange_ButtonVol()
    {
        audioSource_Click.volume = ButtonSlider.value / 100f;
        audioSource_FlipPage.volume = ButtonSlider.value / 100f;
    }
}
