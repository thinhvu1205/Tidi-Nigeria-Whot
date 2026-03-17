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
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using GameState = Proto.GameState;
using Utility = Globals.Utility;
using Yuujins.Api.V1;

public class BaccaratView : BaseDiceGameView
{
    // ================== Prefabs & Containers ==================
    [Header("Prefabs & Containers")] [SerializeField]
    public BaccaratChip chipPref;

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
    [Header("Cards")] [SerializeField] public List<CardModel> listCardB = new List<CardModel>();
    [SerializeField] public List<CardModel> listCardP = new List<CardModel>();

    // ================== Buttons ==================
    [Header("Buttons")] [SerializeField] public Button btnDoubleBet;
    [SerializeField] public Button btnRebet;
    [SerializeField] public List<Button> listChipBets;

    // ================== Text Labels ==================
    [Header("Text Labels")] [SerializeField]
    public TextMeshProUGUI lbTimeBet;

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
    [Header("Animations")] [SerializeField]
    public SkeletonGraphic ani_win;

    // ================== Private Runtime Data ==================
    [Header("Runtime Data")] private BaccaratHistory popupHistory;
    private Yuujins.Api.V1.BaccaratSimpleHistory baccaratSimpleHistory;
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
    public override GameState[] AvailableLeaveStates => new GameState[]
    {
        GameState.Idle,
        GameState.Matching,
        GameState.Preparing,
    };

    private long betValue = 0;
    private int chipBetColorInx = 0;
    private int myChipBetColor = 0;

    private long[] listBet = { 0, 0, 0, 0, 0 };
    private long[] saveListBet = { 0, 0, 0, 0, 0 };
    private long[] saveListMyBet = { 0, 0, 0, 0, 0 };
    private long[] listMyBet = { 0, 0, 0, 0, 0 };
    private long[] listLastBet = { 0, 0, 0, 0, 0 };
    private readonly Vector3 dealCardPos = new Vector3(296, 301, 0);
    private Vector2[] posP = {
        new Vector2(-101, 171),
        new Vector2(-172, 171),
        new Vector2(-254, 171)
    };
    private Vector2[] posB = {
        new Vector2(101, 171),
        new Vector2(172, 171),
        new Vector2(254, 171)
    };

    private int scorePl = 0, scoreBk = 0, scorePl1 = 0, scoreBk1 = 0;
    private bool checkBeted = false, checkBetDouble = false;
    private int indexCard = 0;
    private Sequence waitingTextSequence;
    private const string WIN_ANIMATION_PATH = "Baccarat/Ani/skeleton_SkeletonData";
    private const string CHAT_ROOM_NAME = "baccarat";
    private bool isNewGame = true;
    private int minUnitTotalBet = 1,  maxUnitTotalBet = 100;
    private bool isEnableRebet = false;
    protected override void Awake()
    {
        base.Awake();
        LoadProfile();
        buttonBetBaccarat.SetActive(false);
        PoolService.Instance.Register(PrefabType.Card, cardContainer, listCardP[0], 8, 10, 6);
        PoolService.Instance.Register(PrefabType.ChipPlayerBaccarat, chipContainer.transform, chipPref, 20, 30, 15);
    }

    protected override void Update()
    {
        
    }

    protected override void OnDestroy()
    {
        // Clear all pools to ensure clean state
        base.OnDestroy();
        PoolService.Instance.ClearPool<BaccaratChip>(PrefabType.ChipPlayerBaccarat);
        PoolService.Instance.ClearPool<CardModel>(PrefabType.Card);
    }
    

    public override void LoadInfoMatch(Match match)
    {
        if (WantSwitchTable)
        {
            ResetDefaultUI();
        }
        base.LoadInfoMatch(match);
        SetInfoBet(MarkUnit);
         _ = NetworkManager.INSTANCE.JoinRoomChat(CHAT_ROOM_NAME + "-" + match.TableId);
    }

    private void LoadProfile()
    {
        currentPlayerView.id = User.UserAccount.Profile.UserId;
        currentPlayerView.wallet = User.UserAccount.Profile.Balance.ToString();
        currentPlayerView.avatar_id = User.UserAccount.Profile.AvatarId.ToString();
        currentPlayerView.vipLevel = User.UserAccount.Profile.Vip;
        currentPlayerView.user_name = User.UserAccount.Profile.DisplayName;
    }

    #region Hander Api

