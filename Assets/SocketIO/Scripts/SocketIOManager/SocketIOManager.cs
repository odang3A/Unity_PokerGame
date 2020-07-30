using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using SocketIO;

public class SocketIOManager : MonoBehaviour
{
	private string version = "2.0.2";
	private SocketIOComponent socket;

	private SignInRegisterManager SIRM;	//SignInRegisterManager

	private PlayerInfo PI;	//PlayerInfo
	private JSONObject currPlayerInfo = new JSONObject();

	Dictionary<string, string> data = new Dictionary<string, string>();
	JSONObject jdata;

	//시간 관련
	private DateTime startDateTime = default(DateTime); //플레이 시작 시간
	private DateTime endDateTime = default(DateTime); //플레이 종료 시간
	private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
	private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

	//게임, 라운드 로그 관련
	private Dictionary<string, string> currGameLog = new Dictionary<string, string>();
	private Dictionary<string, string> currRoundLog = new Dictionary<string, string>();
	private GameControl GC;	//GameControl

	private void Start() 
	{
		GameObject go = GameObject.Find("SocketIO");
		socket = go.GetComponent<SocketIOComponent>();

		//SignInRegisterManager
		GameObject SIRMObj = GameObject.Find("SignInRegisterManager");
		SIRM = SIRMObj.GetComponent<SignInRegisterManager>();

		socket.On("CheckVersion", (e) => {
			if(e.data["version"].ToString().Trim('"') != version){
				GameObject.Find("NewVersion").transform.Find("NewVersionOnOff").gameObject.SetActive(true);
			}
		});

		socket.On("updateCurrPlayerInfo", (e) => {
			currPlayerInfo = e.data;
			if(SceneManager.GetActiveScene().name == "MainMenuScene"){
				PI.setPlayerStats(currPlayerInfo);
			}
		});

		socket.On("IsPlaying", (e) => {
			if(currPlayerInfo.ToString().Trim('"') != "null") {
				Dictionary<string, string> NickData = new Dictionary<string, string>();
				NickData.Add("nick", currPlayerInfo["nick"].ToString().Trim('"'));
				JSONObject NickJdata = new JSONObject(NickData);

				socket.Emit("IsPlaying", NickJdata);
			}
		});
	}

	// playerAuth
    public void SignIn(string id, string passwd)
    {
        data = new Dictionary<string, string>();
        data.Add("id", id);
        data.Add("passwd", passwd);
        jdata = new JSONObject(data);

        socket.Emit("SignIn", jdata);
		
		socket.On("SignIn", (e) => {
			data = e.data.ToDictionary();
			//Debug.Log(e) ;
			SIRM.SignInSuccess(data);
			socket.Off("SignIn");
		});
    }

	private void SignOut()
	{
		if(currPlayerInfo.ToString().Trim('"') != "null") {
			Dictionary<string, string> SOData = new Dictionary<string, string>();
			SOData.Add("nick", currPlayerInfo["nick"].ToString().Trim('"'));
			JSONObject SOJdata = new JSONObject(SOData);

			socket.Emit("SignOut", SOJdata);
		}
	}

	public void SignOutFromMenu()
	{
		SignOut();
	}

	public void Register(string id, string nick, string passwd)
	{
		data = new Dictionary<string, string>();
		data.Add("id", id);
		data.Add("nick", nick);
		data.Add("passwd", passwd);
		jdata = new JSONObject(data);

		socket.Emit("Register", jdata);
		socket.On("Register", (e) => {
			data = e.data.ToDictionary();
			//Debug.Log("Register Success!");
			
			SIRM.RegisterSuccess(data);
			socket.Off("Register");
		});
	}
	// playerAuth

	// playerStats
	public void playerStats(string nick)	//called from playerInfo!
	{
		data = new Dictionary<string, string>();
		data.Add("nick", nick);
		jdata = new JSONObject(data);

		socket.Emit("GetPlayerStats", jdata);
		socket.On("GetPlayerStats", (e) => {
			GameObject PIObj = GameObject.Find("PlayerInfo");
			PI = PIObj.GetComponent<PlayerInfo>();
			currPlayerInfo = e.data;
			PI.setPlayerStats(currPlayerInfo);
			
			socket.Off("GetPlayerStats");
		});
	}
	// playerStats

