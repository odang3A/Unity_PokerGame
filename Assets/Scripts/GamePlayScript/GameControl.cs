using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    private PlayerStat Player;
    public GameObject Players;
    private PlayersActivate playersActivate;
    private List<GameObject> PlayerObjList;
    public GameObject PlayerManagement;
    private PlayerManagement playerManagement;
    private bool[] cards = new bool[52];
    public Sprite[] cardSprite = new Sprite[53];
    public GameObject ChooseCard;
    public Slider ChooseTimer;
    private bool cardSelected;
    private bool betFirst = true;
    public GameObject ChooseBet;
    private bool betDone;
    public GameObject Pot;
    private PotControl potControl;
    public GameObject CallCheck;
    private bool betCheck;
    public GameObject CallValue;
    private Text CallValueText;
    public GameObject RaiseObj;
    public GameObject Raise;
    private Slider RaiseSlider;
    public GameObject RaiseValue;
    private Text RaiseValueText;
    private int Turn;
    public GameObject[] ShareCardObj = new GameObject[4];
    private int[] shareCard = new int[4];
    private bool mineIsBetter = true;
    private int readyForNext;
    public GameObject SkipToNextBtn;
    public GameObject FinalWinner;
    private Text FinalWinnerText;
    public Text JoinRematchText;
    private PhotonView PhotonView;

    private SocketIOManager socketIOManager;
    private int RoundNum = 0;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        playersActivate = Players.GetComponent<PlayersActivate>();
        PlayerObjList = new List<GameObject>(playersActivate.Players);
        playerManagement = PlayerManagement.GetComponent<PlayerManagement>();
        potControl = Pot.GetComponent<PotControl>();
        CallValueText = CallValue.GetComponent<Text>();
        RaiseSlider = Raise.GetComponent<Slider>();
        RaiseValueText = RaiseValue.GetComponent<Text>();
        FinalWinnerText = FinalWinner.transform.Find("Text").GetComponent<Text>();

        //소켓 오브젝트, 스크립트 연결
        GameObject SIOMObj = GameObject.Find("SocketIOManager");
        socketIOManager = SIOMObj.GetComponent<SocketIOManager>();

        Player = new PlayerStat(100);           //플레이어 생성(토큰 부여)
        ChooseCard.SetActive(false);
    }

    public void StartGame() //완전 처음 (게임 시작)
    {
        SetPlayers();

        if(!PlayerNetwork.Instance.isGuest)     //로그인한 플레이어이면
        {   //전적 게임 플레이 횟수 업데이트
            socketIOManager.updatePlayerStats("gamePlayCnt");
        }

        if(PhotonNetwork.isMasterClient)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            int i = 1;
            foreach(PhotonPlayer p in PhotonNetwork.playerList) {
                d.Add("p"+i++, p.NickName);
            }
            d.Add("playerCnt", ""+PhotonNetwork.room.playerCount);
            //게임 로그 기록
            socketIOManager.gameStartLog(d);
        }

        // SpredCard()
    }
    // 게임 로그 뿌리기, 게임 시작
    public void spreadGameLog(Dictionary<string, string> gameLog){
        PhotonView.RPC("RPC_SpreadGameLog", PhotonTargets.Others, gameLog);

        SpredCard();
    }
    [PunRPC]
    private void RPC_SpreadGameLog(Dictionary<string, string> gameLog){
        socketIOManager.setCurrGameLog(gameLog);
    }
    // 게임 로그 뿌리기

    private void SetPlayers()
    {
        playersActivate.Activate();
    }

    private void SpredCard() // (라운드 시작)
    {
        PlayerControl playerControl;
        PhotonPlayer player;
        int card = 0;
        int i=0, j=0;

        Dictionary<string, string> d = new Dictionary<string, string>();
        int k=1;

        for(i=0;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                player = playerControl.PhotonPlayer;
                
                if(!playerControl.isDisable())      //파산이 아니면
                {
                    d.Add("p"+k++, player.NickName);//라운드 플레이어 목록
                    for(j=0;j<3;j++)
                    {
                        do{
                            card = UnityEngine.Random.Range(0, 52);
                        } while(cards[card]);
                        PhotonView.RPC("RPC_UpdateCards", PhotonTargets.All, card);
                        PhotonView.RPC("RPC_ReceiveCard", player, card, j);
                    }
                }
            }
        }
        d.Add("playerCnt", (k-1).ToString());
        d.Add("roundNum", ""+(++RoundNum));
        //라운드 로그 기록
        socketIOManager.roundStartLog(d);

        PhotonView.RPC("RPC_selectCard", PhotonTargets.All);
    }
    // 라운드 로그 뿌리기
    public void spreadRoundLog(Dictionary<string, string> roundLog) {
        PhotonView.RPC("RPC_SpreadRoundLog", PhotonTargets.Others, roundLog);
    }
    [PunRPC]
    private void RPC_SpreadRoundLog(Dictionary<string, string> roundLog) {
        socketIOManager.setCurrRoundLog(roundLog);
        ++RoundNum;
    }
    // 라운드 로그 뿌리기

    private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
    {
        PlayerLeftGame(photonPlayer);
    }

    public void OnClick_QuitToMain()     //게임 탈주
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(1);
    }

    private void OnDisconnectedFromPhoton()
    {
        SceneManager.LoadScene(1);
    }

    private void PlayerLeftGame(PhotonPlayer player)
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.setPlayerAsDisconnected();
        playerControl.clearCard();

        if(playerControl.IsBetting.activeSelf)      //베팅 도중에 베팅하던 놈이 나가면
        {
            playerControl.activeIsBetting(false);
            
            if(PhotonNetwork.isMasterClient)
            {
                playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
                int i=0;
                bool found = false;
                for(i=0;i<8;i=(i+1)%8)
                {
                    if(PlayerObjList[i].activeSelf)
                    {
                        playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                        if(found && !playerControl.isDisable())
                        {
                            PhotonView.RPC("RPC_callNextBet", playerControl.PhotonPlayer);
                            break;
                        }
                        if(playerControl.PhotonPlayer == player)
                        {
                            found = true;
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    private void RPC_selectCard()
    {
        if(!Player.disable)     //파산이 아니면
        {
            if(!checkAllDie())      //만약 전 판에서 모두 죽지 않았으면(파산하지 않았으면)
            {
                ChooseCard.SetActive(true);
                if(!PlayerNetwork.Instance.isGuest){
                    //전적 라운드 플레이 횟수 업데이트
                    socketIOManager.updatePlayerStats("roundPlayCnt");
                }
            }
            else
            {
                return;
            }
        }

        StartCoroutine(WaitForSelect());
    }

    [PunRPC]
    private void RPC_UpdateCards(int card)
    {
        cards[card] = true;
    }

    [PunRPC]
    private void RPC_ReceiveCard(int card, int i)
    {
        ChooseCard chooseCard = ChooseCard.GetComponent<ChooseCard>();
        chooseCard.setCardSprite(i, cardSprite[card]);
        Player.card[i] = card;
        Player.deck[i] = card;
    }

    public void OnClick_Card1()
    {
        int tmp = Player.card[0];
        Player.card[0] = Player.card[2];
        Player.card[2] = tmp;
        cardSelected = true;
    }
    public void OnClick_Card2()
    {
        int tmp = Player.card[1];
        Player.card[1] = Player.card[2];
        Player.card[2] = tmp;
        cardSelected = true;
    }
    public void OnClick_Card3()
    {
        cardSelected = true;
    }

    private PlayerControl searchPlayer(PhotonPlayer player)
    {
        PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();;
        int i=0;
        for(i=0;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                if(playerControl.PhotonPlayer == player)
                    break;
            }
        }
        return playerControl;
    }

    private IEnumerator WaitForSelect()
    {
        if(!Player.disable)             //파산이 아니면
        {
            int i=0;

            ChooseTimer.value = ChooseTimer.maxValue;
            print("coroutine start");
            print(cardSelected);
            while(true)     //카드 고를 때 까지 기다리고
            {
                yield return new WaitForSeconds(0.03f);

                if(ChooseTimer.value <= 0)
                {
                    OnClick_Card3();
                }
                if(cardSelected)
                {
                    break;
                }
                ChooseTimer.value -= 0.002f;
            }
            ChooseCard.SetActive(false);

            PlayerControl playerControl;
            playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
            for(i=0;i<3;i++)
                playerControl.SetCard(i, cardSprite[Player.card[i]]);   //고르면 플레이어 앞에 세팅
        }

        PhotonView.RPC("RPC_WhenSelect", PhotonTargets.All, PhotonNetwork.player);  //골랐다고 알려줌
        
        StartCoroutine(waitForEveryoneSelectCard());        //모두 깔 카드가 준비될 때 까지 기다림
    }

    [PunRPC]
    private void RPC_WhenSelect(PhotonPlayer player)
    {
        int i=0;
        PlayerControl playerControl = searchPlayer(player);
        if(!playerControl.isDisable())      //파산이 아니면
        {
            playerControl.moveCard();       //카드 골랐다고 표시
        }

        playerControl.setReadyCard(true);       //카드 준비됨
    }

    private IEnumerator waitForEveryoneSelectCard()     //모두 깔 카드가 준비될 때 까지 기다림
    {
        int i=0;
        while(i!=8)
        {
            yield return null;

            PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
            for(i=0;i<8;i++)
            {
                if(PlayerObjList[i].activeSelf)
                {
                    playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                    if(!playerControl.isDisable())          //파산이 아니면
                    {
                        if(!playerControl.isReadyCard())        //카드 아직 안고름?
                        {
                            break;
                        }
                    }
                }
            }
        }

        if(!Player.disable)
        {
            initBet(getInitBet());      //기본배팅 하기
        }
        readyToShow();      //카드 보여줄 준비 됨
    }

    
    private int getInitBet()            //가장 적은 토큰 반환
    {
        PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
        int i=0;
        int minInit=999999;
        for(i=0;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                if(!playerControl.isDisable())          //파산이 아니면
                {
                    if(playerControl.GetTokenInt() < minInit)
                    {
                        minInit = playerControl.GetTokenInt();
                    }
                }
            }
        }
        return minInit;
    }

    private void initBet(int minInit)           //기본베팅
    {
        if(!Player.disable)         //파산이 아니면
        {
            int init = 5;           //최대 기본배팅
            if(init <= minInit)
            {
                Player.Token -= init;
                PhotonView.RPC("RPC_addToPot", PhotonTargets.All, PhotonNetwork.player, init);
            }
            else
            {
                Player.Token -= minInit;
                PhotonView.RPC("RPC_addToPot", PhotonTargets.All, PhotonNetwork.player, minInit);
            }
        }
    }


    private void readyToShow()
    {
        PhotonView.RPC("RPC_showCard", PhotonTargets.All, PhotonNetwork.player, Player.card[2]);//남들에게 카드 보여줌
        
        StartCoroutine(isBetReady());   //베팅 준비됨?(누가 먼저 베팅할지 카드 비교)
    }

    [PunRPC]
    private void RPC_showCard(PhotonPlayer player, int card)
    {
        PlayerControl playerControl = searchPlayer(player);

        if(PhotonNetwork.player != player)  //자신이랑 비교하는게 아닌지 확인
        {
            if(!playerControl.isDisable())      //파산이 아니면
            {
                if(card%13 == Player.card[2]%13)    //숫자       //첫 베팅 판별
                {
                    if(card/13 < Player.card[2]/13) //모양
                        betFirst = false;
                }
                else if(card%13 > Player.card[2]%13)    //숫자
                    betFirst = false;

                playerControl.showCard(cardSprite[card]);       //카드 보여줌
            }
        }

        playerControl.setReadyToBet(true);      //베팅할 준비가 되었는지 확인
        
    }

    private IEnumerator isBetReady()                        //첫 베팅 준비 완료
    {
        int i=0;
        while(i!=8)
        {
            yield return null;

            PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
            for(i=0;i<8;i++)
            {
                if(PlayerObjList[i].activeSelf)
                {
                    playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                    if(!playerControl.isDisable())          //파산이 아니면
                    {
                        if(!playerControl.isReadyToBet())        //베팅 준비 안됨?
                        {
                            break;
                        }
                    }
                }
            }
        }

        if(!Player.disable)     //파산이 아니면
        {
            if(betFirst)        //가장 먼저 베팅이면
            {
                StartCoroutine(Betting());  //첫 베팅 시작
            }
        }
    }

    private IEnumerator Betting()
    {
        bool allDie = false;
        PlayerControl playerControl = searchPlayer(PhotonNetwork.player);

        Text CheckText = CallCheck.GetComponent<Text>();
        if(potControl.GetCurrBet() == 0)    //체크인지 확인
        {
            CheckText.text = "Check";
            CallValue.SetActive(false);
        }
        else        //아니면 콜 띄워줌
        {
            int callInt = ((potControl.GetCurrBet() - Player.MyBet) < Player.Token) 
                           ? (potControl.GetCurrBet() - Player.MyBet) : Player.Token;
            CheckText.text = "Call";
            betCheck = false;
            CallValue.SetActive(true);
            CallValueText.text = "" + callInt;
        }
        if(checkAllDie())       //모두 다이쳐서 나만 남았는지 확인
        {
            onAllDie();                     //나 빼고 모두 뒤졌으면 호출 
        }
        else if((Player.MyBet == potControl.GetCurrBet() 
        && (potControl.GetCurrBet() != 0 || checkEveryoneAllIn()))
        || betCheck)     //모두 콜해서 베팅이 끝남
        {
            TurnEnd();                    //그러면 다음 베팅 준비 해야지
        }
        else                                                   //아니면 이어서 베팅
        {
            int c = potControl.GetCurrBet() - Player.MyBet; //콜 할 금액

            if(c >= Player.Token || getTopToken() == 0)   //콜이 내 토큰보다 많거나
                RaiseObj.SetActive(false);                  //나머지 모두 올인이거나
            else                                                            //아니면
                RaiseObj.SetActive(true);      //가장 많이 있는 토큰 또는 내 토큰으로 레이즈 범위지정
            RaiseSlider.minValue = 1;      //콜 + 1
            RaiseSlider.maxValue = ((Player.Token - c < getTopToken())
                                    ? Player.Token - c : getTopToken());
            RaiseSlider.value = RaiseSlider.minValue;

            ChooseBet.SetActive(true);      //나 배팅하고 있다고 보여줌(알려줌)
            PhotonView.RPC("RPC_showBetting", PhotonTargets.All, PhotonNetwork.player, true);
            playerControl.BetTimer.value = playerControl.BetTimer.maxValue; //타이머 초기화
            while(!betDone)     //베팅 햘 때까지 기다림
            {
                if((allDie = checkAllDie()) || (Player.Token == 0)) //그 사이에 누가 나가서(뒤져서) 나만 남으면
                    break;                     //아니면 내 차례인데 내가 올인이면
                RaiseValueText.text = "call(" + c + ")+" + (RaiseSlider.value);

                yield return null;
                
                if(playerControl.BetTimer.value == 0)
                {
                    OnClick_Die();
                }
                
            }
            betDone = false;
            if(allDie)      //나만 남음
            {
                onAllDie();                     //나 빼고 모두 뒤졌으면 호출 (이김)
            }
            else
            {
                checkAllIn();                 //내가 베팅하고 이게 올인인지 확인
                PhotonView.RPC("RPC_callNextBet", findNextBet());      //다음 베팅 사람 찾아서 호출 
                ChooseBet.SetActive(false);
                PhotonView.RPC("RPC_showBetting", PhotonTargets.All, PhotonNetwork.player, false);  //나 베팅 끝났다고 알려줌(보여줌)
                if(potControl.GetCurrBet() == 0)    //내가 체크했으면(다음에 돌아왔을 때 체크이면 다음 턴)
                    betCheck = true;
            }
        }
    }

    private int getTopToken()
    {
        PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
        int i=0;
        int maxInt=0;
        for(i=1;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                if(!playerControl.isDisable() && !playerControl.isDie())          //파산이 아니면
                {
                    if(playerControl.GetTokenInt() > maxInt)
                    {
                        maxInt = playerControl.GetTokenInt();
                    }
                }
            }
        }
        return maxInt;
    }

    [PunRPC]
    private void RPC_callNextBet()      //다음 사람 베팅
    {
        StartCoroutine(Betting());
    }

    [PunRPC]
    private void RPC_showBetting(PhotonPlayer player, bool b)           //누가 베팅하고 있는지 보여줌
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.activeIsBetting(b);
    }

    private PhotonPlayer findNextBet()              //다음 베팅할 사람 찾기
    {
        PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
        int i=0;
        for(i=1;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                if(!playerControl.isDie() && !playerControl.isDisable())
                    break;
            }
        }
        if(i==8)
            return PhotonNetwork.player;            //없으면 모두 다이로 봄
        return playerControl.PhotonPlayer;
    }

    public void OnClick_Die()                       //내가 다이 치면
    {
        betDone = true;
        PhotonView.RPC("RPC_PlayerDied", PhotonTargets.All, PhotonNetwork.player, true);
        Player.die = true;
    }

    [PunRPC]
    private void RPC_PlayerDied(PhotonPlayer player, bool b)                //다이 쳤다고 표시
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.setPlayerAsDie(b);
    }

    private bool checkAllDie()                      //나 빼고 다 뒤졌는지 확인
    {
        if(PhotonNetwork.player == findNextBet())
            return true;
        else
            return false;
    }

    private void onAllDie()                         //나 빼고 다 뒤졌을 때
    {
        ChooseBet.SetActive(false);
        PhotonView.RPC("RPC_showBetting", PhotonTargets.All, PhotonNetwork.player, false);  //나 베팅 끝났다고 알려줌(보여줌)
        Player.myBestDeck[5] = 0;
        PhotonView.RPC("RPC_showWinner", PhotonTargets.All, PhotonNetwork.player, Player.myBestDeck);      //내가 이김
    }

    private void checkAllIn()                       //내가 올인 했는지 확인
    {
        if(Player.Token == 0)
            PhotonView.RPC("RPC_playerAllIn", PhotonTargets.All, PhotonNetwork.player);
    }
    [PunRPC]
    private void RPC_playerAllIn(PhotonPlayer player)       //누가 올인 했다고 표시
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.setPlayerAllIn();
    }

    private bool checkEveryoneAllIn()                       //모두 올인인지 확인
    {
        PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
        int i=0;
        for(i=1;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                if(!playerControl.isDie() && !playerControl.isDisable())
                    if(!playerControl.checkPlayerAllIn())
                        break;
            }
        }
        return i==8;
    }

    public void OnClick_Call()                  //까짓거 콜 해줄게
    {
        betDone = true;
        int toBet;
        toBet = potControl.GetCurrBet() - Player.MyBet;
        if(toBet <= Player.Token)                           //토큰이 충분할 때
        {
            PhotonView.RPC("RPC_addToPot", PhotonTargets.All, PhotonNetwork.player, toBet);
            Player.Token -= toBet;
            Player.MyBet += toBet;
        }
        else                                                //그지새끼가 콜해서 올인
        {
            PhotonView.RPC("RPC_addToPot", PhotonTargets.All, PhotonNetwork.player, Player.Token);
            Player.MyBet += Player.Token;
            Player.Token = 0;
        }
    }

    public void OnClick_Raise()                 //좀더 올린다?
    {
        betDone = true;
        int c = potControl.GetCurrBet() - Player.MyBet;
        int raisedBet;
        raisedBet = (int)RaiseSlider.value;
        PhotonView.RPC("RPC_addToPot", PhotonTargets.All, PhotonNetwork.player, c + raisedBet);
        PhotonView.RPC("RPC_raiseCurrBet", PhotonTargets.All, potControl.GetCurrBet() + raisedBet);
        Player.Token -= (c + raisedBet);
        Player.MyBet = potControl.GetCurrBet();
    }

    public void OnClick_Add()
    {
        if(RaiseSlider.maxValue != RaiseSlider.value)
            RaiseSlider.value++;
    }

    public void OnClick_Dec()
    {
        if(RaiseSlider.minValue != RaiseSlider.value)
            RaiseSlider.value--;
    }

    [PunRPC]
    private void RPC_addToPot(PhotonPlayer player, int b)           //판돈 올라가는거 보여줌
    {
        potControl.AddToPot(b);
        PlayerControl playerControl = searchPlayer(player);
        playerControl.playerHasBet(b);

        if(PhotonNetwork.player == player 
        && !PlayerNetwork.Instance.isGuest)
        { // 전적 토큰 업데이트 (-)
            socketIOManager.updatePlayerStats((-1*b).ToString());
        }
    }

    [PunRPC]
    private void RPC_raiseCurrBet(int r)                            //모두에게 레이즈 된 값 알려줌
    {
        potControl.RaiseBetTo(r);
    }

    private void TurnEnd()                            //한 턴 끝나면
    {
        PhotonView.RPC("RPC_GetReadyForNextBet", PhotonTargets.All, PhotonNetwork.player);        //다음 베팅을 위해 베팅 값 설정
    }

    [PunRPC]
    private void RPC_GetReadyForNextBet(PhotonPlayer player)              //다음 베팅 준비
    {
        Player.MyTotalBet += Player.MyBet;      //내 총 베팅 업데이트
        Player.MyBet = 0;
        potControl.SetCurrBetToZero();      //pot 초기화
        betCheck = false;

        Turn++;                                //카드 깐 횟수 증가
        if(Turn>3)                             //카드 모두 깜 (모든 베팅 끝남)
        {
            if(!Player.die && !Player.disable)                 //다이 안한 상태이면
            {
                PhotonView.RPC("RPC_showEveryCard", PhotonTargets.Others, PhotonNetwork.player, Player.card); //카드 모두 보여주기
            }
            else                            //다이한 상태이면
            {
                PhotonView.RPC("RPC_setPlayerIsReadyToCompare", PhotonTargets.All, PhotonNetwork.player);//덱 비교 준비 됬다고 함
            }
            
            ChooseBet.SetActive(false);     //화면 깔끔하게
            StartCoroutine(waitForReadyToCompare());    //승자 찾기(가장 높은 덱 찾게 하고 비교)
        }
        else
        {
            if(PhotonNetwork.player == player)
                NextShareCard();
        }
    }

    private void NextShareCard()            //다음 공유카드 뽑음
    {
        int s_card = 0;
        if(Turn == 1)                      //처음으로 까는 경우 (2개 깜)
        {
            do{
                s_card = UnityEngine.Random.Range(0, 52);
            } while(cards[s_card]);
            PhotonView.RPC("RPC_UpdateCards", PhotonTargets.All, s_card);
            PhotonView.RPC("RPC_showNextCard", PhotonTargets.All, s_card, 0);
        }

        do{
            s_card = UnityEngine.Random.Range(0, 52);
        } while(cards[s_card]);
        PhotonView.RPC("RPC_UpdateCards", PhotonTargets.All, s_card);
        PhotonView.RPC("RPC_showNextCard", PhotonTargets.All, s_card, Turn);
        
        StartCoroutine(Betting());       //카드 뽑은 뒤 다시 베팅 시작
    }

    [PunRPC]
    private void RPC_showNextCard(int s_card, int index)            //뽑은 카드 보여줌
    {
        SpriteRenderer CardSprite = ShareCardObj[index].GetComponent<SpriteRenderer>();
        CardSprite.sprite = cardSprite[s_card];
        Player.deck[index+3] = s_card;                          //공유 카드에 저장
    }

    

    [PunRPC]
    private void RPC_showEveryCard(PhotonPlayer player, int[] playerCard)   //남은 카드 보여주기
    {
        int i=0;
        PlayerControl playerControl = searchPlayer(player);
        for(i=0;i<2;i++)
            playerControl.showRest(i, cardSprite[playerCard[i]]);
    }

    [PunRPC]
    private void RPC_setPlayerIsReadyToCompare(PhotonPlayer player)          //다이한 상태이면 바로 올려줌
    {                                               //아니면 가장 높은 덱을 만들고 올려줌
        PlayerControl playerControl = searchPlayer(player);
        playerControl.setReadyToCompare(true);
    }

    private IEnumerator waitForReadyToCompare()
    {
        if(!Player.die && !Player.disable)         //죽지 않았으면
        {
            Player.myBestDeck = findMyBestDeck();      //가장 높은 덱 만들기
            PhotonView.RPC("RPC_setPlayerIsReadyToCompare", PhotonTargets.All, PhotonNetwork.player);//만들었다고 알림
        }
        else
        {                       //죽으면
            Player.myBestDeck[5] = 0;                   //덱 순위 0으로 만듬
        }

        int i=0;
        while(i!=8)
        {
            yield return null;

            PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
            for(i=0;i<8;i++)
            {
                if(PlayerObjList[i].activeSelf)
                {
                    playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                    if(!playerControl.isDisable())          //파산이 아니면
                    {
                        if(!playerControl.isReadyToCompare())        //덱 아직 못고름?
                        {
                            break;
                        }
                    }
                }
            }
        }

        PhotonView.RPC("RPC_Compare", PhotonTargets.All, PhotonNetwork.player, Player.myBestDeck);    //덱 전달

        StartCoroutine(waitForEveryoneToCompare());     //승자 고를 준비    
    }

    
    [PunRPC]
    private void RPC_Compare(PhotonPlayer player, int[] playerDeck)  //내 덱과 비교
    {
        if(PhotonNetwork.player != player)      //자신과 비교하는게 아니면
        {
            if(!Player.die && !Player.disable)         //덱을 받았는데 내가 다이한 상태가 아니면
            {
                if(playerDeck[5] != 0)      //받은 덱이 다이한 상태가 아니면 
                {
                    print(Player.myBestDeck[5]);
                    print(playerDeck[5]);
                    if(mineIsBetter)
                    {
                        if(Player.myBestDeck[5] < playerDeck[5])        //만약 상대 덱이 더 좋으면
                        {
                            mineIsBetter = false;
                        }
                        else if(Player.myBestDeck[5] == playerDeck[5])      //족보가 같을 때
                        {
                            mineIsBetter = ifSameLevel(Player.myBestDeck, playerDeck);
                        }
                    }
                }
            }
            else
            {
                mineIsBetter = false;
            }
        }

        PlayerControl playerControl = searchPlayer(player); //얘랑 비교함
        playerControl.setReadyToChoose(true);

    }

    private IEnumerator waitForEveryoneToCompare()
    {
        int i=0;
        while(i!=8)     //모두 비교 했을 때
        {
            yield return null;

            PlayerControl playerControl = PlayerObjList[0].GetComponent<PlayerControl>();
            for(i=0;i<8;i++)
            {
                if(PlayerObjList[i].activeSelf)
                {
                    playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                    if(!playerControl.isDisable())          //파산이 아니면
                    {
                        if(!playerControl.isReadyToChoose())        //승자 고를 준비 됨?
                        {
                            break;
                        }
                    }
                }
            }
        }
        if(mineIsBetter)            //내것이 가장 좋다면
        {
            PhotonView.RPC("RPC_showWinner", PhotonTargets.All, PhotonNetwork.player, Player.myBestDeck);
        }
}
    
    [PunRPC]
    private void RPC_showWinner(PhotonPlayer player, int[] deck)            //승자 보여줌
    {
        if(PhotonNetwork.player == player)
        { // 라운드 승자 로그 저장
            socketIOManager.roundEndLog(PhotonNetwork.playerName, potControl.getPot());
            if(!PlayerNetwork.Instance.isGuest)
            { //전적 라운드 승리 횟수 업데이트
                // 전적 토큰 업데이트 (+)
                string p = potControl.getPot().ToString();
                socketIOManager.updatePlayerStats((p=="0"?"roundWinCnt":p));
            }
        }

        readyForNext = 0;
        StartCoroutine(ShowWinnerCoru(player, deck));
    }

    private IEnumerator ShowWinnerCoru(PhotonPlayer player, int[] deck)         //5초동안 승자 보여줌
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.WinnerActive(true);                           //승자 위치에 표시
        if(deck[5]!=0)  //모두 다이쳐서 이긴게 아니면
        {
            showDeck(playerControl, deck);                     //승자 덱 보여줌
        }

        if(Player.disable)
        {
            PhotonView.RPC("RPC_setPlayerSkip", PhotonTargets.All, PhotonNetwork.player);
        }
        else
        {
            SkipToNextBtn.SetActive(true);
        }
        //라운드 끝
        int i=0, s=0;
        while(i!=8 && s < 1000)      //모두 스킵을 누르지 않으면
        {
            s++;
            yield return new WaitForSeconds(0.01f);      //10초 기다림

            PlayerControl player_control = PlayerObjList[0].GetComponent<PlayerControl>();
            for(i=0;i<8;i++)
            {
                if(PlayerObjList[i].activeSelf)
                {
                    player_control = PlayerObjList[i].GetComponent<PlayerControl>();
                    if(!player_control.isDisable())          //파산이 아니면
                    {
                        if(!player_control.isSkip())        //스킵 아직 안누름?
                        {
                            break;
                        }
                    }
                }
            }
        }

        SkipToNextBtn.SetActive(false);

        emptyPot(player);                                   //베팅 초기화
        playerControl.HideDeck();               //승자 덱 보여줌 취소
        playerControl.WinnerActive(false);
        initData();                         //데이터 초기화(다음판 준비)
        
        i=0;
        while(i!=8)
        {
            yield return null;

            PlayerControl player_control = PlayerObjList[0].GetComponent<PlayerControl>();
            for(i=0;i<8;i++)
            {
                if(PlayerObjList[i].activeSelf)
                {
                    player_control = PlayerObjList[i].GetComponent<PlayerControl>();
                    if(!player_control.isDisable())          //파산이 아니면
                    {
                        if(!player_control.isReadyForNext())    //다음판 준비 안됨?(데이터 초기화 됨?)
                        {
                            break;
                        }
                    }
                }
            }
        }

        for(i=0;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                PlayerControl player_control = PlayerObjList[i].GetComponent<PlayerControl>();
                player_control.setSkip(false);
                player_control.setReadyForNext(false);  //남은 놈 초기화
            }
        }

        if(checkAllDie())       //나 빼고 모두 완전히 죽었으면
        {
            if(!PlayerNetwork.Instance.isGuest)     //로그인한 플레이어이면
            {   //전적 게임 승리 횟수 업데이트
                socketIOManager.updatePlayerStats("gameWinCnt");
            }
            //승자 게임 로그 저장
            socketIOManager.gameEndLog(PhotonNetwork.playerName);
            //최종 승자
            PhotonView.RPC("RPC_showFinalWinner", PhotonTargets.All, PhotonNetwork.player);
        }
        else if(PhotonNetwork.player == player)     //이긴 놈이 다음 카드 뿌림
        {
            SpredCard();              //다음 시작
        }
    }

    [PunRPC]
    private void RPC_showFinalWinner(PhotonPlayer player)     //최종 승자
    {
        FinalWinnerText.text = "WINNER is " + "《" + player.NickName + "》";
        FinalWinner.SetActive(true);
        StartCoroutine(WaitForRematch());
    }

    private IEnumerator WaitForRematch()        //끝나고 다시
    {
        int i=0, s=10;
        while(true)
        {  
            if(PhotonNetwork.room.playerCount < 2)
            {
                JoinRematchText.text = "Lack of Players";
                yield return new WaitForSeconds(2.0f);
                OnClick_QuitToMain();
            }
                
            else if(s >= 0)
            {
                JoinRematchText.text = "Rematch in " + s;

                yield return new WaitForSeconds(1.0f);
                s--;
            }
            else
            {
                FinalWinner.SetActive(false);
                
                if(PhotonNetwork.isMasterClient)
                {
                    yield return new WaitForSeconds(1.0f);

                    PhotonNetwork.LoadLevel(2);
                }
                else
                {
                    yield return new WaitForSeconds(2.0f);
                }
            }
        }
    }

    public void OnClick_Skip()      //스킵 버튼을 누르면 
    {
        PhotonView.RPC("RPC_setPlayerSkip", PhotonTargets.All, PhotonNetwork.player);
        SkipToNextBtn.SetActive(false);
    }

    [PunRPC]
    private void RPC_setPlayerSkip(PhotonPlayer player)        //스킵 준비된 사람 올림
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.setSkip(true);
    }

    private void showDeck(PlayerControl playerControl, int[] deck)  //덱 카드 아닌 것들 어둡게
    {
        int i=0, j=0;
        for(i=0;i<4;i++)            //공유 카드에서 어둡게
        {
            for(j=0;j<5;j++)
            {
                SpriteRenderer CardSprite = ShareCardObj[i].GetComponent<SpriteRenderer>();
                if(CardSprite.sprite == cardSprite[deck[j]])
                {
                    CardSprite.color = new Color(1, 1, 1, 1);
                    break;
                }
                CardSprite.color = new Color(100/255f, 100/255f, 100/255f, 1);
            }
        }
        playerControl.ShowDeck(deck, cardSprite);       //승자의 카드에서 어둡게
    }

    private void emptyPot(PhotonPlayer player)              //딴 돈 가져가고 pot 초기화
    {
        int potInt = potControl.getPot();
        PlayerControl playerControl = searchPlayer(player);
        playerControl.playerHasBet(potInt * -1);
        if(PhotonNetwork.player == player)
        {
            Player.Token += potInt;
        }
        potControl.initPot();
    }

    private void initData()                     //나머지 데이터 초기화
    {
        int i=0;
        cards = new bool[52];
        cardSelected = false;
        betFirst = true;
        betDone = false;
        betCheck = false;
        Turn = 0;
        shareCard = new int[4];
        mineIsBetter = true;

        ChooseCard.SetActive(false);

        Player.card = new int[3];
        Player.deck = new int[7];
        Player.myBestDeck = new int[6];
        Player.MyBet = 0;
        Player.MyTotalBet = 0;
        Player.die = false;

        for(i=0;i<4;i++)                //승자 덱 표시 취소
        {
            SpriteRenderer CardSprite = ShareCardObj[i].GetComponent<SpriteRenderer>();
            CardSprite.color = new Color(1, 1, 1, 1);
        }

        if(Player.Token == 0)               //파산
        {
            betDone = true;
            PhotonView.RPC("RPC_PlayerBroke", PhotonTargets.All, PhotonNetwork.player);
            Player.disable = true;
        }
        
        for(i=0;i<4;i++)
        {
            SpriteRenderer CardSprite = ShareCardObj[i].GetComponent<SpriteRenderer>();
            CardSprite.sprite = null;
        }

        for(i=0;i<8;i++)
        {
            if(PlayerObjList[i].activeSelf)
            {
                PlayerControl playerControl = PlayerObjList[i].GetComponent<PlayerControl>();
                playerControl.tokenTextWhite();

                playerControl.clearCard();      //RPC_ShowBack(PhotonPlayer player), 카드 이동
                
                playerControl.setReadyCard(false);
                playerControl.setReadyToBet(false);
                playerControl.setReadyToCompare(false);
                playerControl.setReadyToChoose(false);

                playerControl.activeIsBetting(false);

                if(!playerControl.isDisable())
                {
                    playerControl.setPlayerAsDie(false);    //죽은놈 살리기
                }
            }
        }
        PhotonView.RPC("RPC_setPlayerIsReadyForNext", PhotonTargets.All, PhotonNetwork.player);
    }

    [PunRPC]
    private void RPC_PlayerBroke(PhotonPlayer player)       //파산 알려줌
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.setPlayerAsBroke();
    }

    [PunRPC]
    private void RPC_setPlayerIsReadyForNext(PhotonPlayer player)
    {
        PlayerControl playerControl = searchPlayer(player);
        playerControl.setReadyForNext(true);
    }

    private int[] findMyBestDeck()      //가장 높은 덱을 찾아서 반환(마지막 족보 레벨 포함)
    {
        int level=0;
        int bestLevel = 0;
        /*
            13: 로티플
            12: 백티플
            11: 스트레이트 플러시
            10: 포카드
            9: 풀하우스
            8: 플러시
            7: 마운틴(로열 스트레이트)
            6: 백스트레이트
            5: 스트레이트
            4: 트리플
            3: 투페어
            2: 원페어
            1: 탑
        */
        int i=0, j=0, k=0;
        
        int[] tempDeck = new int[5];
        int[] bestDeck = new int[6];    //반환할 덱

        for(i=0;i<6;i++)
        {
            for(j=i+1;j<7;j++)
            {
                int l = 0;
                int Fi=0, Ro=1, MRo=0, Ch=0, St=1;
                /*
                    Fi: 첫 카드
                    Ro: 같은 숫자 연속
                    MRo: 같은 숫자 최고 연속
                    Ch: 숫자 바뀐 횟수
                    St: 스트레이트 연속 횟수
                */
                int[] number = new int[5];
                int[] shape = new int[5];
                bool[] combi = new bool[8];
                /*
                    0: 포카드
                    1: 트리플
                    2: 원페어
                    3: 플러시
                    4: 스트레이트
                    5: 투페어
                    6: 풀하우스
                    7: 백스트레이트
                */

                for(k=0;k<7;k++)            //돌아가며 덱 만들기
                {
                    if(k==i || k==j)
                        continue;
                    number[l] = Player.deck[k] % 13;    //숫자만 가져옴
                    shape[l] = Player.deck[k] / 13;     //문양만 가져옴
                    tempDeck[l] = Player.deck[k];       //임시 덱
                    l++;
                }
                Array.Sort(number);
                
                for(k=0;k<5;k++)        //문양 체크(플러시)
                {
                    if(k==0)
                    {
                        Fi = shape[k];
                        continue;
                    }
                    if(Fi!=shape[k])
                        break;
                }
                combi[3] = k==5;        //문양 체크(플러시)

                for(k=0;k<5;k++)        //숫자 체크
                {
                    if(k==0)        //첫 카드
                    {
                        Fi = number[k];
                        continue;
                    }
                    
                    if(number[k]!=number[k-1])      //같은 숫자체크
                    {
                        if(Ro>MRo)
                            MRo = Ro;
                        Ro=1;
                        Ch++;
                    }
                    else
                    {
                        Ro++;
                        if(Ro>MRo)
                            MRo = Ro;
                    }

                    if(number[k]==number[k-1]+1)    //스트레이트 체크
                    {
                        St++;
                    }
                    else
                    {
                        if(St==4 && k==4 && number[4]==12 && number[0]==0)  //백스트레이트 체크
                            combi[7] = true;
                        else
                            St = 1;
                    }
                }                       //숫자 체크

                if(MRo==4)                  //포카드
                    combi[0] = true;
                else if(MRo==3 && Ch==1)    //풀하우스
                    combi[6] = true;
                else if(St==5)             //스트레이트
                    combi[4] = true;
                else if(MRo==3 && Ch==2)    //트리플
                    combi[1] = true;
                else if(MRo==2 && Ch==2)    //투페어
                    combi[5] = true;
                else if(MRo==2 && Ch==3)    //원페어
                    combi[2] = true;
                
                /*
                    0: 포카드
                    1: 트리플
                    2: 원페어
                    3: 플러시
                    4: 스트레이트
                    5: 투페어
                    6: 풀하우스
                    7: 백스트레이트
                */

                //족보 순위
                if(combi[3])//플러시
                {
                    if(combi[4])//스트레이트
                    {
                        if(number[0]==8)//탑 A
                            level = 13;             //로티플
                        else//아니면
                            level = 11;             //스트레이트 플러시
                    }
                    else if(combi[7])//백스트레이트
                    {
                        level = 12;                 //백티플
                    }
                    else//아니면
                    {
                        level = 8;                  //플러시
                    }
                }
                else if(combi[0])//포카드
                {
                    level = 10;                     //포카드
                }
                else if(combi[6])//풀하우스
                {
                    level = 9;                      //풀하우스
                }
                else if(combi[4])//스트레이트
                {
                    if(number[0]==8)//탑 A
                        level = 7;                  //마운틴(로열 스트레이트)
                    else//아니면
                        level = 5;                  //스트레이트
                }
                else if(combi[7])//백스트레이트
                {
                    level = 6;                      //백스트레이트
                }
                else if(combi[1])//트리플
                {
                    level = 4;                      //트리플
                }
                else if(combi[5])//투페어
                {
                    level = 3;                      //투페어
                }
                else if(combi[2])//원페어
                {
                    level = 2;                      //원페어
                }
                else//탑
                {
                    level = 1;                      //탑
                }
                
                if(level >= bestLevel)       //현재 덱이 더 높거나 같으면
                {
                    if(level == bestLevel)     //만약 같은 덱이면
                    {
                        if(ifSameLevel(bestDeck, tempDeck))
                            continue;
                    }
                    bestLevel = level;      //레벨 설정
                    for(k=0;k<5;k++)
                    {
                        bestDeck[k] = tempDeck[k];      //반환 덱 업데이트
                    }
                    bestDeck[5] = bestLevel;        //덱 레벨 첨부
                }
            }
        }
        //print("found best deck");
        return bestDeck;
    }

    private bool ifSameLevel(int[] deck1, int[] deck2)      //덱1이 덱2보다 높으면 참
    {
        bool deck1IsBetter = true;
        bool ASN = true;    //all same number
        int[] deck_1 = new int[5];
        int[] deck_2 = new int[5];
        int i=0, j=0, k=0, tmp=0, mindex=0;
        for(i=0;i<5;i++)        //덱 복사
        {
            deck_1[i] = deck1[i];
            deck_2[i] = deck2[i];
        }

        for(i=0;i<4;i++)        //복사 덱 정렬(deck_1)
        {
            mindex = i;
            for(j=i+1;j<5;j++)
            {
                if(deck_1[mindex]%13 >= deck_1[j]%13)       //숫자 비교
                {
                    if(deck_1[mindex]%13 == deck_1[j]%13)           //숫자 같으면
                    {
                        if(deck_1[mindex]/13 < deck_1[j]/13)        //문양비교
                        {
                            mindex = j;
                        }
                    }
                    else
                    {
                        mindex = j;
                    }
                }
            }
            tmp = deck_1[mindex];
            deck_1[mindex] = deck_1[i];
            deck_1[i] = tmp;
        }

        for(i=0;i<4;i++)        //복사 덱 정렬(deck_2)
        {
            mindex = i;
            for(j=i+1;j<5;j++)
            {
                if(deck_2[mindex]%13 >= deck_2[j]%13)       //숫자 비교
                {
                    if(deck_2[mindex]%13 == deck_2[j]%13)           //숫자 같으면
                    {
                        if(deck_2[mindex]/13 < deck_2[j]/13)        //문양비교
                        {
                            mindex = j;
                        }
                    }
                    else
                    {
                        mindex = j;
                    }
                }
            }
            tmp = deck_2[mindex];
            deck_2[mindex] = deck_2[i];
            deck_2[i] = tmp;
        }


        switch(deck1[5])
        {
            case 13:        //스트레이트인 경우 (로티플, 백티플, 스플, 마운틴, 스트레이트)
            case 11:
            case 7:
            case 5:
                if(deck_1[4]%13 < deck_2[4]%13)  //숫자먼저 보고
                    deck1IsBetter = false;
                else if(deck_1[4]%13 == deck_2[4]%13)    //숫자가 같으면
                {
                    for(i=4;i>=0;i--)               //큰놈부터
                    {
                        if(deck_1[i]/13 != deck_2[i]/13)        //다른지 확인 후, 
                        {
                            if(deck_1[i]/13 > deck_2[i]/13)     //문양보고
                            {
                                deck1IsBetter = false;
                            }
                            break;
                        }
                    }
                }
                break;
            case 8:         //플러시인 경우
                if(deck_1[4]/13 > deck_2[4]/13)  //문양먼저 보고
                    deck1IsBetter = false;
                else if(deck_1[4]/13 == deck_2[4]/13)    //문양이 같으면
                {
                    for(i=4;i>=0;i--)                               //큰놈부터 순서대로
                    {
                        if(deck_1[i]%13 != deck_2[i]%13)        //다른지 확인 후, 
                        {
                            if(deck_1[i]%13 < deck_2[i]%13)  //숫자보고
                            {
                                deck1IsBetter = false;
                            }
                            break;
                        }
                    }
                }
                break;
            case 10:        //포카드인 경우
                if(deck_1[3]%13 < deck_2[3]%13)     //포타드의 숫자 비교
                    deck1IsBetter = false;
                else if(deck_1[3]%13 == deck_2[3]%13)       //같다면
                {
                    int res_1 = (deck_1[3]%13 == deck_1[4]%13)?deck_1[0]:deck_1[4];
                    int res_2 = (deck_2[3]%13 == deck_2[4]%13)?deck_2[0]:deck_2[4];

                    if(res_1%13 < res_2%13)                 //남은 놈의 숫자 비교 
                        deck1IsBetter = false;
                    else if(res_1%13 == res_2%13)           //숫자가 같다면
                    {
                        if(res_1/13 > res_2/13)                       //문양비교
                            deck1IsBetter = false;
                    }
                }
                break;
            case 9:         //풀하우스인 경우
                if(deck_1[2]%13 < deck_2[2]%13)             //트리플 숫자 비교 
                    deck1IsBetter = false;
                else if(deck_1[2]%13 == deck_2[2]%13)           //같으면
                {
                    int d_1 = (deck_1[2]%13 == deck_1[3]%13)?1:4;
                    int d_2 = (deck_2[2]%13 == deck_2[3]%13)?1:4;

                    if(deck_1[d_1]%13 < deck_2[d_2]%13)             //원페어 숫자 비교
                        deck1IsBetter = false;
                    else if(deck_1[d_1]%13 == deck_2[d_2]%13)             //숫자가 같다면
                    {
                        for(i=0;i<5;i++)                            //트리플 큰 문양부터 돌아가며
                        {
                            if(deck_1[((d_1+3)%5-i+5)%5]/13 != deck_2[((d_2+3)%5-i)%5]/13)  //다른지 확인 후, 
                            {
                                if(deck_1[((d_1+3)%5-i+5)%5]/13 > deck_2[((d_2+3)%5-i)%5]/13)  //문양 비교
                                {
                                    deck1IsBetter = false;
                                }
                                break;
                            }
                        }
                    }
                }
                break;
            case 12:            //백티플인 경우
            case 6:             //백스트레이트인 경우
                for(i=4;i>=0;i--)                               //5부터
                {
                    if(deck_1[(i+4)%5]/13 != deck_2[(i+4)%5]/13)    //다른지 확인 후, 
                    {
                        if(deck_1[(i+4)%5]/13 > deck_2[(i+4)%5]/13)       //A까지 문양 비교
                        {
                            deck1IsBetter = false;
                        }
                        break;
                    }
                }
                break;
            case 4:         //트리플인 경우
                if(deck_1[2]%13 < deck_2[2]%13)         //트리플 숫자 비교 
                    deck1IsBetter = false;
                else if(deck_1[2]%13 == deck_2[2]%13)       //숫자가 같으면
                {
                    int[] TSort_1 = new int[5];
                    int[] TSort_2 = new int[5];
                    int jt = 2, kt = 2;
                    j=0; k=0;
                    for(i=0;i<5;i++)                       //트리플이 위에 오도록 재배열
                    {
                        if(deck_1[2]%13 != deck_1[i]%13)
                        {
                            TSort_1[j++] = deck_1[i];
                        }
                        else
                        {
                            TSort_1[jt++] = deck_1[i];
                        }

                        if(deck_2[2]%13 != deck_2[i]%13)
                        {
                            TSort_2[k++] = deck_2[i];
                        }
                        else
                        {
                            TSort_2[kt++] = deck_2[i];
                        }
                    }
                    for(i=4;i>=0;i--)                            //재배열 한 놈을 큰놈부터
                    {
                        if(ASN)             //모든 숫자가 같은지 확인
                        {
                            ASN = TSort_1[i]%13 == TSort_2[i]%13;

                            if(TSort_1[i]%13 < TSort_2[i]%13)           //숫자 비교
                            {
                                deck1IsBetter = false;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(ASN)                 //만약 모든 숫자가 같다면
                    {
                        for(i=4;i>=0;i--)                            //큰놈부터
                        {
                            if(TSort_1[i]/13 != TSort_2[i]/13)      //다른지 확인 후, 
                            {
                                if(TSort_1[i]/13 > TSort_2[i]/13)           //문양 비교
                                {
                                    deck1IsBetter = false;
                                }
                                break;
                            }
                        }
                    }
                }
                break;
            case 3:         //투페어인 경우
                int[] BSort_1 = new int[5];                     //나머지가 마지막에 오도록 정렬
                int[] BSort_2 = new int[5];
                int res=0;

                if(deck_1[0]%13 != deck_1[1]%13)                //덱1 정렬
                    res = deck_1[0];
                else if(deck_1[3]%13 != deck_1[4]%13)
                    res = deck_1[4];
                else
                    res = deck_1[2];
                BSort_1[0] = res;
                j=0;
                for(i=1;i<5;i++)
                {
                    if(deck_1[j] == res)
                    {
                        j++;
                        i--;
                        continue;
                    }
                    BSort_1[i] = deck_1[j];
                    j++;
                }

                if(deck_2[0]%13 != deck_2[1]%13)                //덱 2 정렬
                    res = deck_2[0];
                else if(deck_2[3]%13 != deck_2[4]%13)
                    res = deck_2[4];
                else
                    res = deck_2[2];
                BSort_2[0] = res;
                j=0;
                for(i=1;i<5;i++)
                {
                    if(deck_2[j] == res)
                    {
                        j++;
                        i--;
                        continue;
                    }
                    BSort_2[i] = deck_2[j];
                    j++;
                }

                for(i=4;i>=0;i--)
                {
                    if(ASN)
                    {
                        ASN = BSort_1[i]%13 == BSort_2[i]%13;
                        if(BSort_1[i]%13 < BSort_2[i]%13)           //숫자 비교
                        {
                            deck1IsBetter = false;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if(ASN)                         //모든 숫자가 같댜면
                {
                    for(i=4;i>=0;i--)
                    {
                        if(BSort_1[i]/13 != BSort_2[i]/13)      //다른지 확인 후, 
                        {
                            if(BSort_1[i]/13 > BSort_2[i]/13)       //문양비교
                            {
                                deck1IsBetter = false;
                            }
                            break;
                        }
                    }
                }
                break;
            case 2:                     //원페어인 경우
                int[] OSort_1 = new int[5];             //원페어를 위로 보내 정렬
                int[] OSort_2 = new int[5];
                
                j=0;
                for(i=0;i<5;i++)                //deck_1 원페어 찾기
                {
                    if(i!=4 && deck_1[i]%13 == deck_1[i+1]%13)
                    {
                        OSort_1[3] = deck_1[i];
                        i++;
                        OSort_1[4] = deck_1[i];
                        continue;
                    }
                    OSort_1[j++] = deck_1[i];
                }

                j=0;
                for(i=0;i<5;i++)                //deck_2 원페어 찾기
                {
                    if(i!=4 && deck_2[i]%13 == deck_2[i+1]%13)
                    {
                        OSort_2[3] = deck_2[i];
                        i++;
                        OSort_2[4] = deck_2[i];
                        continue;
                    }
                    OSort_2[j++] = deck_2[i];
                }
                
                for(i=4;i>=0;i--)
                {
                    if(ASN)
                    {
                        ASN = OSort_1[i]%13 == OSort_2[i]%13;
                        if(OSort_1[i]%13 < OSort_2[i]%13)       //숫자비교
                        {
                            deck1IsBetter = false;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if(ASN)
                {
                    for(i=4;i>=0;i--)
                    {
                        if(OSort_1[i]/13 != OSort_2[i]/13)          //다른지 확인 후, 
                        {
                            if(OSort_1[i]/13 > OSort_2[i]/13)           //문양비교
                            {
                                deck1IsBetter = false;
                            }
                            break;
                        }
                    }
                }
                break;
            case 1:         //탑인 경우
                for(i=4;i>=0;i--)
                {
                    if(ASN)
                    {
                        ASN = deck_1[i]%13 == deck_2[i]%13;
                        if(deck_1[i]%13 < deck_2[i]%13)
                        {
                            deck1IsBetter = false;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if(ASN)
                {
                    for(i=4;i>=0;i--)
                    {
                        if(deck_1[i]/13 > deck_2[i]/13)     //다른지 확인 후, 
                        {
                            if(deck_1[i]/13 > deck_2[i]/13)
                            {
                                deck1IsBetter = false;
                            }
                            break;
                        }
                    }
                }
                break;
            default:
                break;
        }
        //print("compare done");
        return deck1IsBetter;
    }
}

public class PlayerStat
{
    public int Token;
    public int[] card = new int[3];
    public int[] deck = new int[7];
    public int[] myBestDeck = new int[6];
    public int MyBet;
    public int MyTotalBet;
    public bool die;
    public bool disable;

    public PlayerStat(int token)
    {
        Token = token;
        MyBet = 0;
    }
}