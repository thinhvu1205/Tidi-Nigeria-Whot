using System;
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
using Color = UnityEngine.Color;
using GameState = Proto.GameState;
using Utility = Globals.Utility;

public class BaccaratView : BaseDiceGameView
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
    [SerializeField] public BaccaratHistory popupHistoryPrefab;
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
    private BaccaratSimpleHistory baccaratSimpleHistory;
    private BaccaratPlayerView playerViewBaccarat;

    private List<long> listValueChipBets = new List<long>();
    private List<BaccaratChip> chipPoolTG = new List<BaccaratChip>();
    private List<BaccaratChip> listChipInTable = new List<BaccaratChip>();
    private List<BaccaratChip> listChipsPay = new List<BaccaratChip>();

    private List<int> listCodeCardPlayer = new List<int>();
    private List<int> listCodeCardBanker = new List<int>();
    private List<int> listWinResult = new List<int>();
    private List<int> savePotLose = new List<int>();
    [HideInInspector] public List<TypeWinBaccarat> listSaveHistory = new List<TypeWinBaccarat>();

    private long betValue = 0;
    private int chipBetColorInx = 0;
    private int myChipBetColor = 0;

    private long[] listBet = { 0, 0, 0, 0, 0 };
    private long[] saveListBet = { 0, 0, 0, 0, 0 };
    private long[] saveListMyBet = { 0, 0, 0, 0, 0 };
    private long[] listMyBet = { 0, 0, 0, 0, 0 };
    private long[] listLastBet = { 0, 0, 0, 0, 0 };
    private readonly Vector3 dealCardPos = new Vector3(296, 301, 0);


    private int scorePl = 0, scoreBk = 0, scorePl1 = 0, scoreBk1 = 0;
    private bool checkBeted = false, checkBetDouble = false;
    private int indexCard = 0;
    private Sequence waitingTextSequence;
    
    protected override void Awake()
    {
        base.Awake();
        LoadProfile();
        buttonBetBaccarat.SetActive(false);
        PoolService.Instance.Register(PrefabType.Card, cardContainer , listCardP[0], 8, 10, 6);
        PoolService.Instance.Register(PrefabType.ChipPlayerBaccarat, chipContainer.transform , chipPref, 20, 30, 15);
    }

    public override void OnDestroy()
    {
        // Clear all pools to ensure clean state
        PoolService.Instance.ClearPool<BaccaratChip>(PrefabType.ChipPlayerBaccarat);
        PoolService.Instance.ClearPool<CardModel>(PrefabType.Card);
    }
    

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        SetInfoBet(MarkUnit);
    }

    private void LoadProfile()
    {
        thisPlayer.id = User.userMain.userId;
        thisPlayer.wallet = User.userMain.accountChip.ToString();
        thisPlayer.avatar_id = User.userMain.avatarId;
        thisPlayer.vipLevel = User.userMain.vipLevel;
        thisPlayer.user_name = User.userMain.displayName;
    }

    #region Hander Api
    
    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        var data  = BaccaratUpdateDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateTable " + data);

        if (data.Error != null && data.Error.ErrorType != ErrorType.Unspecified)
        {
            if (data.Error.ErrorType == ErrorType.ChipNotEnough)
            {
                Debug.Log("Not enough chip ");
            }
            
        }else
        {
            if (data.IsUpdateDeskCell)
            {
                foreach (var cellInfo in data.DeskCells)
                {
                    switch (cellInfo.Cell)
                    {
                        case BaccaratBetCell.BaccaratCellPlayer:
                            boxBetBaccarat[0].SetActive(true);
                            listBoxBet[0].text = Utility.FormatNumber(cellInfo.Chips);
                            break;
                        case BaccaratBetCell.BaccaratCellBanker:
                            boxBetBaccarat[1].SetActive(true);
                            listBoxBet[1].text = Utility.FormatNumber(cellInfo.Chips);
                            break;
                        case BaccaratBetCell.BaccaratCellTie:
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
                if (userIdToView.TryGetValue(data.UserBet.UserId, out var playerView))
                {
                    if (data.UserBet.UserId == User.userMain.userId)
                    {
                        foreach (var infoBet in data.UserBet.Bets)
                        {
                            int i = (int)infoBet.Cell - 1;
                            listMyBet[i] += infoBet.Chips;
                        }
                    }

                    foreach (var infoBet in data.UserBet.Bets)
                    {
                        playerView.setAg(playerView.agCurrent - infoBet.Chips);
                        int i = (int)infoBet.Cell - 1;
                        BaccaratChip chip = PoolService.Instance.Get<BaccaratChip>(PrefabType.ChipPlayerBaccarat);
                        chipBetColorInx = listValueChipBets.IndexOf(infoBet.Chips);
                        chip.init(1, 0.4f);
                        chip.SetInfo(data.UserBet.UserId, i + 1, playerView.transform.localPosition, infoBet.Chips,
                            chipBetColorInx);
                        ChipMoveTo(chip, i);
                        listChipInTable.Add(chip);
                    }

                    buttonBetBaccarat.SetActive(true);
                    SetStatusButtonsBet(!checkBeted, checkBeted);
                    SetDisplayBet();
                    UpdateStatePot();

                }
            }

            if (data.IsUpdateGameHistory)
            {
                if (data.History != null)
                {
                    baccaratSimpleHistory= data.History;
                    listSaveHistory = data.DetailedHistory.ToList();
                    UpdateHistoryDisplay();
                }
            }
        }
    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateUserInTable " + updateTable);
        UpdatePosUserTable(updateTable);
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
        var baccaratUpdateDeal = BaccaratUpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateDeal " + baccaratUpdateDeal);

        if (indexCard == 0 && baccaratUpdateDeal.IsPlayer)
        {
            clock.SetActive(false);
            buttonBetBaccarat.SetActive(false);
            foreach (var btn in listPot)
            {
                btn.GetComponent<Button>().interactable = false;
            }
            SetStatusButtonsBet(!checkBeted, checkBeted);
        }
        
        CardModel cardModel = PoolService.Instance.Get<CardModel>(PrefabType.Card);
        cardModel.HideCardPusoy();
        cardModel.transform.localPosition = dealCardPos;
        cardModel.transform.localScale = new Vector2(0.38f, 0.4f);
        cardModel.transform.localEulerAngles = new Vector3(0, 0, 64.48f);
        cardModel.gameObject.SetActive(true);
        
        if (baccaratUpdateDeal.IsPlayer)
        {
            cardModel.SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);
            listCardP[indexCard].SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);

            int i = indexCard;
            if (i == 2)
            {
                scorePlayer.SetActive(false);
                scorePlayer.transform.localPosition = new Vector2(-338, 174);
            }
            Sequence seq = DOTween.Sequence();
      
            seq.Append(cardModel.transform.DOLocalMove(
                (dealCardPos + listCardP[i].transform.localPosition) / 2, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0, 0.2f));
            seq.Join(cardModel.transform.DOLocalRotate(new Vector3(0, 0, 32), 0.2f));
            seq.AppendCallback(() =>
            {
                listCardP[i].ShowCardPusoy();
                cardModel.ShowCardPusoy();
            });
            seq.Append(cardModel.transform.DOLocalMove(listCardP[i].transform.localPosition, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0.38f, 0.2f));
            seq.Join(cardModel.transform.DOLocalRotate(new Vector3(0, 0, i != 2 ? 0 : -90), 0.2f));
            seq.AppendCallback(() =>
            { 
                listCardP[i].gameObject.SetActive(true);
                PoolService.Instance.Release(PrefabType.Card, cardModel);
                if (i == 0) return;
                lbScorePlayer.text = baccaratUpdateDeal.Hands.Player.Point.ToString();
                scorePlayer.SetActive(true);
            });
        }
        else
        {
            cardModel.SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);
            listCardB[indexCard].SetData((int)baccaratUpdateDeal.Cards[0].Rank, (int)baccaratUpdateDeal.Cards[0].Suit);

            int i = indexCard;
            if (i == 2)
            {
                scoreBanker.SetActive(false);
                scoreBanker.transform.localPosition = new Vector2(338, 174);
            }
            Sequence seq = DOTween.Sequence();
 
            seq.Append(cardModel.transform.DOLocalMove(
                (dealCardPos + listCardB[i].transform.localPosition) / 2, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0, 0.2f));
            seq.Join(cardModel.transform.DOLocalRotate(new Vector3(0, 0, 32), 0.2f));
            seq.AppendCallback(() =>
            {
                listCardB[i].ShowCardPusoy();
                cardModel.ShowCardPusoy();
            });
            seq.Append(cardModel.transform.DOLocalMove(listCardB[i].transform.localPosition, 0.2f));
            seq.Join(cardModel.transform.DOScaleX(0.38f, 0.2f));
            seq.Join(cardModel.transform.DOLocalRotate(new Vector3(0, 0, i != 2 ? 0 : 90), 0.2f));
            seq.AppendCallback(() =>
            {
                listCardB[i].gameObject.SetActive(true);
                PoolService.Instance.Release(PrefabType.Card, cardModel);
                if (i == 0) return;
                lbScoreBanker.text = baccaratUpdateDeal.Hands.Banker.Point.ToString();
                scoreBanker.SetActive(true);
            });
            indexCard++;
        }
        
    }
    
    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
        var updateGameState = UpdateGameState.Parser.ParseFrom(matchState.State);
        GameState = updateGameState.State;
        if (GameState != GameState.Play)
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
                    if (waitingTextSequence != null && waitingTextSequence.IsActive())
                    {
                        waitingTextSequence.Kill();
                    }
                    clock.gameObject.SetActive(true);
                    buttonBetBaccarat.SetActive(true);
                    foreach (var btn in listPot)
                    {
                        btn.GetComponent<Button>().interactable = true;
                    }
                    if (popupHistory != null)
                    {
                        // popupHistory.onClickClose(true);
                    }

                    SetDisplayBet();
                    SetStatusButtonsBet(!checkBeted, checkBeted);
                    foreach (var player in playingPlayers)
                    {
                        if (userIdToView.TryGetValue(player.Id, out var playerView))
                        {
                            playerView.setTurn(true, updateGameState.CountDown);
                        }
                    }
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

    // Add balance storage
    private Dictionary<string, BalanceUpdate> balanceUpdates = new Dictionary<string, BalanceUpdate>();

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var balanceResult = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("BaccaratUpdateWallet " + balanceResult);
        
        // Store balance updates for later use
        balanceUpdates.Clear();
        foreach (var update in balanceResult.Updates)
        {
            balanceUpdates[update.UserId] = update;
        }
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        base.HandleUpdateKickOffTheTable(matchState);
        Debug.Log("HandleUpdateKickOffTheTable for baccarat "+ matchState.State.ToString());
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var baccaratGameFinish = BaccaratGameFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("BaccaratGameFinish " + baccaratGameFinish);
        
        // Clear previous win results
        listWinResult.Clear();
        Dictionary<string, int> playerLoseAmounts = new Dictionary<string, int>();
        
        foreach (var baccaratBetCell in baccaratGameFinish.WinCells)
        {
            listWinResult.Add((int)baccaratBetCell);
        }
        
        foreach (var chip in listChipInTable.Where(chip => !listWinResult.Contains(chip.gateId)))
        {
            playerLoseAmounts.TryAdd(chip.idPl, 0);
            playerLoseAmounts[chip.idPl] -= (int)chip.chipValue;
        }
        
        // Show Ani Banker & Player and  Points
        ShowEffWinType(baccaratGameFinish);
        
        // Show image light for button winning gates
        foreach (var index in listWinResult)
        {
            ShowEffWinGate(index);
        }
        
        Sequence sequence = DOTween.Sequence();
        
        // Collect Lose Bets - Move chips to top position
        sequence.AppendInterval(3f).AppendCallback(() =>
        {
            if (ani_win != null)
            {
                ani_win.gameObject.SetActive(false);
            }
            // Get collection position (usually top center of screen)
            Vector3 collectionPos = new Vector3(0, 300, 0);

            foreach (BaccaratChip chip in listChipInTable)
            {
                if (!listWinResult.Contains(chip.gateId))
                {
                    // Animate losing chips to collection position
                    chip.transform.DOLocalMove(collectionPos, 0.5f).SetEase(Ease.InSine);
                    chip.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        // Return chip to pool
                        PoolService.Instance.Release(PrefabType.ChipPlayerBaccarat, chip);
                        listChipInTable.Remove(chip);
                    });
                }
            }

            // Hiệu ứng flyMoney Lose cho tất cả player (bay 1 lần duy nhất per player)
            foreach (var kvp in playerLoseAmounts)
            {
                string idPl = kvp.Key;
                int loseAmount = kvp.Value;
                
                if (userIdToView.TryGetValue(idPl, out var playerObj) && loseAmount < 0){
                    playerObj.effectFlyMoney(loseAmount, 40);
                    Debug.Log($"Player {idPl} lose amount: {loseAmount}");
                }
            }
            
        });

        // Distribute Winning Chips to Players
        sequence.AppendInterval(1f).AppendCallback(PayChipWin);
        
        // update effect win money fly player
        sequence.AppendInterval(1f).AppendCallback(() =>
        {
            foreach (KeyValuePair<string, BalanceUpdate> kvp in balanceUpdates)
            {
                string playerId = kvp.Key;
                BalanceUpdate balanceUpdate = kvp.Value;

                Debug.Log($"Player ID: {playerId}, Balance Delta: {balanceUpdate.AmountChipAdd}");

                if (!userIdToView.TryGetValue(playerId, out var playerObj) || balanceUpdate.AmountChipAdd <= 0) continue;
                playerObj.effectFlyMoney(balanceUpdate.AmountChipAdd, 40);
                playerObj.setAg(balanceUpdate.AmountChipCurrent);
            }
        });
        
        // Reset and Return cards to deck
        sequence.AppendInterval(1.5f).AppendCallback(ResetGame);
        sequence.AppendInterval(1.0f).AppendCallback(SetLoopLbWaiting);
    }
    
    #endregion

    #region UI History
    
    private void UpdateHistoryDisplay()
    {
        // Update history labels
        int bankerWins = baccaratSimpleHistory.BankerWin;
        int playerWins = baccaratSimpleHistory.PlayerWin;
        int tieWins = baccaratSimpleHistory.Tie;
        
        lb_his_banker.text = bankerWins.ToString();
        lb_his_player.text = playerWins.ToString();
        lb_his_tie.text = tieWins.ToString();
    }
    
    public void OnClickShowHistory()
    {
        if (listSaveHistory.Count > 0)
        {
            if (popupHistory == null)
            {
                popupHistory = Instantiate(popupHistoryPrefab, transform);
                popupHistory.transform.SetAsLastSibling();
            }
            popupHistory.gameObject.SetActive(true);
            popupHistory.handleResultHisLayer1(listSaveHistory);
            popupHistory.handleResultHisLayer2(listSaveHistory);
            popupHistory.handleResultBigEyes(listSaveHistory);
            SetInfoResultHistory();
        }
    }
    
    private void SetInfoResultHistory()
    {
        int bankerWinCount = baccaratSimpleHistory.BankerWin;
        int playerWinCount = baccaratSimpleHistory.PlayerWin;
        int tieWinCount = baccaratSimpleHistory.Tie;
        int bankerPairCount = baccaratSimpleHistory.BankerPair;
        int playerPairCount = baccaratSimpleHistory.PlayerPair;
        
        popupHistory.numWinB = bankerWinCount;
        popupHistory.numWinP = playerWinCount;
        popupHistory.numWinT = tieWinCount;
        popupHistory.numWinBP = bankerPairCount;
        popupHistory.numWinPP = playerPairCount;

        popupHistory.lb_his_player_detail.text = popupHistory.numWinP.ToString();
        popupHistory.lb_his_banker_detail.text = popupHistory.numWinB.ToString();
        popupHistory.lb_his_tie_detail.text = popupHistory.numWinT.ToString();
        popupHistory.lb_his_playerPair.text = popupHistory.numWinPP.ToString();
        popupHistory.lb_his_bankerPair.text = popupHistory.numWinBP.ToString();
    }
    
    #endregion

    #region UI CountDown Preparing 
    
    private void SetLoopLbWaiting()
    {
        lb_waiting.text = "";
        lb_waiting.gameObject.SetActive(true);
        string text1 = lb_waiting.text + ".";
        string text2 = lb_waiting.text + "..";
        string text3 = lb_waiting.text + "...";

        waitingTextSequence = DOTween.Sequence()
            .AppendInterval(1.0f)
            .AppendCallback(() => lb_waiting.text = text1)
            .AppendInterval(1.0f)
            .AppendCallback(() => lb_waiting.text = text2)
            .AppendInterval(1.0f)
            .AppendCallback(() => lb_waiting.text = text3);
        waitingTextSequence.SetLoops(-1);
    }
    
    private void SetTextTime(int time)
    {
        lbTimeBet.SetText(time.ToString());
        if (time == 3) Config.Vibration();
    }
    
    #endregion

    #region UI Bet
    
    private BaccaratBetCell GetBetCellFromIndex(int index)
    {
        switch (index)
        {
            case 1: return BaccaratBetCell.BaccaratCellPlayer;
            case 2: return BaccaratBetCell.BaccaratCellBanker;
            case 3: return BaccaratBetCell.BaccaratCellTie;
            case 4: return BaccaratBetCell.BaccaratCellPlayerPair;
            case 5: return BaccaratBetCell.BaccaratCellBankerPair;
            default: return BaccaratBetCell.BaccaratCellTie;
        }
    }
    
    private void SetDisplayBet()
    {
        //long betValid = 0;
        bool check = false;
        for (int i = 4; i >= 0; i--)
        {
            if (listValueChipBets[i] > long.Parse(thisPlayer.wallet))
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
                if (GameState == GameState.Play && check && myChipBetColor >= i)
                {
                    listChipBets[i].transform.Find("Border").gameObject.SetActive(true);
                    betValue = listValueChipBets[i];
                    check = false;
                }
                //betValid = listValueChipBets[i];
            }
        }
    }
    
    private void SetStatusButtonsBet(bool btnrebet, bool btndouble)
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
                if (listMyBet[i] > 0 && listMyBet[i] <= MarkUnit * 100)
                {
                    checkBetDouble = true;
                    // SocketSend.sendBetBaccarat(listMyBet[i], getBetGate2(i + 1));
                    BaccaratPlayerBet baccaratPlayerBet = new BaccaratPlayerBet
                    {
                        ActionType = BaccaratBetActionType.BaccaratBetDouble
                    };
                    
                    DataSender.SendMatchState((long)OpCodeRequest.Bet, baccaratPlayerBet.ToByteArray());
                    return;
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
                    DataSender.SendMatchState((long)OpCodeRequest.Bet, baccaratPlayerBet.ToByteArray());
                    return;
                }
            }
        }
    }
    
    private void SetInfoBet(int m)
    {
        listValueChipBets = new List<long> { m, m * 5, m * 10, m * 50, m * 100 };
        for (int i = 0; i < 5; i++)
        {
            listChipBets[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = Utility.FormatMoney(listValueChipBets[i], true);
        }
        listChipBets[0].transform.Find("Border").gameObject.SetActive(true);
        betValue = listValueChipBets[0];
    }
    
    public void OnClickBet(int betArea)
    {
        if (betValue <= 0) return;
        if (GameState != GameState.Play) return;
        
        // Create bet request
        BaccaratPlayerBet baccaratPlayerBet = new BaccaratPlayerBet
        {
            ActionType = BaccaratBetActionType.BaccaratBetNormalUnspecified
        };
        
        BaccaratBet baccaratBet = new BaccaratBet
        {
            Chips = betValue,
            Cell = GetBetCellFromIndex(betArea)
        };
        
        baccaratPlayerBet.Bets.Add(baccaratBet);
        
        // Send bet to server
        DataSender.SendMatchState((long)OpCodeRequest.Bet, baccaratPlayerBet.ToByteArray());
        
        // Update UI
        checkBeted = true;
        UpdateStatePot();
        
        Debug.Log($"Placed bet: {betValue} on area {betArea}");
    }
    
    private void UpdateStatePot(){
        for (int i = 0; i < 5; i++)
        {
            listBoxBet[i].text = listMyBet[i] > 0 ? Utility.FormatMoney(listMyBet[i], true) : "";
            listBetContainer[i].text = listBet[i] > 0 ? Utility.FormatMoney(listBet[i], true) : "";
            boxBetBaccarat[i].gameObject.SetActive(listMyBet[i] > 0);
        }
    }

    #endregion

    #region UI Effect
    
    private void AnimateReturnCards()
    {
        foreach (CardModel card in listCardP)
        {
            int index = listCardP.IndexOf(card);
            Vector3 startPos = card.transform.localPosition;
            DOTween.Sequence()
                .AppendInterval(index * 0.3f)
                .AppendCallback(() =>
                {
                    card.HideCardPusoy();
                    card.gameObject.transform.DOLocalMove(new Vector2(-296, 301), 0.5f);
                    card.gameObject.transform.DOLocalRotate(new Vector3(0, 0, -64.48f), 0.5f);
                    card.gameObject.transform.DOScale(new Vector2(0.38f, 0.4f), 0.5f);
                })
                .AppendInterval(0.6f)
                .AppendCallback(() =>
                {
                    card.gameObject.SetActive(false);
                    card.transform.localPosition = startPos;
                    card.transform.localEulerAngles = new Vector3(0, 0, index != 2 ? 0 : -90);
                });
        }

        foreach (CardModel card in listCardB)
        {
            int index = listCardB.IndexOf(card);
            Vector3 startPos = card.transform.localPosition;
            DOTween.Sequence()
                .AppendInterval(index * 0.3f)
                .AppendCallback(() =>
                {
                    card.HideCardPusoy();
                    card.gameObject.transform.DOLocalMove(new Vector2(-296, 301), 0.5f);
                    card.gameObject.transform.DOLocalRotate(new Vector3(0, 0, -64.48f), 0.5f);
                    card.gameObject.transform.DOScale(new Vector2(0.38f, 0.4f), 0.5f);
                })
                .AppendInterval(0.6f)
                .AppendCallback(() =>
                {
                    card.gameObject.SetActive(false);
                    card.transform.localPosition = startPos;
                    card.transform.localEulerAngles = new Vector3(0, 0, index != 2 ? 0 : 90);
                });
        }
    }
    
    private void PayChipWin()
    {
        Vector2 posModel = new Vector2(0, 300);
        for (int i = 0; i < listChipInTable.Count; i++)
        {
            var chip = listChipInTable[i];
            if (listWinResult.Contains(chip.gateId))
            {
                // SoundManager.instance.playEffectFromPath(Globals.SOUND_GAME.THROW_CHIP);
                BaccaratChip chip1 = PoolService.Instance.Get<BaccaratChip>(PrefabType.ChipPlayerBaccarat);
                chip1.SetInfo(chip.idPl, chip.gateId, posModel, chip.chipValue, chip.chipSprite);
                chip1.transform.localScale = new Vector2(0.5f, 0.5f);
                Vector2 posChip = chip.transform.localPosition;
                posChip.x += 5f;
                posChip.y += 5f;
                chip1.transform.DOLocalMove(posChip, 0.5f);
                listChipsPay.Add(chip1);
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
                    BaccaratChip chip = listChipInTable[i];
                    if (listWinResult.Contains(chip.gateId))
                    {
                        // SoundManager.instance.playEffectFromPath(Globals.SOUND_GAME.WIN);
                        // get player bet win position - Sửa cast an toàn
                        if (userIdToView.TryGetValue(chip.idPl, out var playerView))
                        {
                            Vector2 posPlayer = playerView.transform.localPosition;
                            chip.transform.DOLocalMove(posPlayer, 0.5f).OnComplete(() =>
                            {
                                // Return chip to pool sau khi animation hoàn thành
                                PoolService.Instance.Release(PrefabType.ChipPlayerBaccarat, chip);
                                listChipInTable.Remove(chip);
                            });
                        }
                        else
                        {
                            // Nếu không tìm thấy player, return chip về pool
                            PoolService.Instance.Release(PrefabType.ChipPlayerBaccarat, chip);
                            listChipInTable.Remove(chip);
                        }
                    }
                }
            });
    }
    
    private void ChipMoveTo(BaccaratChip chip, int betGate)
    {
        Vector2 posPot = listPot[betGate].transform.localPosition;
        chip.transform
            .DOLocalMove(posPot, 0.3f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                Vector2 randomPosition = new Vector2(
                    posPot.x + UnityEngine.Random.Range(-30, 30),
                    posPot.y + UnityEngine.Random.Range(-8, 8)
                );

                chip.transform.DOLocalJump(randomPosition, 20, 1, 0.2f);
            });
    }
    
    private void ShowEffWinGate(int index)
    {
        int resultWin = index - 1;

        for (int i = 0; i < listPot.Count; i++)
        {
            if (i == resultWin)
            {
                Button objButton = listPot[i].GetComponent<Button>();
                Sprite spr = objButton.spriteState.pressedSprite;
                GameObject btnPress = objButton.transform.Find("press").gameObject;
                if (btnPress != null)
                {
                    Image btnImgPress = btnPress.GetComponent<Image>();
                    btnImgPress.sprite = spr;
                    btnPress.gameObject.SetActive(true);
                    btnImgPress.enabled = true;
                    Color normalColor = btnImgPress.color;
                    Color noOpacity = new Color(1, 1, 1, 0);
                    DOTween.Sequence()
                        .Append(btnImgPress.DOColor(noOpacity, 0.2f))
                        .Append(btnImgPress.DOColor(normalColor, 0.2f))
                        .SetLoops(5)
                        .OnComplete(() =>
                        {
                            btnPress.gameObject.SetActive(false);
                            btnImgPress.enabled = false;
                        });
                }
            }
        }
    }
    
    private void ShowEffWinType(BaccaratGameFinish data)
    {
        bool hasTie = false;
        bool hasPlayer = false;
        bool hasBanker = false;

        // Duyệt danh sách WinCells
        foreach (var cell in data.WinCells)
        {
            if (cell == BaccaratBetCell.BaccaratCellTie)
                hasTie = true;
            else if (cell == BaccaratBetCell.BaccaratCellPlayer)
                hasPlayer = true;
            else if (cell == BaccaratBetCell.BaccaratCellBanker)
                hasBanker = true;
        }

        // Ưu tiên TIE
        if (hasTie)
        {
            ani_win.gameObject.SetActive(true);
            ani_win.Initialize(true);
            ani_win.AnimationState.SetAnimation(0, "tie", false);
        }
        else if (hasPlayer)
        {
            ani_win.gameObject.SetActive(true);
            ani_win.Initialize(true);
            ani_win.AnimationState.SetAnimation(0, "player", false);
        }
        else if (hasBanker)
        {
            ani_win.gameObject.SetActive(true);
            ani_win.Initialize(true);
            ani_win.AnimationState.SetAnimation(0, "banker", false);
        }
        else
        {
            ani_win.gameObject.SetActive(false); // Không có kết quả chính
        }
    }
    
    #endregion
    
    private void ResetGame()
    {
        Array.Copy(listMyBet, listLastBet, 5);
        checkBeted = false;
        SetStatusButtonsBet(true, false);
        
        // Clear chips
        foreach (BaccaratChip chip in listChipInTable)
        {
            if (chip != null)
            {
                PoolService.Instance.Release(PrefabType.ChipPlayerBaccarat, chip);
            }
        }
        listChipInTable.Clear();
        
        listBet = listBet.Select(x => x * 0).ToArray();
        listMyBet = listMyBet.Select(x => x * 0).ToArray();

        foreach (var btnGate in listPot)
        {
            btnGate.GetComponent<Button>().interactable = false;
        }
        
        listWinResult.Clear();
        
        lbScoreBanker.text = "";
        lbScorePlayer.text = "";
        ani_win.gameObject.SetActive(false);

        indexCard = 0;
        scorePl = 0;
        scorePl1 = 0;
        scoreBk = 0;
        scoreBk1 = 0;

        scoreBanker.SetActive(false);
        scorePlayer.SetActive(false);
        scoreBanker.transform.localPosition = new Vector2(253, 174);
        scorePlayer.transform.localPosition = new Vector2(-253, 174);
        
        // Reset UI
        UpdateStatePot();
        AnimateReturnCards();
        Debug.Log("Game reset completed");
    }
    
}
