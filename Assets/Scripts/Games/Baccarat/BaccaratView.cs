using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Pool;
using DG.Tweening;
using Games.Baccarat;
using Games.Card;
using Globals;
using Google.Protobuf;
using Proto;
using Nakama;
using Spine.Unity;
using TMPro;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;
using GameState = Proto.GameState;
using Utility = Globals.Utility;

public class BaccaratView : BaseGameView
{
     // ================== Prefabs & Containers ==================
    [Header("Prefabs & Containers")]
    [SerializeField] public BaccaratChip chipPref;
    [SerializeField] public GameObject chipContainer;
    [SerializeField] public GameObject cardDeckB;
    [SerializeField] public GameObject cardDeckP;
    [SerializeField] public GameObject buttonBetBaccarat;
    [SerializeField] public List<GameObject> boxBetBaccarat = new List<GameObject>();
    [SerializeField] public List<GameObject> listPot = new List<GameObject>();
    [SerializeField] public GameObject buttonMenu;
    [SerializeField] public GameObject clock;
    [SerializeField] public GameObject scorePlayer;
    [SerializeField] public GameObject scoreBanker;
    [SerializeField] public GameObject popupHistoryPrefab;
    [SerializeField] public BaccaratPlayerView currentPlayerView;
    [SerializeField] public Transform cardContainer;
    // ================== Cards ==================
    [Header("Cards")]
    [SerializeField] public List<CardModel> listCardB = new List<CardModel>();
    [SerializeField] public List<CardModel> listCardP = new List<CardModel>();

    // ================== Buttons ==================
    [Header("Buttons")]
    [SerializeField] public Button btnDoubleBet;
    [SerializeField] public Button btnRebet;
    [SerializeField] public List<Button> listChipBets = new List<Button>();

    // ================== Text Labels ==================
    [Header("Text Labels")]
    [SerializeField] public TextMeshProUGUI lbTimeBet;
    [SerializeField] public TextMeshProUGUI lbScorePlayer;
    [SerializeField] public TextMeshProUGUI lbScoreBanker;
    [SerializeField] public List<TextMeshProUGUI> listBoxBet = new List<TextMeshProUGUI>();
    [SerializeField] public List<TextMeshProUGUI> listBetContainer = new List<TextMeshProUGUI>();
    [SerializeField] public TextMeshProUGUI lb_waiting;
    [SerializeField] public TextMeshProUGUI lb_max_bet;
    [SerializeField] public TextMeshProUGUI lb_not_enough_gold;
    [SerializeField] public TextMeshProUGUI lb_his_banker;
    [SerializeField] public TextMeshProUGUI lb_his_player;
    [SerializeField] public TextMeshProUGUI lb_his_tie;

    // ================== Animations ==================
    [Header("Animations")]
    [SerializeField] public SkeletonGraphic ani_win;

    // ================== Private Runtime Data ==================
    [Header("Runtime Data")]
    private BaccaratHistory popupHistory;
    // private JObject saveDT;
    private BaccaratPlayerView playerViewBaccarat;

    private List<long> listValueChipBets = new List<long>();
    private List<BaccaratChip> chipPoolTG = new List<BaccaratChip>();
    private List<BaccaratChip> listChipInTable = new List<BaccaratChip>();
    private List<BaccaratChip> listChipsPay = new List<BaccaratChip>();

    private List<int> listCodeCardPlayer = new List<int>();
    private List<int> listCodeCardBanker = new List<int>();
    private List<int> listWinResult = new List<int>();
    private List<int> savePotLose = new List<int>();
    [HideInInspector] public List<int> listSaveHistory = new List<int>();

    private long betValue = 0;
    private int chipBetColorInx = 0;
    private int myChipBetColor = 0;

    private long[] listBet = { 0, 0, 0, 0, 0 };
    private long[] saveListBet = { 0, 0, 0, 0, 0 };
    private long[] saveListMyBet = { 0, 0, 0, 0, 0 };
    private long[] listMyBet = { 0, 0, 0, 0, 0 };
    private long[] listLastBet = { 0, 0, 0, 0, 0 };
    private readonly Vector3 dealCardPos = new Vector3(296, 301, 0);