    protected override void RequestSyncStateTable()
    {
        base.RequestSyncStateTable();
        ResetDefaultUI();
        // Yuujins: no UserInTable/SyncTable; server pushes state on join
        DataSender.SendMatchState((long)OpCodeRequest.UserInTable, Array.Empty<byte>());
        DataSender.SendMatchState((long)OpCodeRequest.SyncTable, Array.Empty<byte>());
    }

    // Yuujins server: opcode 1 = GameState. Chỉ dùng logic mới, không gọi client cũ.
    public override void HandleUpdateGameState(IMatchState matchState)
    {
        try
        {
            var data = BaccaratGameStateUpdate.Parser.ParseFrom(matchState.State);
            var stateStr = data.State ?? "";
            GameState = stateStr switch
            {
                "play" => GameState.Play,
                "reward" => GameState.Reward,
                "idle" => GameState.Idle,
                "preparing" => GameState.Preparing,
                _ => GameState
            };
            if (lbTimeBet != null)
                lbTimeBet.text = Mathf.Max(0, (int)data.CountdownSec).ToString();
            if (stateStr == "play")
            {
                if (isNewGame) { isNewGame = false; lb_waiting.gameObject.SetActive(false); SoundManager.Instance.PlayEffectFromPath(Sound.START_GAME); }
                clock.gameObject.SetActive(true);
                buttonBetBaccarat.SetActive(true);
                foreach (var btn in listPot) btn.GetComponent<Button>().interactable = true;
            }
            else if (stateStr == "reward")
                foreach (var btn in listPot) btn.GetComponent<Button>().interactable = false;
        }
        catch (InvalidProtocolBufferException) { /* Yuujins payload only */ }
    }

    // Yuujins server: opcode 2 = Table. Chỉ dùng logic mới (BaccaratTableUpdate), không gọi client cũ.
    public override void HandleUpdateTable(IMatchState matchState)
    {
        try
        {
            var data = BaccaratTableUpdate.Parser.ParseFrom(matchState.State);
            foreach (var cellInfo in data.DeskCells)
            {
                int i = (int)cellInfo.Cell - 1;
                if (i >= 0 && i < 5) { boxBetBaccarat[i].SetActive(true); listBoxBet[i].text = Utility.FormatNumber(cellInfo.TotalChips); }
            }
            foreach (var userBet in data.UserBets)
            {
                if (string.IsNullOrEmpty(userBet.UserId) || !userIdToView.TryGetValue(userBet.UserId, out var playerView)) continue;
                if (userBet.UserId == User.UserAccount.Profile.UserId)
                {
                    foreach (var b in userBet.Bets) { int i = (int)b.Cell - 1; if (i >= 0 && i < 5) listMyBet[i] += b.Chips; }
                }
                foreach (var b in userBet.Bets)
                {
                    int i = (int)b.Cell - 1;
                    if (i < 0 || i >= 5) continue;
                    listBet[i] += b.Chips;
                    var chip = PoolService.Instance.Get<BaccaratChip>(PrefabType.ChipPlayerBaccarat);
                    chipBetColorInx = listValueChipBets.IndexOf(b.Chips);
                    chip.init(1, 0.4f);
                    chip.SetInfo(userBet.UserId, i + 1, playerView.transform.localPosition, b.Chips, chipBetColorInx);
                    chip.transform.localPosition = listPot[i].transform.localPosition;
                    chip.transform.localPosition += new Vector3(UnityEngine.Random.Range(-30, 30), UnityEngine.Random.Range(-8, 8), 0);
                    listChipInTable.Add(chip);
                }
            }
            if (listMyBet.Any(x => x > 0)) { checkBeted = true; buttonBetBaccarat.SetActive(true); }
            SetStatusButtonsBet(!checkBeted, checkBeted);
            SetDisplayBet();
            UpdateStatePot();
        }
        catch (InvalidProtocolBufferException) { /* Yuujins payload only */ }
    }

