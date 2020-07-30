using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreatePlayer : MonoBehaviour
{
    public Text InputPlayerName;

    private void Start()
    {
        //Screen.SetResolution(Screen.width, Screen.width/16*9, false);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
            OnClick_CreatePlayer();
    }

    public void OnClick_CreatePlayer()
    {
        PlayerNetwork.Instance.setPlayerNetwork();

        Debug.Log("Created player: " + InputPlayerName.text);
        SceneManager.LoadScene(1);
    }
}