	// gamePlay
	public void updatePlayerStats(string update) {
		data = new Dictionary<string, string>();
		Debug.Log(update);
		data.Add("nick", currPlayerInfo["nick"].ToString().Trim('"'));
		data.Add("update", update);
		
		jdata = new JSONObject(data);

		socket.Emit("UpdatePlayerStats", jdata);
	}

	//씬 로드 되면
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(scene.name == "GamePlayScene") {
			GameObject GCObj = GameObject.Find("GameControl");
			GC = GCObj.GetComponent<GameControl>();

			if(!PlayerNetwork.Instance.isGuest) {
				if(startDateTime == default(DateTime)) {
					startDateTime = DateTime.Now;
				}
			}

		} else {
			if(!PlayerNetwork.Instance.isGuest){
				if(startDateTime != default(DateTime)) {
					updatePlayTime();
				}
			}
		}

		if(scene.name == "StartScene"){
			SignOut();
		}
	}
	private void OnApplicationQuit()
	{
		if(!PlayerNetwork.Instance.isGuest){
			if(SceneManager.GetActiveScene().name == "GamePlaySceme"){
				updatePlayTime();
			}
		}

		SignOut();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if(pauseStatus){
			SignOut();
		}
	}
	
	private void updatePlayTime() {
		endDateTime = DateTime.Now;
		TimeSpan playTimeSpan = endDateTime - startDateTime;
		string t = (TimeSpan.Parse(currPlayerInfo["playTime"].ToString().Trim('"'))
					+ playTimeSpan).ToString();

		data = new Dictionary<string, string>();

		data.Add("nick", currPlayerInfo["nick"].ToString().Trim('"'));
		data.Add("playTime", t);
		jdata = new JSONObject(data);

		socket.Emit("UpdatePlayTime", jdata);

		startDateTime = endDateTime = default(DateTime);
	}
	// gamePlay

	// gameLog
	public void gameStartLog(Dictionary<string, string> d) {
		jdata = new JSONObject(d);

		// 게임 id 요청, 플레이어 정보 전달
		socket.Emit("gameStartLog", jdata);

		//게임id 받기
		socket.On("gameStartLog", (e) => {
			socket.Off("gameStartLog");
			
			currGameLog = e.data.ToDictionary();

			GC.spreadGameLog(currGameLog);
		});

	}
	public void setCurrGameLog(Dictionary<string, string> gameLog){
		currGameLog = gameLog;
	} //게임로그 받음

	public void gameEndLog(string gameWinner) {
		data = new Dictionary<string, string>();
		data.Add("gameId", currGameLog["_id"]);
		data.Add("gameWinner", gameWinner);
		jdata = new JSONObject(data);

		socket.Emit("gameEndLog", jdata);
	}
	// gameLog

	// roundLog
	public void roundStartLog(Dictionary<string, string> d) {
		d.Add("gameId", currGameLog["_id"]);
		jdata = new JSONObject(d);
		
		socket.Emit("roundStartLog", jdata);

		socket.On("roundStartLog", (e) => {
			currRoundLog = e.data.ToDictionary();

			GC.spreadRoundLog(currRoundLog);

			socket.Off("roundStartLog");
		});
	}

	public void setCurrRoundLog(Dictionary<string, string> roundLog) {
		currRoundLog = roundLog;
	}

	public void roundEndLog(string roundWinner, int roundPrise) {
		data = new Dictionary<string, string>();
		data.Add("roundId", currRoundLog["_id"]);
		data.Add("roundWinner", roundWinner);
		data.Add("roundPrise", roundPrise.ToString());
		jdata = new JSONObject(data);

		socket.Emit("roundEndLog", jdata);
	}
}