    private readonly List<Vector3> listPosCardP = new List<Vector3>
    {
        new Vector3(-101, 171, 0),
        new Vector3(-172, 171, 0),
        new Vector3(-254, 171, 0),
    };
    private readonly List<Vector3> listPosCardB = new List<Vector3>
    {
        new Vector3(101, 171, 0),
        new Vector3(172, 171, 0),
        new Vector3(254, 171, 0),
    };
    private int scorePl = 0, scoreBk = 0, scorePl1 = 0, scoreBk1 = 0;
    private bool checkBeted = false, checkBetDouble = false;
    private int indexCard = 0;
    
    private void Awake()
    {
        LoadProfile();
        buttonBetBaccarat.SetActive(false);
        PoolService.Instance.Register(PrefabType.BaccaratCard, cardContainer , listCardP[0], 8, 10, 6);

    }

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        SetInfoBet(agTable);
    }

    private void LoadProfile()
    {
        currentPlayerView.id = User.userMain.userId;
        currentPlayerView.setAg(User.userMain.accountChip);
        currentPlayerView.setName(User.userMain.displayName);
        currentPlayerView.avatar_id = User.userMain.avatarId;
        currentPlayerView.vipLevel = User.userMain.vipLevel;
        thisPlayer.Id = User.userMain.userId;
        thisPlayer.Wallet = User.userMain.accountChip.ToString();
        thisPlayer.AvatarId = User.userMain.avatarId;
        thisPlayer.VipLevel = User.userMain.vipLevel;
        thisPlayer.UserName = User.userMain.displayName;
    }

    #region Hander Api
    
    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        var data  = BaccaratUpdateDesk.Parser.ParseFrom(matchState.State);
        var error = Error.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateTable " + data);

        if (error != null)
        {
            if (error.ErrorType == ErrorType.ChipNotEnough)
            {
                Debug.Log("Not enough chip ");
            }
            
        }else if (data != null)
        {
            if (data.IsUpdateDeskCell)
            {
                foreach (var cellInfo in data.DeskCells)
                {
                    switch (cellInfo.Cell)
                    {
                        case BaccaratBetCell.BaccaratCellTie:
                            boxBetBaccarat[0].SetActive(true);
                            listBoxBet[0].text = Utility.FormatNumber(cellInfo.Chips);
                            break;
                        case BaccaratBetCell.BaccaratCellPlayer:
                            boxBetBaccarat[1].SetActive(true);
                            listBoxBet[1].text = Utility.FormatNumber(cellInfo.Chips);
                            break;
                        case BaccaratBetCell.BaccaratCellBanker:
                            boxBetBaccarat[2].SetActive(true);
                            listBoxBet[2].text = Utility.FormatNumber(cellInfo.Chips);
                            break;
                        case BaccaratBetCell.BaccaratCellPlayerPair:
                            boxBetBaccarat[3].SetActive(true);
                            listBoxBet[3].text = Utility.FormatNumber(cellInfo.Chips);
                            break;
                        case BaccaratBetCell.BaccaratCellBankerPair:
                            boxBetBaccarat[4].SetActive(true);
                            listBoxBet[4].text = Utility.FormatNumber(cellInfo.Chips);
                            break;
                    }
                    
                }
            }

            if (data.IsUpdateUserBet)
            {
                
            }
        }
        
    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        UpdateListPlayer(updateTable.Players.ToList());
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
        var baccaratUpdateDeal = BaccaratUpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateDeal " + baccaratUpdateDeal);
        CardModel cardModel = PoolService.Instance.Get<CardModel>(PrefabType.BaccaratCard);
        
        cardModel.HideCardPusoy();
        cardModel.transform.localPosition = dealCardPos;
        cardModel.gameObject.SetActive(false);
        cardModel.transform.localScale = Vector3.one * 0.4f;

        if (baccaratUpdateDeal.IsPlayer)
        {
            cardModel.SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);
            listCardP[indexCard].SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);

            int i = indexCard;
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                cardModel.transform.localPosition = dealCardPos;
                cardModel.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                cardModel.transform.rotation = Quaternion.Euler(0, 0, 68);
                cardModel.gameObject.SetActive(true);
            });
            seq.Append(cardModel.transform.DOLocalMove(
                (dealCardPos + listCardP[i].transform.localPosition) / 2, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0, 0.2f));
            seq.Join(cardModel.transform.DORotate(new Vector3(0, 0, 34), 0.2f));
            seq.AppendCallback(() =>
            {
                listCardP[i].ShowCardPusoy();
                cardModel.ShowCardPusoy();
            });
            seq.Append(cardModel.transform.DOLocalMove(listCardP[i].transform.localPosition, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0.4f, 0.2f));
            seq.Join(cardModel.transform.DORotate(Vector3.zero, 0.2f));
            seq.AppendCallback(() =>
            { 
                listCardP[i].gameObject.SetActive(true);
                PoolService.Instance.Release(PrefabType.BaccaratCard, cardModel);
            });
        }
        else
        {
            cardModel.SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);
            listCardB[indexCard].SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);

            int i = indexCard;
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                cardModel.transform.position = dealCardPos;
                cardModel.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                cardModel.transform.rotation = Quaternion.Euler(0, 0, 68);
                cardModel.gameObject.SetActive(true);
            });
            seq.Append(cardModel.transform.DOLocalMove(
                (dealCardPos + listCardB[i].transform.localPosition) / 2, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0, 0.2f));
            seq.Join(cardModel.transform.DORotate(new Vector3(0, 0, 34), 0.2f));
            seq.AppendCallback(() =>
            {
                listCardB[i].ShowCardPusoy();
                cardModel.ShowCardPusoy();
            });
            seq.Append(cardModel.transform.DOLocalMove(listCardB[i].transform.localPosition, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0.4f, 0.2f));
            seq.Join(cardModel.transform.DORotate(Vector3.zero, 0.2f));
            seq.AppendCallback(() =>
            {
                listCardB[i].gameObject.SetActive(true);
                PoolService.Instance.Release(PrefabType.BaccaratCard, cardModel);
            });
            indexCard++;
        }
        
    }
    
    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
        var updateGameState = UpdateGameState.Parser.ParseFrom(matchState.State);
        stateGame = updateGameState.State;
        if (stateGame != GameState.Play)
        {
            buttonBetBaccarat.SetActive(false);
        }
        switch (updateGameState.State)
        {
            case GameState.Idle:
                Debug.Log("HandleUpdateGameState Idle "+updateGameState.ToString());
                break;
            case GameState.Matching:
                Debug.Log("HandleUpdateGameState Matching "+ updateGameState.ToString());
                break;
            case GameState.Preparing:
                SetLoopLbWaiting();
                Debug.Log("HandleUpdateGameState Preparing " + updateGameState.ToString());
                break;
            case GameState.Play:
                Debug.Log("HandleUpdateGameState Play " + updateGameState.ToString());
                if (updateGameState.CountDown == 10)
                {
                    lb_waiting.gameObject.SetActive(false);
                    clock.gameObject.SetActive(true);
                    buttonBetBaccarat.SetActive(true);
                    if (popupHistory != null)
                    {
                        // popupHistory.onClickClose(true);
                    }

                    SetDisplayBet();
                    SetStatusButtonsBet(!checkBeted, checkBeted);
                    // for (int i = 0; i < players.Count; i++)
                    // {
                    //     players[i].playerView.setTurn(true, getInt(data, "finishAfter") / 1000 - 1);
                    // }
                }
                SetTextTime((int) updateGameState.CountDown);
                break;
            case GameState.Reward:
                Debug.Log("HandleUpdateGameState Reward " + updateGameState.ToString());
                break;
            case GameState.Finish:
                Debug.Log("HandleUpdateGameState Finish " + updateGameState.ToString());
                break;
            
        }
    }

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var balanceResult = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("BaccaratUpdateWallet " + balanceResult);
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        base.HandleUpdateKickOffTheTable(matchState);
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var baccaratGameFinish = BaccaratGameFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("BaccaratGameFinish " + baccaratGameFinish);
        
        // Show game result
        ShowGameResult(baccaratGameFinish);
        
        // Update history
        UpdateHistory(baccaratGameFinish);
        
        // Handle chip payouts
        HandleChipPayouts(baccaratGameFinish);
    }

    private void ShowGameResult(BaccaratGameFinish result)
    {
        // Determine winner
        BaccaratBetCell winner = result.WinCells[0];
  
        
        // Show win effect
        if (ani_win != null)
        {
            ani_win.gameObject.SetActive(true);
            ani_win.AnimationState.SetAnimation(0, "win", false);
        }
        
        // Update score display
        lbScorePlayer.text = result.Hand.Player.Point.ToString();
        lbScoreBanker.text = result.Hand.Banker.Point.ToString();
        
        Debug.Log($"Game finished - Winner: {winner}, Player: {result.Hand.Player.Point}, Banker: {result.Hand.Banker.Point}");
    }

    private void UpdateHistory(BaccaratGameFinish result)
    {
        // // Add to history list
        // int historyCode = 0;
        // if (result.PlayerScore > result.BankerScore)
        // {
        //     historyCode = 1; // Player win
        // }
        // else if (result.BankerScore > result.PlayerScore)
        // {
        //     historyCode = 2; // Banker win
        // }
        // else
        // {
        //     historyCode = 3; // Tie
        // }
        //
        // listSaveHistory.Add(historyCode);
        //
        // // Update history display
        // UpdateHistoryDisplay();
    }

    private void UpdateHistoryDisplay()
    {
        // Update history labels
        int bankerWins = listSaveHistory.Count(x => x == 2);
        int playerWins = listSaveHistory.Count(x => x == 1);
        int tieWins = listSaveHistory.Count(x => x == 3);
        
        lb_his_banker.text = bankerWins.ToString();
        lb_his_player.text = playerWins.ToString();
        lb_his_tie.text = tieWins.ToString();
    }

    private void HandleChipPayouts(BaccaratGameFinish result)
    {
        // // Determine winning bets
        // listWinResult.Clear();
        //
        // if (result.PlayerScore > result.BankerScore)
        // {
        //     listWinResult.Add(1); // Player bet wins
        // }
        // else if (result.BankerScore > result.PlayerScore)
        // {
        //     listWinResult.Add(2); // Banker bet wins
        // }
        // else
        // {
        //     listWinResult.Add(0); // Tie bet wins
        // }
        //
        // // Handle pair bets
        // if (result.PlayerPair)
        // {
        //     listWinResult.Add(3); // Player pair wins
        // }
        // if (result.BankerPair)
        // {
        //     listWinResult.Add(4); // Banker pair wins
        // }
        //
        // // Animate chip payouts
        // PayChipWin();
    }
    
    #endregion
    
    private void ClearCards()
    {
        foreach (var card in listCardP)
        {
            card.gameObject.SetActive(false);
        }
        foreach (var card in listCardB)
        {
            card.gameObject.SetActive(false);
        }
    }
    
    private void SetLoopLbWaiting()
    {
        lb_waiting.gameObject.SetActive(true);
        string text1 = lb_waiting.text + ".";
        string text2 = lb_waiting.text + "..";
        string text3 = lb_waiting.text + "...";

        Sequence textSequence = DOTween.Sequence()
            .AppendInterval(1.0f)
            .AppendCallback(() => lb_waiting.text = text1)
            .AppendInterval(1.0f)
            .AppendCallback(() => lb_waiting.text = text2)
            .AppendInterval(1.0f)
            .AppendCallback(() => lb_waiting.text = text3);
        textSequence.SetLoops(-1);
    }
    
    private void SetTextTime(int time)
    {
        lbTimeBet.SetText(time.ToString());
        if (time == 3) Config.Vibration();
    }
    
    public void SetDisplayBet()
    {
        //long betValid = 0;
        bool check = false;
        Debug.Log("t "+thisPlayer.Wallet);
        for (int i = 4; i >= 0; i--)
        {
            if (listValueChipBets[i] > long.Parse(thisPlayer.Wallet))
            {
                listChipBets[i].interactable = false;
                listChipBets[i].transform.localPosition = new Vector2(listChipBets[i].transform.localPosition.x, -321);
                listChipBets[i].transform.Find("Border").gameObject.SetActive(false);
                check = true;
            }
            else
            {
                // break
                listChipBets[i].interactable = true;
                if (stateGame == GameState.Play && check && myChipBetColor >= i)
                {
                    listChipBets[i].transform.Find("Border").gameObject.SetActive(true);
                    betValue = listValueChipBets[i];
                    check = false;
                }
                //betValid = listValueChipBets[i];
            }
        }
    }
    
    public void SetStatusButtonsBet(bool btnrebet, bool btndouble)
    {
        if (listLastBet.All(element => element == 0))
        {
            btnrebet = false;
        }
        Debug.Log("setStatusButtonsBet:" + btndouble);
        btnRebet.interactable = btnrebet;
        btnDoubleBet.interactable = btndouble;
    }
    
    public void OnClickChipBet(int chipBet)
    {
        // SoundManager.instance.soundClick();

        betValue = listValueChipBets[chipBet];
        for (int i = 0; i < 5; i++)
        {
            listChipBets[i].transform.Find("Border").gameObject.SetActive(i == chipBet);
        }
        chipBetColorInx = chipBet;
        myChipBetColor = chipBet;
    }
    
    public void OnClickDoubleBet()
    {
        // SoundManager.instance.soundClick();
        int timeleft = int.Parse(lbTimeBet.text);
        if (timeleft > 1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (listMyBet[i] > 0 && listMyBet[i] <= agTable * 100)
                {
                    checkBetDouble = true;
                    // SocketSend.sendBetBaccarat(listMyBet[i], getBetGate2(i + 1));
                    BaccaratPlayerBet baccaratPlayerBet = new BaccaratPlayerBet
                    {
                        ActionType = BaccaratBetActionType.BaccaratBetDouble
                    };
                    BaccaratBet baccaratBet = new BaccaratBet
                    {
                        Chips = listMyBet[i],
                        Cell = GetBetCellFromIndex(i)
                    };
                    baccaratPlayerBet.Bets.Add(baccaratBet);
                    DataSender.SendMatchState((long)OpCodeRequest.Bet, baccaratPlayerBet.ToByteArray());
                }
            }
        }
    }

    public void OnClickRebet()
    {
        // SoundManager.instance.soundClick();

        int timeleft = int.Parse(lbTimeBet.text);
        if (timeleft > 1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (listLastBet[i] > 0)
                {
                    checkBeted = true;
                    // SocketSend.sendBetBaccarat(listLastBet[i], getBetGate2(i + 1));
                    BaccaratPlayerBet baccaratPlayerBet = new BaccaratPlayerBet
                    {
                        ActionType = BaccaratBetActionType.BaccaratBetRebet
                    };
                    BaccaratBet baccaratBet = new BaccaratBet
                    {
                        Chips = listLastBet[i],
                        Cell = GetBetCellFromIndex(i)
                    };
                    baccaratPlayerBet.Bets.Add(baccaratBet);
                    DataSender.SendMatchState((long)OpCodeRequest.Bet, baccaratPlayerBet.ToByteArray());
                }
            }
        }
    }
    
    public void SetInfoBet(int m)
    {
        listValueChipBets = new List<long> { m, m * 5, m * 10, m * 50, m * 100 };
        for (int i = 0; i < 5; i++)
        {
            listChipBets[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = Utility.FormatMoney(listValueChipBets[i], true);
        }
        listChipBets[0].transform.Find("Border").gameObject.SetActive(true);
        betValue = listValueChipBets[0];
    }
    
    public void UpdateStatePot(){
        for (int i = 0; i < 5; i++)
        {
            listBoxBet[i].text = listMyBet[i] > 0 ? Utility.FormatMoney(listMyBet[i], true) : "";
            listBetContainer[i].text = listBet[i] > 0 ? Utility.FormatMoney(listBet[i], true) : "";
            boxBetBaccarat[i].gameObject.SetActive(true && listMyBet[i] > 0);
        }
    }

    public void PayChipWin()
    {

        Vector2 posModel = new Vector2(0, 300);
        for (int i = 0; i < listChipInTable.Count; i++)
        {
            var chip = listChipInTable[i];
            if (listWinResult.Contains(chip.gateId))
            {
                // SoundManager.instance.playEffectFromPath(Globals.SOUND_GAME.THROW_CHIP);

                // ChipBaccarat chip1 = getChip(chip.chipSprite);
                // chip1.transform.localPosition = posModel;
                // Vector2 posChip = chip.transform.localPosition;
                // posChip.x += 5f;
                // posChip.y += 5f;
                // chip1.transform.DOLocalMove(posChip, 0.5f);
                // chip1.idPl = chip.idPl;
                // chip1.gateId = chip.gateId;
                // chip1.chipValue = chip.chipValue;
                // chip1.transform.GetComponentInChildren<TextMeshProUGUI>().text = Utility.FormatMoney(chip.chipValue, true);
                // listChipsPay.Add(chip1);
            }
        }
        listChipInTable.AddRange(listChipsPay);
        listChipsPay.Clear();
        DOTween.Sequence()
            .AppendInterval(0.7f)
            .AppendCallback(() =>
            {
                for (int i = 0; i < listChipInTable.Count; i++)
                {
                    var chip = listChipInTable[i];
                    if (listWinResult.Contains(chip.gateId))
                    {
                        // SoundManager.instance.playEffectFromPath(Globals.SOUND_GAME.WIN);
                        // get player bet win position
                        // Player playerBet = getPlayerWithID(chip.idPl);
                        // PlayerViewBaccarat plView = (PlayerViewBaccarat)playerBet.playerView;
                        // Vector2 posPlayer = plView.transform.localPosition;
                        // chip.transform.DOLocalMove(posPlayer, 0.5f);
                    }
                }
            });
    }

    public void OnClickBet(int betArea)
    {
        if (betValue <= 0) return;
        if (stateGame != GameState.Play) return;
        
        // Create bet request
        BaccaratPlayerBet baccaratPlayerBet = new BaccaratPlayerBet
        {
            ActionType = BaccaratBetActionType.BaccaratBetNormalUnspecified
        };
        
        BaccaratBet baccaratBet = new BaccaratBet
        {
            Chips = betValue,
            Cell = GetBetCellFromArea(betArea)
        };
        
        baccaratPlayerBet.Bets.Add(baccaratBet);
        
        // Send bet to server
        DataSender.SendMatchState((long)OpCodeRequest.Bet, baccaratPlayerBet.ToByteArray());
        
        // Update UI
        checkBeted = true;
        UpdateStatePot();
        
        Debug.Log($"Placed bet: {betValue} on area {betArea}");
    }

    private BaccaratBetCell GetBetCellFromArea(int area)
    {
        switch (area)
        {
            case 0: return BaccaratBetCell.BaccaratCellTie;
            case 1: return BaccaratBetCell.BaccaratCellPlayer;
            case 2: return BaccaratBetCell.BaccaratCellBanker;
            case 3: return BaccaratBetCell.BaccaratCellPlayerPair;
            case 4: return BaccaratBetCell.BaccaratCellBankerPair;
            default: return BaccaratBetCell.BaccaratCellTie;
        }
    }

    private BaccaratBetCell GetBetCellFromIndex(int index)
    {
        switch (index)
        {
            case 0: return BaccaratBetCell.BaccaratCellTie;
            case 1: return BaccaratBetCell.BaccaratCellPlayer;
            case 2: return BaccaratBetCell.BaccaratCellBanker;
            case 3: return BaccaratBetCell.BaccaratCellPlayerPair;
            case 4: return BaccaratBetCell.BaccaratCellBankerPair;
            default: return BaccaratBetCell.BaccaratCellTie;
        }
    }

    public void OnClickShowHistory()
    {
        if (popupHistory == null && popupHistoryPrefab != null)
        {
            popupHistory = Instantiate(popupHistoryPrefab, transform).GetComponent<BaccaratHistory>();
        }
        
        if (popupHistory != null)
        {
            popupHistory.gameObject.SetActive(true);
            // popupHistory.SetHistoryData(listSaveHistory);
        }
    }

    public void ResetGame()
    {
        // Clear bets
        for (int i = 0; i < 5; i++)
        {
            listBet[i] = 0;
            listMyBet[i] = 0;
            listLastBet[i] = 0;
        }
        
        // Clear chips
        foreach (var chip in listChipInTable)
        {
            if (chip != null)
            {
                Destroy(chip.gameObject);
            }
        }
        listChipInTable.Clear();
        
        // Clear cards
        ClearCards();
        
        // Reset UI
        UpdateStatePot();
        SetDisplayBet();
        
        // Hide effects
        if (ani_win != null)
        {
            ani_win.gameObject.SetActive(false);
        }
        
        Debug.Log("Game reset completed");
    }
    
    public void EffDealCard(CardModel card, int code, Vector2 pos, bool axis, float time)
    {
        DOTween.Sequence()
            .AppendInterval(time)
            .AppendCallback(() =>
            {
                // SoundManager.instance.playEffectFromPath(Globals.SOUND_GAME.DISPATCH_CARD);
                // card.setTextureWithCode(0);
                card.gameObject.transform.localScale = new Vector2(0.38f, 0.4f);
                card.gameObject.transform.localPosition = new Vector2(296, 301);
                card.gameObject.transform.localEulerAngles = new Vector3(0, 0, 64.48f);

                card.gameObject.transform.DOLocalMove(pos, 0.5f);
                card.gameObject.transform.DOLocalRotate(new Vector3(0, 0, axis ? 0 : 90), 0.5f);
                card.gameObject.transform.SetAsLastSibling();
                card.gameObject.SetActive(true);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                card.gameObject.transform.DOScale(new Vector2(0f, 0.4f), 0.1f)
                    .OnComplete(() =>
                    {
                        // card.setTextureWithCode(code);
                        card.gameObject.transform.DOScale(new Vector2(0.38f, 0.4f), 0.1f);
                    });


            });
    }
}