    private void HandleFinishFromProto(BaccaratFinishUpdate data)
    {
        var finish = new Proto.BaccaratGameFinish();
        if (data.Hand != null)
        {
            finish.Hand = new Proto.BaccaratHands { Player = new Proto.BaccaratHand(), Banker = new Proto.BaccaratHand() };
            if (data.Hand.Player != null)
            {
                foreach (var c in data.Hand.Player.Cards)
                    finish.Hand.Player.Cards.Add(new Proto.Card { Rank = (CardRank)c.Rank, Suit = (CardSuit)c.Suit });
                finish.Hand.Player.Point = data.Hand.Player.Point;
            }
            if (data.Hand.Banker != null)
            {
                foreach (var c in data.Hand.Banker.Cards)
                    finish.Hand.Banker.Cards.Add(new Proto.Card { Rank = (CardRank)c.Rank, Suit = (CardSuit)c.Suit });
                finish.Hand.Banker.Point = data.Hand.Banker.Point;
            }
        }
        foreach (var w in data.WinCells)
            finish.WinCells.Add((Proto.BaccaratBetCell)(int)w);
        foreach (var pr in data.ListBetResults)
        {
            var pres = new Proto.BaccaratPlayerBetResult { UserId = pr.UserId ?? "" };
            foreach (var l in pr.Lists)
                pres.Lists.Add(new Proto.BaccaratBetResult { Bet = new Proto.BaccaratBet { Chips = l.Bet.Chips, Cell = (Proto.BaccaratBetCell)(int)l.Bet.Cell }, IsWin = l.IsWin });
            finish.ListBetResults.Add(pres);
        }
        listWinResult.Clear();
        foreach (var w in finish.WinCells) listWinResult.Add((int)w);
        Dictionary<string, int> playerLoseAmounts = new Dictionary<string, int>();
        foreach (var chip in listChipInTable.Where(chip => !listWinResult.Contains(chip.gateId)))
        {
            playerLoseAmounts.TryAdd(chip.idPl, 0);
            playerLoseAmounts[chip.idPl] -= (int)chip.chipValue;
        }
        ShowEffWinType(finish);
        foreach (var index in listWinResult) ShowEffWinGate(index);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(3f).AppendCallback(() =>
        {
            if (ani_win != null) ani_win.gameObject.SetActive(false);
            Vector3 collectionPos = new Vector3(0, 300, 0);
            foreach (BaccaratChip chip in listChipInTable.ToList())
            {
                if (!listWinResult.Contains(chip.gateId))
                {
                    SoundManager.Instance.PlayEffectFromPath(Sound.GET_CHIP);
                    chip.transform.DOLocalMove(collectionPos, 0.5f).SetEase(Ease.InSine);
                    chip.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        PoolService.Instance.Release(PrefabType.ChipPlayerBaccarat, chip);
                        listChipInTable.Remove(chip);
                    });
                }
            }
            foreach (var kvp in playerLoseAmounts)
                if (userIdToView.TryGetValue(kvp.Key, out var playerObj) && kvp.Value < 0)
                    playerObj.AnimateFlyMoney(kvp.Value, 40);
        });
        sequence.AppendInterval(1f).AppendCallback(PayChipWin);
        sequence.AppendInterval(1f).AppendCallback(AnimateReturnCards);
    }

    public void HandleBaccaratReject(IMatchState matchState)
    {
        if (matchState.State == null || matchState.State.Length == 0) return;
        try
        {
            var data = BaccaratReject.Parser.ParseFrom(matchState.State);
            switch (data.Reason)
            {
                case BaccaratRejectReason.BaccaratRejectBalanceNotEnough:
                    UIManager.Instance.ShowAlertDialog("Not enough chip !");
                    break;
                default:
                    UIManager.Instance.ShowAlertDialog(data.Reason.ToString().Replace("_", " "));
                    break;
            }
        }
        catch (InvalidProtocolBufferException) { UIManager.Instance.ShowAlertDialog("Bet rejected"); }
    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        // Debug.Log("HandleUpdateUserInTable " + updateTable);
        UpdatePosUserTable(updateTable);
    }

    // Yuujins server: opcode 3 = Deal. Chỉ dùng logic mới (BaccaratDealUpdate), không gọi client cũ.
    public override void HandleUpdateDeal(IMatchState matchState)
    {
        try
        {
            var data = BaccaratDealUpdate.Parser.ParseFrom(matchState.State);
            if (data.Hands != null && (data.Hands.Player?.Cards.Count > 0 || data.Hands.Banker?.Cards.Count > 0))
            {
                clock.SetActive(false);
                buttonBetBaccarat.SetActive(false);
                foreach (var btn in listPot) btn.GetComponent<Button>().interactable = false;
                SetStatusButtonsBet(!checkBeted, checkBeted);
                var ph = data.Hands.Player;
                var bh = data.Hands.Banker;
                if (ph != null)
                    for (int i = 0; i < ph.Cards.Count && i < listCardP.Count; i++)
                    {
                        var c = ph.Cards[i];
                        listCardP[i].SetData(c.Rank, c.Suit);
                        listCardP[i].ShowCard();
                        listCardP[i].gameObject.SetActive(true);
                    }
                if (bh != null)
                    for (int i = 0; i < bh.Cards.Count && i < listCardB.Count; i++)
                    {
                        var c = bh.Cards[i];
                        listCardB[i].SetData(c.Rank, c.Suit);
                        listCardB[i].ShowCard();
                        listCardB[i].gameObject.SetActive(true);
                    }
                if (ph != null) { lbScorePlayer.text = ph.Point.ToString(); scorePlayer.SetActive(ph.Cards.Count > 0); }
                if (bh != null) { lbScoreBanker.text = bh.Point.ToString(); scoreBanker.SetActive(bh.Cards.Count > 0); }
                return;
            }
            if (data.Card != null)
            {
                int idx = data.IsPlayer ? listCardP.Count(c => c.gameObject.activeSelf) : listCardB.Count(c => c.gameObject.activeSelf);
                if (idx == 0 && data.IsPlayer) { clock.SetActive(false); buttonBetBaccarat.SetActive(false); foreach (var btn in listPot) btn.GetComponent<Button>().interactable = false; SetStatusButtonsBet(!checkBeted, checkBeted); }
                SoundManager.Instance.PlayEffectFromPath(Sound.DISPATCH_CARD);
                var cardModel = PoolService.Instance.Get<CardModel>(PrefabType.Card);
                cardModel.HideCard();
                cardModel.transform.localPosition = dealCardPos;
                cardModel.transform.localScale = new Vector2(0.38f, 0.4f);
                cardModel.SetData(data.Card.Rank, data.Card.Suit);
                cardModel.gameObject.SetActive(true);
                if (data.IsPlayer && idx < listCardP.Count) { listCardP[idx].SetData(data.Card.Rank, data.Card.Suit); listCardP[idx].gameObject.SetActive(true); }
                else if (!data.IsPlayer && idx < listCardB.Count) { listCardB[idx].SetData(data.Card.Rank, data.Card.Suit); listCardB[idx].gameObject.SetActive(true); }
                cardModel.gameObject.SetActive(false);
            }
            return;
        }
        catch (InvalidProtocolBufferException) { /* Yuujins payload only */ }
    }

    // public override void HandleUpdateGameState(IMatchState matchState)
    // {
    //     base.HandleUpdateGameState(matchState);
    //     var updateGameState = UpdateGameState.Parser.ParseFrom(matchState.State);
    //     GameState = updateGameState.State;
    //     if (GameState != GameState.Play)
    //     {
    //         buttonBetBaccarat.SetActive(false);
    //     }
    //     switch (updateGameState.State)
    //     {
    //         case GameState.Idle:
    //             // Debug.Log("HandleUpdateGameState Idle "+updateGameState.ToString());
    //             break;
    //         case GameState.Matching:
    //             // Debug.Log("HandleUpdateGameState Matching "+ updateGameState.ToString());
    //             break;
    //         case GameState.Preparing:
    //             isNewGame = true;
    //             SetLoopLbWaiting();
    //             // Debug.Log("HandleUpdateGameState Preparing " + updateGameState.ToString());
    //             break;
    //         case GameState.Play:
    //             // Debug.Log("HandleUpdateGameState Play " + updateGameState.ToString());
    //             if (isNewGame)
    //             {
    //                 isNewGame = false;
    //                 if (waitingTextSequence != null && waitingTextSequence.IsActive())
    //                 {
    //                     waitingTextSequence.Kill();
    //                     waitingTextSequence = null;
    //                 }
    //                 SoundManager.Instance.PlayEffectFromPath(Sound.START_GAME);
    //                 lb_waiting.gameObject.SetActive(false);
    //                 clock.gameObject.SetActive(true);
    //                 buttonBetBaccarat.SetActive(true);
    //                 foreach (var btn in listPot)
    //                 {
    //                     btn.GetComponent<Button>().interactable = true;
    //                 }
    //                 if (popupHistory != null)
    //                 {
    //                     popupHistory.gameObject.SetActive(false);
    //                 }
    //
    //                 SetDisplayBet();
    //                 var sumLastBet = listLastBet.Sum();
    //                 isEnableRebet =  sumLastBet <= long.Parse(currentPlayerView.wallet) && sumLastBet > 0;
    //                 SetStatusButtonsBet(isEnableRebet, checkBeted);
    //             
    //                 foreach (var player in players)
    //                 {
    //                     if (userIdToView.TryGetValue(player.Id, out var playerView))
    //                     {
    //                         playerView.SetCurrentTurn(true, updateGameState.CountDown, 10);
    //                     }
    //                 }
    //             }
    //             SetTextTime((int) updateGameState.CountDown);
    //             break;
    //         case GameState.Reward:
    //             // Debug.Log("HandleUpdateGameState Reward " + updateGameState.ToString());
    //             foreach (var btnGate in listPot)
    //             {
    //                 btnGate.GetComponent<Button>().interactable = false;
    //             }
    //             break;
    //         case GameState.Finish:
    //             // Debug.Log("HandleUpdateGameState Finish " + updateGameState.ToString());
    //             break;
    //         
    //     }
    // }

    // Add balance storage
    private Dictionary<string, BalanceUpdate> balanceUpdates = new Dictionary<string, BalanceUpdate>();

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var balanceResult = BalanceResult.Parser.ParseFrom(matchState.State);
        // Debug.Log("BaccaratUpdateWallet " + balanceResult);
        
        // Store balance updates for later use
        balanceUpdates.Clear();
        foreach (var update in balanceResult.Updates)
        {
            balanceUpdates[update.UserId] = update;
        }
    }

    // Yuujins server: opcode 4 = Finish. Chỉ dùng logic mới (BaccaratFinishUpdate → HandleFinishFromProto), không gọi client cũ.
    public override void HandleFinish(IMatchState matchState)
    {
        try
        {
            var data = BaccaratFinishUpdate.Parser.ParseFrom(matchState.State);
            HandleFinishFromProto(data);
        }
        catch (InvalidProtocolBufferException) { /* Yuujins payload only */ }
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
        if (time > 0)
        {
            SoundManager.Instance.PlayEffectFromPath(Sound.CLOCK_TICK);
        }
        lbTimeBet.SetText(time.ToString());
        if (time == 3) Config.Vibration();
    }
    
    #endregion

    #region UI Bet
    
    private Yuujins.Api.V1.BaccaratBetCell GetBetCellFromIndex(int index)
    {
        switch (index)
        {
            case 1: return Yuujins.Api.V1.BaccaratBetCell.Player;
            case 2: return Yuujins.Api.V1.BaccaratBetCell.Banker;
            case 3: return Yuujins.Api.V1.BaccaratBetCell.Tie;
            case 4: return Yuujins.Api.V1.BaccaratBetCell.PlayerPair;
            case 5: return Yuujins.Api.V1.BaccaratBetCell.BankerPair;
            default: return Yuujins.Api.V1.BaccaratBetCell.Tie;
        }
    }
    
    private void SetDisplayBet()
    {
        //long betValid = 0;
        long totalBet = listMyBet.Sum();
        bool check = false;
        for (int i = 4; i >= 0; i--)
        {
            if (listValueChipBets[i] > long.Parse(currentPlayerView.wallet) || totalBet + listValueChipBets[i] > maxUnitTotalBet * MarkUnit )
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

        if (check)
        {
            betValue = 0;
        }
    }
    
    private void SetStatusButtonsBet(bool btnrebet, bool btndouble)
    {
        if (listLastBet.All(element => element == 0))
        {
            btnrebet = false;
        }
        btnRebet.interactable = btnrebet;
        btnDoubleBet.interactable = btndouble;
    }
    
    public void OnClickChipBet(int chipBet)
    {
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);

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
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        int timeleft = int.Parse(lbTimeBet.text);
        if (timeleft > 1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (listMyBet[i] > 0 && listMyBet[i] <= MarkUnit * 100)
                {
                    checkBetDouble = true;
                    // SocketSend.sendBetBaccarat(listMyBet[i], getBetGate2(i + 1));
                    DataSender.SendMatchState((long)BaccaratOpCodeRequest.BaccaratRequestDoubleBet, Array.Empty<byte>());
                    return;
                }
            }
        }
    }

    public void OnClickRebet()
    {
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);

        int timeleft = int.Parse(lbTimeBet.text);
        if (timeleft > 1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (listLastBet[i] > 0)
                {
                    checkBeted = true;
                    // SocketSend.sendBetBaccarat(listLastBet[i], getBetGate2(i + 1));
                    DataSender.SendMatchState((long)BaccaratOpCodeRequest.BaccaratRequestRebet, Array.Empty<byte>());
                    return;
                }
            }
        }
    }
    
    private void SetInfoBet(int m)
    {
        listValueChipBets = new List<long> { m, m * 5, m * 10, m * 20, m * 50 };
        for (int i = 0; i < 5; i++)
        {
            listChipBets[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = Utility.FormatMoney(listValueChipBets[i], true);
        }
        listChipBets[0].transform.Find("Border").gameObject.SetActive(true);
        betValue = listValueChipBets[0];
    }
    
    public void OnClickBet(int betArea)
    {
        if (betValue <= 0)
        {
            UIManager.Instance.ShowAlertDialog("You have reached the betting limit for this round !");
            return;
        }
        if (GameState != GameState.Play) return;
        
        // Create bet request
        Yuujins.Api.V1.BaccaratPlayerBet baccaratPlayerBet = new Yuujins.Api.V1.BaccaratPlayerBet
        {
            // ActionType = BaccaratBetActionType.BaccaratBetNormalUnspecified
        };
        
        var req = new BaccaratBetRequest();
        req.Bets.Add(new Yuujins.Api.V1.BaccaratBet { Chips = betValue, Cell = GetBetCellFromIndex(betArea) });
        DataSender.SendMatchState((long)BaccaratOpCodeRequest.BaccaratRequestBet, req.ToByteString().ToByteArray());
        
        // Update UI
        checkBeted = true;
        UpdateStatePot();
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
                    card.HideCard();
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
                    card.HideCard();
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
                SoundManager.Instance.PlayEffectFromPath(Sound.THROW_CHIP);
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
                        SoundManager.Instance.PlayEffectFromPath(Sound.WIN);
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
            if (cell == Proto.BaccaratBetCell.BaccaratCellTie)
                hasTie = true;
            else if (cell == Proto.BaccaratBetCell.BaccaratCellPlayer)
                hasPlayer = true;
            else if (cell == Proto.BaccaratBetCell.BaccaratCellBanker)
                hasBanker = true;
        }

        // Ưu tiên TIE
        if (hasTie)
        {
            ani_win.gameObject.SetActive(true);
            ani_win.Initialize(true);
            Utility.PlayAnimationByPath(ani_win, WIN_ANIMATION_PATH, "tie", false);
        }
        else if (hasPlayer)
        {
            ani_win.gameObject.SetActive(true);
            ani_win.Initialize(true);
            Utility.PlayAnimationByPath(ani_win, WIN_ANIMATION_PATH, "player", false);
        }
        else if (hasBanker)
        {
            ani_win.gameObject.SetActive(true);
            ani_win.Initialize(true);
            Utility.PlayAnimationByPath(ani_win, WIN_ANIMATION_PATH, "banker", false);
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
    }

    private void ResetDefaultUI()
    {
        //reset time for preparing, play
        isNewGame = true;
        lbTimeBet.text = "";
        lb_waiting.gameObject.SetActive(false);
        
        foreach (var btnGate in listPot)
        {
            btnGate.GetComponent<Button>().interactable = false;
        }
        
        indexCard = 0;
        scorePl = 0;
        scorePl1 = 0;
        scoreBk = 0;
        scoreBk1 = 0;
        
        // reset score
        lbScoreBanker.text = "";
        lbScorePlayer.text = "";
        ani_win.gameObject.SetActive(false);
        scoreBanker.SetActive(false);
        scorePlayer.SetActive(false);
        scoreBanker.transform.localPosition = new Vector2(253, 174);
        scorePlayer.transform.localPosition = new Vector2(-253, 174);
        
        // reset count down player
        foreach (var player in players)
        {
            if (userIdToView.TryGetValue(player.Id, out var playerView))
            {
                playerView.HideCountDown();
            }
        }
        
        // Clear chips
        foreach (BaccaratChip chip in listChipInTable)
        {
            if (chip != null)
            {
                PoolService.Instance.Release(PrefabType.ChipPlayerBaccarat, chip);
            }
        }
        listChipInTable.Clear();
        
        // reset label chip bet player
        listBet = listBet.Select(x => x * 0).ToArray();
        listMyBet = listMyBet.Select(x => x * 0).ToArray();
        UpdateStatePot();
        
        // reset card 
        for (int i = 0; i < listCardP.Count; i++)
        {
            var card = listCardP[i];
            card.gameObject.SetActive(false);
            card.transform.localPosition = posP[i];
            card.transform.localEulerAngles = new Vector3(0, 0, i == 2 ? -90 : 0);
        }

        for (int i = 0; i < listCardB.Count; i++)
        {
            var card = listCardB[i];
            card.gameObject.SetActive(false);
            card.transform.localPosition = posB[i];
            card.transform.localEulerAngles = new Vector3(0, 0, i == 2 ? 90 : 0);
        }
        
    }
    
}
