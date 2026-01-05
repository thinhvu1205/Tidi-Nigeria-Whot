using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Pool;
using DG.Tweening;
using Games.Card;
using Globals;
using Google.Protobuf;
using Nakama;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameState = Proto.GameState;
using Color = UnityEngine.Color;
using Newtonsoft.Json.Linq;

public class BlackjackView : BaseDiceGameView
{
    [Header(" Transforms ")]
    [SerializeField] private Transform boxBetContainer;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform chipContainer;
    [SerializeField] private Transform bankerCardsContainer;
    [SerializeField] private Transform buttonBetContainer;
    [SerializeField] private Transform dealCardContainer;
    [SerializeField] private Transform returnCardContainer;
    [SerializeField] private Transform effectContainer;

    [Header(" Game Objects ")]
    [SerializeField] private GameObject countdownContainer;
    [SerializeField] private Button buttonRebet;
    [SerializeField] private Button buttonDoubleBet;
    [SerializeField] private GameObject buttonDeal;
    [SerializeField] private GameObject buttonClear;
    [SerializeField] private GameObject rulePrefab;
    [SerializeField] private GameObject phaseBet;
    [SerializeField] private GameObject phasePlay;
    [SerializeField] private GameObject imageLight;

    [Header(" Buttons ")]
    [SerializeField] private BlackjackButtonAction buttonDouble;
    [SerializeField] private BlackjackButtonAction buttonSplit;
    [SerializeField] private BlackjackButtonAction buttonHit;
    [SerializeField] private BlackjackButtonAction buttonStand;

    [Header(" Texts ")]
    [SerializeField] private TextMeshProUGUI textCountdown;
    [SerializeField] private TextMeshProUGUI textDealValue;
    [SerializeField] private TextMeshProUGUI textClearValue;
    [SerializeField] private TextMeshProUGUI textMinBet;
    [SerializeField] private TextMeshProUGUI textMaxBet;
    [SerializeField] private TextMeshProUGUI textID;

    [Header(" Images ")]
    [SerializeField] private Image imageCountdown;
    [SerializeField] private Image imageButtonRebet;
    [SerializeField] private Image imageButtonDoubleBet;

    [Header(" Prefabs ")]
    [SerializeField] private CardModel cardPrefab;
    [SerializeField] private BlackjackChip chipPrefab;
    [SerializeField] private BlackjackBoxBet boxBetPrefab;
    [SerializeField] private BlackjackInsurance insurance;
    [SerializeField] private BlackjackBoxBet bankerBoxBet;

    [Header(" Animations ")]
    [SerializeField] private SkeletonGraphic animationWinBlackjack;

    [Header(" Object Pool")]
    private UnityEngine.Pool.ObjectPool<CardModel> cardPool;
    private UnityEngine.Pool.ObjectPool<BlackjackChip> chipPool;

    [Header(" Lists ")]
    [SerializeField] private Vector2[] listBoxBetPosition;
    [SerializeField] private BlackjackChipBet[] listChipBet;
    private readonly Vector2[] listInsuranceChipPosition = new Vector2[]
    {
        new(0, 30),
        new(-248, 181),
        new(248, 181),
    };
    private readonly float[] listLightAngle = new float[]
    {
        0f,
        -70f,
        60f
    };
    private Coroutine countdownCoroutine;
    private BlackjackBoxBet currentPlayerBoxBet;


    [Header(" Constants ")]
    public override GameState[] AvailableLeaveStates => new GameState[]
    {
        GameState.Idle,
        GameState.Matching,
        GameState.Preparing,
        GameState.Reward
    };
    private const string CHAT_ROOM_NAME = "blackjack";
    private readonly Dictionary<string, BlackjackBoxBet> userIdToBoxBetView = new();
    private readonly Dictionary<string, BlackjackChip> userIdToInsuranceChip = new();
    private const float DEAL_CARD_ANIMATION_TIME = 0.4f;

    private List<long> listValueChipBets = new();
    private List<Card> listBankerCard = new();
    [SerializeField] private int currentChipIndex = 0, lastChipIndex = -1, playerReceiveCardCount = 0;
    [SerializeField] private BlackjackActionCode nextActionCode = BlackjackActionCode.BlackjackActionUnspecified; // 1: Split, 2: Double, 3: Hit, 4: Stand
    [SerializeField] private long totalBetValue = 0, currentBetValue = 0, lastBetValue = 0;
    private BlackjackHandN0 blackjackHandN0 = BlackjackHandN0.BlackjackHand1St;
    private long playerWallet = 0;
    private int currentPlayerIndex = 0;
    private bool hasDealtCardsForPlayers = false; // Check xem đã chia bài cho các player chưa
    private bool hasDealtCardsForBanker = false; // Check xem đã chia bài cho banker chưa
    private bool isCurrentPlayerTurn = false; // Check xem có phải lượt của người chơi hiện tại không
    private bool isRebet = false; // Check xem có phải người chơi hiện tại đang rebet không
    private bool isPlaying = true; // Check xem có phải người chơi hiện tại đang chơi lượt này hay không
    private bool isSplitingHand = false; // Check xem có đang tách bài hay ko
    private bool isClickDoubleBet = false; // Check xem có đang tách bài hay ko
    private bool isCountingDown = false;
    private bool isRejoinTable = false;
    private string lastPlayerTurnId, currentPlayerTurnId;
    [SerializeField] private bool isCurrentPlayerFinished = false; // Check xem người chơi hiện tại đã xong ván chưa (được Blackjack/Busted)
    [SerializeField] private bool isCurrentPlayerTurnPassed = false; // Check xem đã qua turn người chơi hiện tại chưa
    private Vector2 countdownPosition;
    private List<BalanceUpdate> listBalanceUpdate;

    protected override void Awake()
    {
        base.Awake();
        InitPool();
        insurance.SetInfo(this);
        phaseBet.SetActive(false);
        phasePlay.SetActive(false);
        countdownPosition = countdownContainer.transform.position;
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PoolService.Instance.ClearPool<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
        PoolService.Instance.ClearPool<CardModel>(PrefabType.Card);
    }

    #region API Handlers
    protected override void RequestSyncStateTable()
    {
        Debug.Log("RequestRejoinTable");
        ResetGame(true);
        UpdateChipBetInteractivity();
        isRejoinTable = true;
        DataSender.SendMatchState((long)OpCodeRequest.UserInTable, new byte[0]);
        DataSender.SendMatchState((long)OpCodeRequest.SyncTable, new byte[0]);
    }

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        SetInfoBet();
        textID.text = "ID: " + match.TableId;
        textMaxBet.text = "Max bet " + Utility.FormatNumber(MarkUnit * 100);
        textMinBet.text = "Min bet " + Utility.FormatNumber(MarkUnit);
        // RequestRejoinTable();
        _ = NetworkManager.INSTANCE.JoinRoomChat(CHAT_ROOM_NAME + "-" + match.TableId);

        // string labelJson = match.Label;
        // Match data = JsonConvert.DeserializeObject<Match>(labelJson);

    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateUserInTable " + updateTable);
        UpdatePosUserTable(updateTable, true);
        Player currentPlayer = players.Find((player) => player.Id == User.userProfile.UserId);
        isPlaying = playingPlayers.Exists((player) => player.Id == User.userProfile.UserId);
        playerWallet = long.Parse(currentPlayer.Wallet);
        UpdateChipBetInteractivity();
        currentPlayerIndex = playingPlayers.FindIndex(p => p.Id == User.userProfile.UserId);
        // if (rearrangedPlayers.Count >= 2)
        // {
        //     Player secondPlayer = rearrangedPlayers[2];
        //     BasePlayerView playerView = userIdToView.GetValueOrDefault(secondPlayer.Id);
        //     playerView.SetPositionInfoRightPlayer();

        // }

    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        BlackjackUpdateDesk data = BlackjackUpdateDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("Update table: " + data.ToString());
        string playerId = data.Bet?.UserId;
        string currentPlayerId = User.userProfile.UserId;
        // Khi có người chơi đặt cược thì tiến hành trừ tiền của người chơi đó
        if (data.Bet?.Balance?.AmountChipAdd != 0)
        {
            if (playerId != null && userIdToView.TryGetValue(playerId, out var playerView))
            {
                SoundManager.Instance.PlayEffectFromPath(SoundBlackjack.BET);
                playerView.AnimateFlyMoney(data.Bet.Balance.AmountChipAdd);
                playerView.SetCurrentChip(data.Bet.Balance.AmountChipCurrent);
                hasBet = true;

                // Rebet
                if (isRebet && data.Bet?.Balance.AmoutChipBet > 0 && data.Bet.UserId == User.userProfile.UserId)
                {
                    hasBet = true;
                    totalBetValue = data.Bet.Balance.AmoutChipBet;
                    OnClickButtonChip(lastChipIndex);
                }

                // Player Khác đặt cược
                if (playerId != currentPlayerId && data.Bet.Insurance == 0)
                {
                    BlackjackBoxBet boxbet = userIdToBoxBetView.GetValueOrDefault(playerId);
                    boxbet.HasBet = true;
                    boxbet.HideAnimationWaiting();

                    BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                    int chipIndex = GetChipIndex((int)data.Bet.Balance.AmoutChipBet);
                    chip.transform.SetParent(playerView.GetAvatarTransform().parent, true);
                    chip.SetInfo(chipIndex, playerView.GetAvatarPosition());
                    chip.transform.localScale = Vector2.zero * 0.3f;
                    AnimateMoveChipForward(chip, boxbet.transform.position, () =>
                    {
                        long totalBet = data.Bet.Balance.AmoutChipBet + boxbet.TotalBet;
                        boxbet.SetBetValue(GetChipIndex(totalBet), totalBet, totalBet);
                    });
                }
                else
                {
                    currentPlayerBoxBet.HasBet = true;
                    playerWallet = data.Bet.Balance.AmountChipCurrent;
                    UpdateChipBetInteractivity();
                }
            }

            if (isClickDoubleBet && GameState != GameState.Play)
            {
                isClickDoubleBet = false;
                currentPlayerBoxBet.DoubleBoxBet();
                currentPlayerBoxBet.ShowHigherScore();
                totalBetValue *= 2;
                currentPlayerBoxBet.SetBetValue(GetChipIndex(totalBetValue), totalBetValue, totalBetValue);
                SetEnableDoubleBetButton(true);
            }
        }

        if (data.IsNewTurn || (isRejoinTable && GameState == GameState.Play))
        {
            int playerIndex = playingPlayers.FindIndex(p => p.Id == data.InTurn);
            isRejoinTable = false;
            BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(data.InTurn);
            lastPlayerTurnId = currentPlayerTurnId;
            currentPlayerTurnId = data.InTurn;
            if (!string.IsNullOrEmpty(data.InTurn))
            {
                RotateImageLight(data.InTurn);
            }
            
            if (!string.IsNullOrEmpty(lastPlayerTurnId) && !string.IsNullOrEmpty(currentPlayerTurnId) && lastPlayerTurnId != currentPlayerTurnId)
            {
                if (userIdToBoxBetView.TryGetValue(lastPlayerTurnId, out var lastTurnBoxBet))
                {
                    lastTurnBoxBet.ShowHigherScore();
                    lastTurnBoxBet.SecondBoxBet.ShowHigherScore();
                }
            }
            // Debug.Log("IsNewTurn - isCurrentPlayerFinished: " + isCurrentPlayerFinished);
            isCurrentPlayerTurn = data.InTurn == User.userProfile.UserId;

            if (!isCurrentPlayerTurnPassed)
            {
                if (isCurrentPlayerTurn)
                    isCurrentPlayerTurnPassed = true;
            }
            isCurrentPlayerFinished = isCurrentPlayerTurnPassed && data.InTurn != currentPlayerId;
            blackjackHandN0 = data.HandN0;
            // Nếu đã chọn action khi đang ở lượt người chơi khác thì khi đến lượt sẽ thực hiện action đó ngay
            if (isCurrentPlayerTurn && nextActionCode != BlackjackActionCode.BlackjackActionUnspecified)
            {
                HandleClickButtonAction(nextActionCode);
                nextActionCode = BlackjackActionCode.BlackjackActionUnspecified;
            }

            if (!isCurrentPlayerTurn)
            {
                ShrinkCurrentPlayerBoxbet();
            }


            // Set Turn hiện tại
            foreach (var player in userIdToView)
            {
                BasePlayerView playerView = player.Value;
                playerView.HideCountDown();
            }
            if (userIdToView.TryGetValue(data.InTurn, out var view))
            {
                view.SetCurrentTurn(true, 10, 10);
            }

            // Cập nhật các nút hành động
            Debug.Log("isPlaying: " + isPlaying);
            Debug.Log("isCurrentPlayerFinished: " + isCurrentPlayerFinished);
            Debug.Log("playerIndex: " + (playerIndex));
            Debug.Log("currentPlayerIndex: " + (currentPlayerIndex));
            Debug.Log("GameState: " + GameState);
            Debug.Log("data.IsInsuranceTurnEnter: " + data.IsInsuranceTurnEnter);
            if (!data.IsInsuranceTurnEnter && isPlaying && !isCurrentPlayerFinished && playerIndex <= currentPlayerIndex && GameState == GameState.Play)
            {
                buttonBetContainer.gameObject.SetActive(true);
                buttonDouble.gameObject.SetActive(true);
                buttonSplit.gameObject.SetActive(true);
                buttonHit.gameObject.SetActive(true);
                buttonStand.gameObject.SetActive(true);

                // Turn của người chơi hiện tại
                if (data.Actions != null)
                {
                    buttonBetContainer.gameObject.SetActive(true);
                    List<BlackjackActionCode> actions = data.Actions.Actions.ToList();
                    buttonDouble.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionDouble);
                    buttonSplit.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionSplit);
                    buttonHit.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionHit);
                    buttonStand.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionStay);
                }

                // Turn của người chơi khác
                if (data.InTurn != User.userProfile.UserId)
                {
                    buttonDouble.button.interactable = true;
                    buttonSplit.button.interactable = currentPlayerBoxBet.IsSplittableBox();
                    buttonHit.button.interactable = true;
                    buttonStand.button.interactable = true;
                }
                UpdateButtonActionVisual(isCurrentPlayerTurn);
            }
            else
            {
                Debug.Log("YOOOO");
                buttonBetContainer.gameObject.SetActive(false);
            }


            if (isCurrentPlayerTurn && isSplitingHand)
            {
                if (data.HandN0 == BlackjackHandN0.BlackjackHand1St)
                {
                    boxBet.EnlargeCards();
                    boxBet.SecondBoxBet.ShrinkCards();
                }
                else
                {
                    boxBet.ShrinkCards();
                    boxBet.SecondBoxBet.EnlargeCards();
                }
            }
        }
        // else
        // {
        //     imageLight.gameObject.SetActive(false);
        // }

        // Hiện hành động của người chơi
        if (data.PlayerAction != null)
        {
            Debug.Log("PlayerAction: " + data.PlayerAction.ToString());
            BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(data.PlayerAction.UserId);
            foreach (var player in userIdToView)
            {
                BasePlayerView playerView = player.Value;
                playerView.HideCountDown();
            }
            if (boxBet != null)
            {
                if (data.HandN0 == BlackjackHandN0.BlackjackHand1St)
                {
                    boxBet.AnimateImageAction(data.PlayerAction.Code);
                }
                else if (data.HandN0 == BlackjackHandN0.BlackjackHand2Nd)
                {
                    boxBet.SecondBoxBet.AnimateImageAction(data.PlayerAction.Code);
                }
            }

            if (data.PlayerAction.Code == BlackjackActionCode.BlackjackActionDouble && data.PlayerAction.UserId == User.userProfile.UserId)
            {
                if (blackjackHandN0 == BlackjackHandN0.BlackjackHand1St)
                {
                    boxBet.DoubleBoxBet();
                    boxBet.HasEnded = true;
                    boxBet.SetBetValue(GetChipIndex(boxBet.TotalBet), boxBet.TotalBet, boxBet.TotalBet);
                }
                else if (blackjackHandN0 == BlackjackHandN0.BlackjackHand2Nd)
                {
                    boxBet.SecondBoxBet.DoubleBoxBet();
                    boxBet.SecondBoxBet.HasEnded = true;
                    boxBet.SecondBoxBet.SetBetValue(GetChipIndex(boxBet.SecondBoxBet.TotalBet), boxBet.SecondBoxBet.TotalBet, boxBet.SecondBoxBet.TotalBet);
                }
                ShrinkCurrentPlayerBoxbet();
            }
        }

        // if (data.IsUpdateLegalAction)
        // {
        //     Debug.Log("IsUpdateLegalAction?");
        //     buttonBetContainer.gameObject.SetActive(true);
        //     buttonDouble.gameObject.SetActive(true);
        //     buttonSplit.gameObject.SetActive(true);
        //     buttonHit.gameObject.SetActive(true);
        //     buttonStand.gameObject.SetActive(true);

        //     // Turn của người chơi hiện tại
        //     if (data.Actions != null)
        //     {
        //         List<BlackjackActionCode> actions = data.Actions.Actions.ToList();
        //         buttonDouble.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionDouble);
        //         buttonSplit.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionSplit);
        //         buttonHit.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionHit);
        //         buttonStand.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionStay);
        //     }
        // }
        // else
        // {
        //     buttonBetContainer.gameObject.SetActive(false);
        // }


        // Nhà cái có 1 lá Át, hiện popup bảo hiểm
        bool isCurrentPlayerPayInsurance = data.PlayersBet.Count > 0 && data.PlayersBet.FirstOrDefault((bet) => bet.UserId == User.userProfile.UserId)?.Insurance > 0;
        if (data.IsInsuranceTurnEnter && isPlaying && playerWallet >= totalBetValue / 2 && !isCurrentPlayerPayInsurance)
        {
            insurance.Show();
        }

        if (data.IsBankerNotBlackjack)
        {
            bankerBoxBet.StopHighlightCards();
            foreach(KeyValuePair<string, BlackjackChip> kvp in userIdToInsuranceChip)
            {
                BlackjackChip chip = kvp.Value;
                chip.transform.SetParent(bankerBoxBet.GetCardPosition.parent);
                AnimateMoveChipForward(chip, bankerBoxBet.GetCardPosition.position);
            }
        }

        // Đặt cược bảo hiểm
        if (data.Bet != null && data.Bet.Insurance > 0)
        {
            Player player = playingPlayers.Find(p => p.Id == data.Bet.UserId);
            BasePlayerView playerView = userIdToView.GetValueOrDefault(data.Bet.UserId);
            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
            // TO F
            userIdToInsuranceChip[data.Bet.UserId] = chip;
            chip.transform.SetParent(playerView.transform, true);
            chip.SetInfo(5, playerView.GetAvatarPosition(), data.Bet.Insurance);
            chip.transform.localScale = Vector2.one * 0.8f;
            AnimateMoveInsuranceChip(chip, listInsuranceChipPosition[GetPlayerIndexById(data.Bet.UserId)]);
            // thisPlayer.AnimateFlyMoney(-data.Bet.Insurance);

        }

        // Tách bài khi có 2 lá cùng Rank
        if (data.IsSplitHand)
        {
            BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(data.PlayerAction.UserId);
            isSplitingHand = true;
            boxBet.SplitBoxBet(data.Hand.First, data.Hand.Second);
        }

        if (data.PlayersBet.Count > 0)
        {
            foreach (BlackjackPlayerBet playerBet in data.PlayersBet)
            {
                BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(playerBet.UserId);
                if (playerBet.First > 0)
                {
                    boxBet.SetBetValue(GetChipIndex(playerBet.First), playerBet.First, playerBet.First);
                }
                if (playerBet.Second > 0)
                {
                    Debug.Log("REJOIN SETUP SECOND BOX CHIP: " + playerBet.Second + ", " + GetChipIndex(playerBet.Second));
                    boxBet.SecondBoxBet.SetBetValue(GetChipIndex(playerBet.Second), playerBet.Second, playerBet.Second);
                }
                // TO F
                if (playerBet.Insurance > 0)
                {
                    BasePlayerView playerView = userIdToView.GetValueOrDefault(playerBet.UserId);
                    BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                    userIdToInsuranceChip[playerBet.UserId] = chip;
                    chip.transform.SetParent(chipContainer);
                    chip.SetInfo(5, playerView.GetAvatarPosition(), playerBet.Insurance);
                    chip.transform.localScale = Vector2.one * 0.8f;
                    chip.transform.localPosition = listInsuranceChipPosition[GetPlayerIndexById(playerBet.UserId)];
                    
                }
            }
        }

    }


    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
        
        var data = BlackjackUpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("Update deal: " + data.ToString());

        if (data.IsBanker)
        {
            imageLight.SetActive(false);
            buttonBetContainer.gameObject.SetActive(false);
            if (hasDealtCardsForBanker)
            {
                foreach(KeyValuePair<string, BlackjackBoxBet> kvp in userIdToBoxBetView)
                {
                    BlackjackBoxBet boxbet = kvp.Value;
                    if (boxbet.HasBet)
                    {
                        boxbet.ShowHigherScore();
                        boxbet.SecondBoxBet.ShowHigherScore();                  
                    }
                }
                foreach (var player in userIdToView)
                {
                    BasePlayerView playerView = player.Value;
                    playerView.HideCountDown();
                }
            }
            // BANKER
            if (data.NewCards.Count > 0)
            {
                // Lật lá thứ 2 (lá úp)
                ShrinkCurrentPlayerBoxbet();
                Sequence sequence = DOTween.Sequence();
                if (data.IsRevealBankerHiddenCard && bankerBoxBet.listCardModel.Count >= 2)
                {
                    currentPlayerBoxBet.ShowHigherScore();
                    bankerBoxBet.EnlargeCards(1.4f);
                    Card flippedCard = data.NewCards[0];
                    CardModel flippedCardModel = bankerBoxBet.listCardModel[1];
                    BlackjackHand bankerHand = data.Hand.First;
                    listBankerCard[1] = flippedCard;
                    flippedCardModel.SetData((int)flippedCard.Rank, (int)flippedCard.Suit);
                    flippedCardModel.HideCard();
                    sequence
                        .AppendInterval(0.4f)
                        .AppendCallback(() =>
                        {
                            bankerBoxBet.StopHighlightCards();
                            AnimateFlipCard(flippedCardModel);
                        })
                        .AppendInterval(0.5f)
                        .AppendCallback(() =>
                        {
                            bankerBoxBet.SpreadCards();
                            bankerBoxBet.ShowScore(bankerHand.Point, bankerHand.MinPoint, bankerHand.MaxPoint, bankerHand.Type);
                        });
                    return;
                }

                // Chia 2 lá đầu tiên (1 là ngửa, 1 lá úp)
                if (!hasDealtCardsForBanker)
                {
                    listBankerCard.AddRange(data.NewCards);
                    StartCoroutine(DealCardsForBanker());
                    hasDealtCardsForBanker = true;
                }
                // Bốc 1 lá mới (lá 3, 4, 5...)
                else
                {
                    BlackjackHand bankerHand = data.Hand.First;

                    bankerBoxBet.EnlargeCards(1.4f);

                    foreach (Card newCard in data.NewCards)
                    {
                        CardModel newCardModel = InitCard();
                        newCardModel.SetData((int)newCard.Rank, (int)newCard.Suit);
                        newCardModel.transform.SetParent(bankerCardsContainer);
                        newCardModel.HideCard();
                        bankerBoxBet.listCardModel.Add(newCardModel);

                        sequence.AppendInterval(0.5f);
                        sequence.AppendCallback(() =>
                        {
                            bankerBoxBet.HideScore();
                            AnimateDealACard(newCardModel, bankerCardsContainer.position, true, null, bankerBoxBet.SpreadCards);
                        });

                    }
                    sequence.AppendInterval(1.2f)
                        .AppendCallback(() =>
                        {
                            Debug.Log("Show banker score after dealing new cards");
                            bankerBoxBet.ShowScore(bankerHand.Point, bankerHand.MinPoint, bankerHand.MaxPoint, bankerHand.Type);
                        });

                }
            }
        }
        else
        // PLAYER
        {
            if (data.NewCards.Count > 0)
            {
                BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(data.UserId);
                float delay = isSplitingHand ? 0f : 0f;

                foreach (Card card in data.NewCards)
                {
                    CardModel cardModel = InitCard();
                    cardModel.SetData((int)card.Rank, (int)card.Suit);
                    cardModel.HideCard();
                    if (data.HandN0 == BlackjackHandN0.BlackjackHand1St)
                    {
                        cardModel.transform.SetParent(boxBet.GetCardPosition);
                        boxBet.listCardModel.Add(cardModel);
                    }
                    else
                    {
                        cardModel.transform.SetParent(boxBet.SecondBoxBet.GetCardPosition);
                        boxBet.SecondBoxBet.listCardModel.Add(cardModel);
                    }
                }

                if (data.UserId == User.userProfile.UserId &&
                    (data.Hand.First.Type == BlackjackHandType.Blackjack ||
                    data.Hand.Second.Type == BlackjackHandType.Blackjack))
                {
                    isCurrentPlayerFinished = true;
                    isCurrentPlayerTurnPassed = true;
                    buttonBetContainer.gameObject.SetActive(false);
                }

                // Chia bài cho player
                if (!hasDealtCardsForPlayers)
                {
                    playerReceiveCardCount++;
                    if (playerReceiveCardCount == playingPlayers.Count)
                    {
                        StartCoroutine(DealCardsForPlayers());
                        hasDealtCardsForPlayers = true;
                    }
                }
                else
                {

                    // Player bốc 1 lá mới
                    // newCardModel.SetData((int)newCard.Rank, (int)newCard.Suit);
                    // newCardModel.transform.SetParent(boxBet.GetCardPosition);
                    // newCardModel.HideCard();

                    // Box bài 1
                    if (data.HandN0 == BlackjackHandN0.BlackjackHand1St)
                    {
                        CardModel newCardModel = boxBet.listCardModel[^1];
                        DOVirtual.DelayedCall(delay, () =>
                        {
                            boxBet.HideScore();
                            AnimateDealACard(newCardModel, boxBet.GetNewCardPosition(), true, null, () =>
                            {
                                boxBet.SpreadCards();
                            });

                        });
                    }
                    // Box bài 2
                    else
                    {
                        CardModel newCardModel = boxBet.SecondBoxBet.listCardModel[^1];
                        DOVirtual.DelayedCall(delay, () =>
                        {
                            boxBet.SecondBoxBet.HideScore();
                            AnimateDealACard(newCardModel, boxBet.SecondBoxBet.GetNewCardPosition(), true, null, () =>
                            {
                                boxBet.SecondBoxBet.SpreadCards();
                            });

                        });
                    }
                    CheckCurrentPlayerFinish(data);
                }
                DOTween.Sequence()
                    .AppendInterval(1f + delay)
                    .AppendCallback(() =>
                    {
                        // Show điểm box 1
                        if (data.Hand.First != null && data.HandN0 == BlackjackHandN0.BlackjackHand1St)
                        {
                            boxBet.ShowScore(data.Hand.First.Point, data.Hand.First.MinPoint, data.Hand.First.MaxPoint, data.Hand.First.Type);
                        }

                        // Show điểm box 2
                        if (data.Hand.Second != null && data.HandN0 == BlackjackHandN0.BlackjackHand2Nd)
                        {
                            boxBet.SecondBoxBet.ShowScore(data.Hand.Second.Point, data.Hand.Second.MinPoint, data.Hand.Second.MaxPoint, data.Hand.Second.Type);
                        }

                    })
                    .AppendInterval(1f)
                    .AppendCallback(() =>
                    {
                        // Nếu thisPlayer ăn được Blackjack thì hiện animation Blackjack
                        if (data.UserId == User.userProfile.UserId &&
                            (data.Hand.First.Type == BlackjackHandType.Blackjack ||
                            data.Hand.Second.Type == BlackjackHandType.Blackjack))
                        {
                            effectContainer.gameObject.SetActive(true);
                            Utility.PlayAnimation(animationWinBlackjack, "animation", false);
                            // animationWinBlackjack.gameObject.SetActive(true);
                            animationWinBlackjack.AnimationState.Complete += delegate
                            {
                                effectContainer.gameObject.SetActive(false);
                                animationWinBlackjack.gameObject.SetActive(false);
                            };
                        }

                    });

                // Check xem player đã chơi hết lượt chưa (Busted/Blackjack)

            }
        }

        // Rejoin Table
        if (data.AllPlayerHand.Count > 0)
        {
            Debug.Log("UPDATE DEAL REJOIN TABLE");
            hasDealtCardsForBanker = true;
            hasDealtCardsForPlayers = true;
            phasePlay.SetActive(true);
            foreach (BlackjackPlayerHand playerHand in data.AllPlayerHand)
            {
                // Setup banker
                if (string.IsNullOrEmpty(playerHand.UserId))
                {
                    hasDealtCardsForBanker = true;
                    foreach (Card newCard in playerHand.First.Cards)
                    {
                        CardModel newCardModel = InitCard();
                        newCardModel.transform.SetParent(bankerCardsContainer);
                        newCardModel.transform.position = bankerCardsContainer.position;
                        newCardModel.transform.localScale = Vector2.one * 0.5f;
                        if (newCard.Rank != CardRank.RankUnspecified && newCard.Suit != CardSuit.SuitUnspecified)
                        {
                            newCardModel.SetData((int)newCard.Rank, (int)newCard.Suit);
                            newCardModel.ShowCard();
                        }
                        else
                        {
                            newCardModel.HideCard();
                        }
                        bankerBoxBet.listCardModel.Add(newCardModel);

                        listBankerCard.Add(newCard);


                    }
                    bankerBoxBet.SpreadCards();
                    if (playerHand.First.Point > 0)
                    {
                        bankerBoxBet.ShowScore(playerHand.First.Point, playerHand.First.MinPoint, playerHand.First.MaxPoint, playerHand.First.Type);
                    }

                    if ((bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankA
                        || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.Rank10
                        || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankJ
                        || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankQ
                        || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankK)
                        && GameState == GameState.Play
                    )
                    {
                        Debug.Log("bankerBoxBet.AnimateHighlightCards();");
                        bankerBoxBet.AnimateHighlightCards();
                    }
                }
                // Setup Player
                else
                {
                    hasDealtCardsForPlayers = true;
                    BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(playerHand.UserId);
                    if (playerHand.First.Cards.Count > 0)
                    {
                        if (playerHand.UserId == User.userProfile.UserId)
                        {
                            isPlaying = true;
                            
                        }
                        foreach (Card card in playerHand.First.Cards)
                        {
                            CardModel cardModel = InitCard();
                            cardModel.SetData((int)card.Rank, (int)card.Suit);
                            cardModel.transform.SetParent(boxBet.GetCardPosition);
                            cardModel.transform.position = boxBet.GetCardPosition.position;
                            cardModel.transform.localScale = Vector2.one * 0.5f;
                            cardModel.ShowCard();
                            boxBet.listCardModel.Add(cardModel);
                        }
                        boxBet.SpreadCards();
                        boxBet.ShowScore(playerHand.First.Point, playerHand.First.MinPoint, playerHand.First.MaxPoint, playerHand.First.Type);
                    }
                    if (playerHand.Second.Cards.Count > 0)
                    {
                        Debug.Log("REJOIN TABLE - SHOW SECOND BOX");
                        isSplitingHand = true;
                        boxBet.ShowSecondBox();
                        // boxBet.SecondBoxBet.ResetSecondBox();
                        foreach (Card card in playerHand.Second.Cards)
                        {
                           
                            CardModel cardModel = InitCard();
                            cardModel.transform.SetParent(boxBet.SecondBoxBet.GetCardPosition);
                            cardModel.transform.position = boxBet.SecondBoxBet.GetCardPosition.position;
                            cardModel.transform.localScale = Vector2.one * 0.5f;
                            cardModel.SetData((int)card.Rank, (int)card.Suit);
                            cardModel.ShowCard();
                            boxBet.SecondBoxBet.listCardModel.Add(cardModel);
                        }
                        boxBet.SecondBoxBet.SpreadCards();
                        boxBet.SecondBoxBet.ShowScore(playerHand.Second.Point, playerHand.Second.MinPoint, playerHand.Second.MaxPoint, playerHand.Second.Type);
                    }

                    foreach (CardModel card in boxBet.SecondBoxBet.listCardModel)
                    {
                         Debug.Log("SECOND BOX CARD: " + card.GetRank() + " " + card.GetSuit());
                    }

                }
            }

            int playerIndex = playingPlayers.FindIndex(p => p.Id == data.UserId);
            // if (isPlaying && !isCurrentPlayerFinished && playerIndex <= currentPlayerIndex && GameState == GameState.Play)
            // {
            //     buttonBetContainer.gameObject.SetActive(true);
            //     buttonDouble.gameObject.SetActive(true);
            //     buttonSplit.gameObject.SetActive(true);
            //     buttonHit.gameObject.SetActive(true);
            //     buttonStand.gameObject.SetActive(true);

            //     // // Turn của người chơi hiện tại
            //     // if (data.Actions != null)
            //     // {
            //     //     buttonBetContainer.gameObject.SetActive(true);
            //     //     List<BlackjackActionCode> actions = data.Actions.Actions.ToList();
            //     //     buttonDouble.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionDouble);
            //     //     buttonSplit.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionSplit);
            //     //     buttonHit.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionHit);
            //     //     buttonStand.button.interactable = actions.Contains(BlackjackActionCode.BlackjackActionStay);
            //     // }

            //     // // Turn của người chơi khác
            //     // if (data.InTurn != User.userProfile.UserId)
            //     // {
            //     //     buttonDouble.button.interactable = true;
            //     //     buttonSplit.button.interactable = false;
            //     //     buttonHit.button.interactable = true;
            //     //     buttonStand.button.interactable = true;
            //     // }
            //     UpdateButtonActionVisual(isCurrentPlayerTurn);
            // }
            // else
            // {
            //     buttonBetContainer.gameObject.SetActive(false);
            // }
        }
    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
        UpdateGameState data = UpdateGameState.Parser.ParseFrom(matchState.State);
        GameState = data.State;
        switch (data.State)
        {
            case GameState.Idle:
                Debug.Log("HandleUpdateGameState Idle " + data.ToString());
                countdownContainer.SetActive(false);
                break;
            case GameState.Matching:
                Debug.Log("HandleUpdateGameState Matching " + data.ToString());
                if (countdownCoroutine != null)
                    StopCoroutine(countdownCoroutine);
                countdownContainer.SetActive(false);
                if (isCountingDown)
                {
                    isCountingDown = false;
                    if (totalBetValue == 0)
                    {
                        lastBetValue = 0;
                    }
                    HideAllPlayersLoading();
                    phaseBet.SetActive(false);
                    phasePlay.SetActive(false);
                    currentPlayerBoxBet.Reset();
                }
                break;
            case GameState.Preparing:
                if (!isCountingDown || data.CountDown == 12)
                {
                    totalBetValue = 0;
                    isCountingDown = true;
                    ShowCountDown();
                    phaseBet.SetActive(true);
                    phasePlay.SetActive(false);
                    ResetGame();
                    StartCountDownBetTime((int)data.CountDown);
                }
                textCountdown.text = data.CountDown.ToString();

                ShowAllPlayersLoading();
                Debug.Log("HandleUpdateGameState Preparing " + data.ToString());
                break;
            case GameState.Play:
                Debug.Log("HandleUpdateGameState Play " + data.ToString());
                if (isCountingDown)
                {
                    if (countdownCoroutine != null)
                        StopCoroutine(countdownCoroutine);
                    HideCountDown();
                    isCountingDown = false;
                    lastBetValue = totalBetValue;
                    currentPlayerBoxBet.SetBetValue(GetChipIndex((int)totalBetValue), totalBetValue, totalBetValue);
                    countdownContainer.SetActive(false);
                    phaseBet.gameObject.SetActive(false);
                    phasePlay.gameObject.SetActive(true);
                    imageLight.gameObject.SetActive(false);
                    HideAllPlayersLoading();
                    if (!hasBet)
                    {
                        currentPlayerBoxBet.HideImageChip();
                    }
                }
                break;
            case GameState.Reward:
                imageLight.gameObject.SetActive(false);
                Debug.Log("HandleUpdateGameState Reward " + data.ToString());
                break;
            case GameState.Finish:
                phasePlay.gameObject.SetActive(false);
                Debug.Log("HandleUpdateGameState Finish " + data.ToString());
                break;

        }
    }



    public override void HandleUpdateTurn(IMatchState matchState)
    {
        var data = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log("Update turn: " + data.ToString());
    }

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var data = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("Update wallet: " + data.ToString());
        listBalanceUpdate = data.Updates.ToList();
        foreach (BalanceUpdate update in data.Updates)
        {
            // BasePlayerView playerView = userIdToView.GetValueOrDefault(update.UserId);
            // if (playerView == null) continue;
            // playerView.AnimateFlyMoney(update.AmountChipAdd);
            // playerView.SetCurrentChip(update.AmountChipCurrent);
            if (update.UserId == User.userProfile.UserId)
            {
                playerWallet = update.AmountChipCurrent;
            }
        }
        UpdateChipBetInteractivity();

    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var data = BlackjackUpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("Update finish: " + data.ToString());
        imageLight.SetActive(false);
        bankerBoxBet.ShrinkCards();
        buttonBetContainer.gameObject.SetActive(false);
        bankerBoxBet.ShowHigherScore();
        AnimateBankerTransferChipsToPlayers(data);
        // DOVirtual.DelayedCall(5.5f, () =>
        // {
        //     foreach (BlackjackPLayerBetResult result in data.BetResults)
        //     {
        //         if (userIdToBoxBetView.TryGetValue(result.UserId, out var boxBet))
        //         {
        //             boxBet.HideScore();
        //         }
        //         if (userIdToView.TryGetValue(result.UserId, out var playerView))
        //         {
        //             if (result.First.IsWin == 1)
        //             {
        //                 playerView.SetEffectWin("animation", false);
        //             }
        //             else if (result.First.IsWin == -1)
        //             {
        //                 playerView.SetEffectLose("lose", false);
        //             }
        //             else
        //             {
        //                 playerView.SetEffectDraw("animation", false);
        //             }
        //         }
        //     }
        //     bankerBoxBet.HideScore();

        // });
    }

    // public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    // {
    //     Destroy(gameObject);
    // }
    #endregion

    private void StartCountDownBetTime(int time)
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownRoutine(time));
    }

    private IEnumerator CountdownRoutine(int startTime)
    {
        Debug.Log("start count down routine");
        countdownContainer.SetActive(true);
        float timeLeft = startTime;
        imageCountdown.fillAmount = 1f;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            imageCountdown.fillAmount = timeLeft / 12;
            yield return null;
        }
        HideCountDown();
        // Hết thời gian
    }

    #region Buttons

    /// --------- PHASE BET ------------ ///
    public void OnClickButtonChip(int index)
    {

        currentBetValue += listValueChipBets[index];
        currentChipIndex = index;

        SetEnableRebetButton(false);
        // buttonRebet.gameObject.SetActive(false);
        textDealValue.text = Utility.FormatMoney(currentBetValue, true);
        textClearValue.text = Utility.FormatMoney(currentBetValue, true);

        // Chip
        foreach (BlackjackChipBet chipBet in listChipBet)
        {
            chipBet.OnUnselect();
        }
        BlackjackChipBet selectedChipBet = listChipBet[index];
        selectedChipBet.OnSelect();
        BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
        chip.transform.SetParent(selectedChipBet.transform.parent, true);
        chip.SetInfo(index, selectedChipBet.transform.localPosition);
        chip.transform.localScale = Vector2.zero * 0.3f;
        if (isRebet)
        {
            currentBetValue = 0;
            AnimateMoveChipForward(chip, currentPlayerBoxBet.transform.position, () =>
            {
                currentPlayerBoxBet.SetBetValue(GetChipIndex(totalBetValue), totalBetValue, totalBetValue);
                OnClickButtonDeal();
            });
        }
        else
        {
            // buttonClear.SetActive(true);
            if (GameState == GameState.Play)
            {
                currentPlayerBoxBet.SetBetValue(GetChipIndex(totalBetValue), totalBetValue, totalBetValue);
            }
            else
            {
                currentPlayerBoxBet.SetBetValue(GetChipIndex(currentBetValue), currentBetValue, totalBetValue + currentBetValue);
            }
            buttonDeal.SetActive(true);
            buttonClear.SetActive(true);
            AnimateMoveChipForward(chip, currentPlayerBoxBet.transform.position);
        }

    }

    public void OnClickButtonDeal()
    {
        if (playerWallet < currentBetValue)
        {
            ShowNotEnoughChipDialog();
            return;
        }
        if (totalBetValue + currentBetValue > MarkUnit * 100)
        {
            UIManager.Instance.ShowToast("You must bet at most " + Utility.FormatNumber(MarkUnit * 100) + " chips!", 2, transform);
            return;
        }
        lastChipIndex = currentChipIndex;
        totalBetValue += currentBetValue;
        Debug.Log("CLICK BUTTON DEAL");
        BlackjackBet bet = new BlackjackBet
        {
            Chips = currentBetValue,
            Code = BlackjackBetCode.BlackjackBetNormal
        };

        if (!isRebet)
        {
            DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
            textDealValue.text = currentBetValue.ToString();
            textClearValue.text = currentBetValue.ToString();
            currentPlayerBoxBet.SetBetValue(GetChipIndex(totalBetValue), totalBetValue, totalBetValue);
        }
        isRebet = false;
        currentBetValue = 0;
        foreach (BlackjackChipBet chipBet in listChipBet)
        {
            chipBet.OnUnselect();
        }

        // buttonDoubleBet.gameObject.SetActive(true);
        SetEnableDoubleBetButton(true);
        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);

    }

    public void OnClickButtonClear()
    {
        Debug.Log("OnClickButtonClear");
        currentBetValue = 0;
        textDealValue.text = currentBetValue.ToString();
        textClearValue.text = currentBetValue.ToString();
        foreach (BlackjackChipBet chipBet in listChipBet)
        {
            chipBet.OnUnselect();
        }
        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);
        currentPlayerBoxBet.SetBetValue(GetChipIndex(totalBetValue), totalBetValue, totalBetValue);

        BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
        chip.transform.SetParent(currentPlayerBoxBet.transform.parent, true);
        chip.SetInfo(currentChipIndex, currentPlayerBoxBet.transform.localPosition);
        chip.transform.localScale = Vector2.one * 0.8f;
        AnimateClearChip(chip);
        SetEnableRebetButton(lastBetValue > 0 && totalBetValue == 0);
    }

    public void OnClickButtonDoubleBet()
    {
        if (playerWallet < totalBetValue)
        {
            ShowNotEnoughChipDialog();
            return;
        }
        BlackjackBet bet = new BlackjackBet
        {
            Chips = totalBetValue,
            Code = BlackjackBetCode.BlackjackBetDouble
        };
        // buttonDoubleBet.gameObject.SetActive(false);
        isClickDoubleBet = true;
        DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
        SetEnableDoubleBetButton(false);
    }

    public void OnClickButtonRebet()
    {
        if (playerWallet < lastBetValue)
        {
            ShowNotEnoughChipDialog();
            return;
        }
        isRebet = true;
        BlackjackBet bet = new BlackjackBet
        {
            // Chips = lastBetValue,
            Code = BlackjackBetCode.BlackjackBetRebet
        };
        // buttonRebet.gameObject.SetActive(false);
        SetEnableDoubleBetButton(false);
        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);
        DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
    }

    /// --------- PHASE PLAY ------------ ///

    public void OnClickButtonSplit()
    {
        if (playerWallet < totalBetValue)
        {
            ShowNotEnoughChipDialog();
            return;
        }
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionSplit);
            buttonBetContainer.gameObject.SetActive(false);
        }
        else
        {
            if (!buttonSplit.isChecked)
            {
                ResetAllButtonAction();
                buttonSplit.OnClickCheckBox();
                nextActionCode = BlackjackActionCode.BlackjackActionSplit;
            }
            else
            {
                buttonSplit.Reset();
                nextActionCode = BlackjackActionCode.BlackjackActionUnspecified;
            }
        }
    }

    public void OnClickButtonDouble()
    {
        if (playerWallet < totalBetValue)
        {
            ShowNotEnoughChipDialog();
            return;
        }
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionDouble);
            if (!isSplitingHand || (isSplitingHand && isCurrentPlayerTurn && blackjackHandN0 == BlackjackHandN0.BlackjackHand2Nd))
            {
                isCurrentPlayerFinished = true;
            }
            if (blackjackHandN0 == BlackjackHandN0.BlackjackHand1St)
            {
                currentPlayerBoxBet.ShrinkCards();
            }
            else if (blackjackHandN0 == BlackjackHandN0.BlackjackHand2Nd)
            {
                currentPlayerBoxBet.SecondBoxBet.ShrinkCards();
            }
            buttonBetContainer.gameObject.SetActive(false);

        }
        else
        {
            if (!buttonDouble.isChecked)
            {
                ResetAllButtonAction();
                buttonDouble.OnClickCheckBox();
                nextActionCode = BlackjackActionCode.BlackjackActionDouble;
            }
            else
            {
                buttonDouble.Reset();
                nextActionCode = BlackjackActionCode.BlackjackActionUnspecified;
            }
        }
    }

    public void OnClickButtonHit()
    {
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionHit);
            buttonBetContainer.gameObject.SetActive(false);
        }
        else
        {
            if (!buttonHit.isChecked)
            {
                ResetAllButtonAction();
                buttonHit.OnClickCheckBox();
                nextActionCode = BlackjackActionCode.BlackjackActionHit;
            }
            else
            {
                buttonHit.Reset();
                nextActionCode = BlackjackActionCode.BlackjackActionUnspecified;
            }
        }

    }

    public void OnClickButtonStand()
    {
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionStay);
            buttonBetContainer.gameObject.SetActive(false);
            currentPlayerBoxBet.ShowHigherScore();
            currentPlayerBoxBet.SecondBoxBet.ShowHigherScore();
            if (blackjackHandN0 == BlackjackHandN0.BlackjackHand1St)
            {
                currentPlayerBoxBet.ShrinkCards();
            }
            else if (blackjackHandN0 == BlackjackHandN0.BlackjackHand2Nd)
            {
                currentPlayerBoxBet.SecondBoxBet.ShrinkCards();
            }
            if (!isSplitingHand || (isSplitingHand && isCurrentPlayerTurn && blackjackHandN0 == BlackjackHandN0.BlackjackHand2Nd))
            {
                isCurrentPlayerFinished = true;
            }
            buttonBetContainer.gameObject.SetActive(false);
        }
        else
        {
            if (!buttonStand.isChecked)
            {
                ResetAllButtonAction();
                Debug.Log("TICK BUTTON");
                buttonStand.OnClickCheckBox();
                nextActionCode = BlackjackActionCode.BlackjackActionStay;
            }
            else
            {
                buttonStand.Reset();
                nextActionCode = BlackjackActionCode.BlackjackActionUnspecified;
            }
        }
    }

    public void OnClickMenuButton()
    {
        UIManager.Instance.OpenGroupMenu();
    }

    public override void OpenRule()
    {
        Instantiate(rulePrefab, transform);
    }

    private void HandleClickButtonAction(BlackjackActionCode code)
    {
        BlackjackAction blackjackAction = new()
        {
            Code = code
        };
        DataSender.SendMatchState((long)OpCodeRequest.DeclareCards, blackjackAction.ToByteArray());
    }

    private void UpdateButtonActionVisual(bool isCurrentPlayerTurn)
    {
        if (isCurrentPlayerTurn)
        {
            buttonDouble.HideCheckBox();
            buttonHit.HideCheckBox();
            buttonSplit.HideCheckBox();
            buttonStand.HideCheckBox();
            ResetAllButtonAction();
        }
        else
        {
            buttonDouble.ShowCheckBox();
            buttonHit.ShowCheckBox();
            buttonSplit.ShowCheckBox();
            buttonStand.ShowCheckBox();
        }

    }

    private void ResetAllButtonAction()
    {
        buttonDouble.Reset();
        buttonHit.Reset();
        buttonSplit.Reset();
        buttonStand.Reset();

    }

    private void ShowCountDown()
    {
        countdownContainer.transform.position = new Vector2(countdownPosition.x, countdownPosition.y + 400f);
        countdownContainer.SetActive(true);
        countdownContainer.transform.DOMoveY(countdownPosition.y, 0.5f).SetEase(Ease.OutBack);
    }

    private void HideCountDown()
    {
        countdownContainer.transform.DOMoveY(countdownPosition.y + 400f, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            phaseBet.SetActive(false);
            countdownContainer.gameObject.SetActive(false);
            countdownContainer.transform.position = countdownPosition;
        });
    }
    #endregion

    #region Cards
    private IEnumerator DealCardsForPlayers()
    {
        int length = rearrangedPlayers.Count;
        for (int i = 0; i < length * 2; i++)
        {

            int indexInRound = i % length;
            int round = i / length; // vòng chia thứ mấy
            Player player = rearrangedPlayers[indexInRound];
            BlackjackBoxBet boxCard = userIdToBoxBetView.GetValueOrDefault(player.Id);
            if (boxCard.listCardModel.Count == 0) continue;
            CardModel cardModel = boxCard.listCardModel[round];
            AnimateDealACard(cardModel, boxCard.GetCardPosition.position, true, boxCard, boxCard.SpreadCards);

            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator DealCardsForBanker()
    {
        for (int i = 0; i < listBankerCard.Count; i++)
        {
            Card card = listBankerCard[i];
            CardModel cardModel = InitCard();
            cardModel.HideCard();
            // cardModel.transform.SetParent(dealCardContainer);
            cardModel.transform.SetParent(bankerCardsContainer);

            bool isShow = card.Rank != CardRank.RankUnspecified && card.Suit != CardSuit.SuitUnspecified;
            if (isShow)
            {
                cardModel.SetData((int)card.Rank, (int)card.Suit);
                cardModel.HideCard();
                AnimateDealACard(cardModel, bankerCardsContainer.position, true, null, bankerBoxBet.SpreadCards);
            }
            else
            {
                AnimateDealACard(cardModel, bankerCardsContainer.position, false, null, bankerBoxBet.SpreadCards);
            }
            bankerBoxBet.listCardModel.Add(cardModel);
            yield return new WaitForSeconds(0.2f);
        }
        if (bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankA
            || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.Rank10
            || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankJ
            || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankQ
            || bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankK
        )
        {
            bankerBoxBet.AnimateHighlightCards();
        }
    }

    #endregion

    #region Card Animations
    private void AnimateDealACard(CardModel cardModel, Vector2 targetPosition, bool isShow, BlackjackBoxBet boxBet = null, Action callback = null)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(cardModel.transform.DOMove(targetPosition, DEAL_CARD_ANIMATION_TIME).SetEase(Ease.InOutCubic))
            .Join(cardModel.transform.DOScale(0.48f, DEAL_CARD_ANIMATION_TIME))
            .AppendCallback(() =>
            {
                if (isShow)
                {
                    cardModel.transform.DOScale(new Vector2(0.01f, 0.5f), DEAL_CARD_ANIMATION_TIME / 2f).OnComplete(() =>
                    {
                        cardModel.ShowCard();
                        cardModel.transform.DOScale(0.5f, DEAL_CARD_ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
                    });
                    Quaternion newRotation = Quaternion.Euler(0, 10, 0);
                    cardModel.transform.DOLocalRotate(newRotation.eulerAngles, DEAL_CARD_ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
                    {
                        newRotation = Quaternion.Euler(0, -10, 0);
                        cardModel.transform.localRotation = newRotation;
                        cardModel.transform.DOLocalRotate(Vector3.zero, DEAL_CARD_ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
                    });
                }
                callback?.Invoke();

            });
    }

    private void AnimateFlipCard(CardModel cardModel)
    {
        SoundManager.Instance.PlayEffectFromPath(SoundBlackjack.CARD_FLIP);
        cardModel.transform.DOScale(new Vector2(0.01f, 1f), DEAL_CARD_ANIMATION_TIME / 2f).OnComplete(() =>
        {
            cardModel.ShowCard();
            cardModel.transform.DOScale(0.5f, DEAL_CARD_ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        cardModel.transform.DOLocalRotate(newRotation.eulerAngles, DEAL_CARD_ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            cardModel.transform.localRotation = newRotation;
            cardModel.transform.DOLocalRotate(Vector3.zero, DEAL_CARD_ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
    }

    private void AnimateReturnAllCards()
    {
        Sequence seq = DOTween.Sequence();

        float delayBetweenCards = 0.15f;
        int cardIndex = 0;

        // Gom cả player và banker vào chung một list để xử lý
        List<BlackjackBoxBet> allBoxBets = new List<BlackjackBoxBet>();
        foreach (Player player in rearrangedPlayers)
        {
            allBoxBets.Add(userIdToBoxBetView.GetValueOrDefault(player.Id));
            allBoxBets.Add(userIdToBoxBetView.GetValueOrDefault(player.Id).SecondBoxBet);
        }
        allBoxBets.Add(bankerBoxBet);
        bankerBoxBet.HideScore();
        // --- FLIP CARDS ---
        foreach (BlackjackBoxBet boxBet in allBoxBets)
        {
            boxBet.HideScore();
            foreach (CardModel card in boxBet.listCardModel)
            {
                card.SetBorder(false);
                card.SetDark(false);
                card.HideSparkleAnimation();
                float startTime = cardIndex * delayBetweenCards;
                card.transform.SetSiblingIndex(50 + cardIndex);

                // Scale down (để lật)
                seq.Insert(startTime, card.transform.DOScaleX(0.01f, DEAL_CARD_ANIMATION_TIME / 2f));

                // Callback để đổi mặt lá bài
                seq.InsertCallback(startTime + DEAL_CARD_ANIMATION_TIME / 2f, () =>
                {
                    card.HideCard();
                });

                // Scale back
                seq.Insert(startTime + DEAL_CARD_ANIMATION_TIME / 2f,
                    card.transform.DOScale(0.5f, DEAL_CARD_ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic));

                // Rotate effect
                seq.Insert(startTime,
                    card.transform.DOLocalRotate(new Vector3(0, 10, 0), DEAL_CARD_ANIMATION_TIME / 5).SetEase(Ease.InOutCubic));

                cardIndex++;
            }
        }

        // --- RETURN CARDS ---
        foreach (BlackjackBoxBet boxBet in allBoxBets)
        {
            foreach (CardModel card in boxBet.listCardModel)
            {
                float startTime = cardIndex * delayBetweenCards;

                // Đặt parent về return container
                // seq.InsertCallback(startTime, () =>
                // {
                //     card.transform.SetParent(cardContainer, true);
                // });

                // Move + Scale song song
                seq.Insert(startTime + 0.5f,
                    card.transform.DOMove(returnCardContainer.position, 0.5f)
                        .SetEase(Ease.OutCubic)
                        .OnComplete(() =>
                        {
                            if (card.gameObject.activeSelf)
                            {
                                // cardPool.Release(card);
                                PoolService.Instance.Release(PrefabType.Card, card);
                            }
                        })
                );

                seq.Insert(startTime + 0.5f,
                    card.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InCubic) // scale về 0
                );

                cardIndex++;


            }
        }
        seq.OnComplete(() =>
        {
            ResetGame();
        });
    }
    #endregion

    #region Chip Animations
    private void AnimateMoveChipForward(BlackjackChip chip, Vector2 position, Action callback = null, float time = 0.3f)
    {
        chip.transform.DOScale(0.8f, time);
        chip.transform
            .DOMove(position, time)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                // Vector2 randomPosition = new Vector2(
                //     targetPosition.x + UnityEngine.Random.Range(-30, 30),
                //     targetPosition.y + UnityEngine.Random.Range(-8, 8)
                // );

                // chip.transform.DOLocalJump(randomPosition, 20, 1, 0.2f);
                PoolService.Instance.Release(PrefabType.ChipPlayerBlackjack, chip);
                callback?.Invoke();
            });
    }

    private void AnimateMoveChipBackward(BlackjackChip chip, Vector2 position, Action callback = null, float time = 0.15f)
    {
        chip.transform.DOScale(0.3f, time);
        chip.transform
            .DOMove(position, time)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                // Vector2 randomPosition = new Vector2(
                //     targetPosition.x + UnityEngine.Random.Range(-30, 30),
                //     targetPosition.y + UnityEngine.Random.Range(-8, 8)
                // );

                // chip.transform.DOLocalJump(randomPosition, 20, 1, 0.2f);
                // PoolService.Instance.Release(PrefabType.ChipPlayerBlackjack, chip);
                chip.gameObject.SetActive(false);
                callback?.Invoke();
            });
    }

    private void AnimateMoveInsuranceChip(BlackjackChip chip, Vector2 targetPosition)
    {
        chip.transform.SetParent(chipContainer, true);
        chip.transform
            .DOLocalMove(targetPosition, 0.3f)
            .SetEase(Ease.InSine);

    }

    private void AnimateClearChip(BlackjackChip chip)
    {
        if (!chip.gameObject.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = chip.gameObject.AddComponent<CanvasGroup>();
        }

        chip.transform.DOScale(Vector3.one * 0.3f, 0.5f);
        chip.transform.DOLocalMove(new Vector3(0, 15, 0), 0.4f);
        canvasGroup.DOFade(0, 1f).OnComplete(() =>
        {
            canvasGroup.alpha = 1f;
            PoolService.Instance.Release(PrefabType.ChipPlayerBlackjack, chip);
        });
    }

    private void AnimateBankerTransferChipsToPlayers(BlackjackUpdateFinish data)
    {
        List<BlackjackPLayerBetResult> listBetResult = data.BetResults.ToList();
        Sequence sequence = DOTween.Sequence();
        foreach (BlackjackPLayerBetResult betResult in listBetResult)
        {
            BlackjackBoxBet boxbet = userIdToBoxBetView.GetValueOrDefault(betResult.UserId);
            BasePlayerView playerView = userIdToView.GetValueOrDefault(betResult.UserId);
            BlackjackBetResult firstHandResult = betResult.First;
            BlackjackBetResult secondHandResult = betResult.Second;
            if (firstHandResult.BetAmount > 0)
            {
                boxbet.ShowResult(firstHandResult.IsWin);
            }
            if (secondHandResult.BetAmount > 0)
            {
                boxbet.SecondBoxBet.ShowResult(secondHandResult.IsWin);
            }

            if (betResult.Insurance.IsWin == 1)
            {
                Debug.Log("TRẢ TIỀN BẢO HIỂM");
                // Insurance
                sequence
                    .AppendInterval(0.5f)
                    .AppendCallback(() =>
                    {
                        Debug.Log("ANIMATE INSURANCE WIN");
                        if (userIdToInsuranceChip.TryGetValue(betResult.UserId, out BlackjackChip insuranceChip))
                        {
                            AnimateMoveChipBackward(insuranceChip, playerView.GetAvatarTransform().position, () =>
                            {
                                boxbet.AnimateFlyMoney(betResult.Insurance.Total);
                                playerView.SetCurrentChip(playerView.CurrentChip + betResult.Insurance.Total);
                            });
                            
                        }
                        BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            // TODO
                        int chipIndex = GetChipIndex((int)betResult.Insurance.WinAmount);
                        chip.transform.SetParent(bankerBoxBet.GetCardPosition.parent, true);
                        chip.SetInfo(chipIndex, bankerBoxBet.GetCardPosition.localPosition);
                        chip.transform.localScale = Vector2.one * 0.3f;
                        AnimateMoveChipBackward(chip, playerView.GetAvatarTransform().position, null, 0.3f);
                    });
            }

            sequence

                // .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    if (firstHandResult.BetAmount > 0)
                    {
                        if (firstHandResult.IsWin > 0) // Win
                        {
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            // TODO
                            int chipIndex = GetChipIndex((int)firstHandResult.WinAmount);
                            chip.transform.SetParent(bankerBoxBet.GetCardPosition.parent, true);
                            chip.SetInfo(chipIndex, bankerBoxBet.GetCardPosition.localPosition);
                            chip.transform.localScale = Vector2.one * 0.3f;
                            AnimateMoveChipForward(chip, boxbet.ChipWinPosition.position, () => boxbet.SetWinChipVisual(chipIndex, firstHandResult.WinAmount - firstHandResult.FeeAmount), 0.3f);
                        }
                    }

                    if (secondHandResult.BetAmount > 0)
                    {
                        if (secondHandResult.IsWin > 0) // Win
                        {
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            // TODO
                            int chipIndex = GetChipIndex((int)secondHandResult.WinAmount);
                            chip.transform.SetParent(bankerBoxBet.GetCardPosition.parent, true);
                            chip.SetInfo(chipIndex, bankerBoxBet.GetCardPosition.localPosition);
                            chip.transform.localScale = Vector2.one * 0.3f;
                            AnimateMoveChipForward(chip, boxbet.SecondBoxBet.ChipWinPosition.position, () => boxbet.SecondBoxBet.SetWinChipVisual(chipIndex, secondHandResult.WinAmount - secondHandResult.FeeAmount), 0.3f);
                        }

                    }
                })
                .AppendInterval(0.5f);
        }

        sequence
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                // Chip fly to all user
                foreach (BalanceUpdate update in listBalanceUpdate)
                {
                }
                Dictionary<string, BalanceResultData> playerResults = listBetResult
                    .Where(r => !string.IsNullOrEmpty(r.UserId))
                    .ToDictionary(
                        r => r.UserId,
                        r => new BalanceResultData(
                            r.First.BetAmount,
                            r.Second.BetAmount,
                            r.First.Total,
                            r.Second.Total,
                            r.First.IsWin,
                            r.Second.IsWin
                        )
                    );
                Dictionary<string, long> playerWallet = listBalanceUpdate
                    .Where(r => !string.IsNullOrEmpty(r.UserId))
                    .ToDictionary(
                        r => r.UserId,
                        r => r.AmountChipCurrent
                    );
                foreach (KeyValuePair<string, BlackjackBoxBet> kvp in userIdToBoxBetView)
                {
                    BlackjackBoxBet boxbet = kvp.Value;
                    BasePlayerView playerView = userIdToView.GetValueOrDefault(kvp.Key);
                    if (playerResults.TryGetValue(kvp.Key, out BalanceResultData data))
                    {
                        // Return chip firsthand
                        if (data.IsWinFirstHand > 0)
                        {
                            boxbet.HideImageChip();
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            int chipIndex = GetChipIndex((int)data.FirstHandBetAmount);
                            chip.transform.SetParent(boxbet.GetCardPosition.parent, true);
                            // chip.transform.localPosition = boxbet.ChipPosition.position;
                            chip.SetInfo(chipIndex, boxbet.ChipPosition.localPosition);
                            chip.transform.localScale = Vector2.one * 0.8f;
                            AnimateMoveChipBackward(chip, playerView.GetAvatarTransform().position, () =>
                            {
                                boxbet.HideImageChipWin();
                                BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                                int chipIndex = GetChipIndex((int)data.FirstHandBetAmount);
                                chip.transform.SetParent(boxbet.GetCardPosition.parent, true);
                                // chip.transform.position = boxbet.ChipWinPosition.position;
                                chip.SetInfo(chipIndex, boxbet.ChipWinPosition.localPosition);
                                chip.transform.localScale = Vector2.one * 0.8f;
                                AnimateMoveChipBackward(chip, playerView.GetAvatarTransform().position, () =>
                                {
                                    boxbet.AnimateFlyMoney(data.FirstHandWinAmount);
                                    playerView.SetCurrentChip(playerWallet[kvp.Key]);
                                });
                            });
                        }
                        else if (data.IsWinFirstHand == 0)
                        {
                            boxbet.HideImageChip();
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            int chipIndex = GetChipIndex((int)data.FirstHandWinAmount);
                            chip.transform.SetParent(boxbet.GetCardPosition, true);
                            // chip.transform.localPosition = boxbet.ChipPosition.position;
                            chip.SetInfo(chipIndex, boxbet.ChipPosition.localPosition);
                            chip.transform.localScale = Vector2.one * 0.8f;
                            AnimateMoveChipBackward(chip, playerView.GetAvatarTransform().position, () =>
                            {
                                boxbet.AnimateFlyMoney(data.FirstHandBetAmount);
                                playerView.SetCurrentChip(playerWallet[kvp.Key]);
                            });
                        }
                        else
                        {
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            int chipIndex = GetChipIndex((int)data.FirstHandBetAmount);
                            chip.transform.SetParent(boxbet.GetCardPosition, true);
                            chip.SetInfo(chipIndex, boxbet.ChipPosition.localPosition);
                            AnimateMoveChipBackward(chip, bankerBoxBet.GetCardPosition.position, null, 0.5f);
                            boxbet.HideImageChip();
                        }
                        
                        // Return chip second hand
                        if (data.IsWinSecondHand > 0)
                        {
                            boxbet.SecondBoxBet.HideImageChip();
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            int chipIndex = GetChipIndex((int)data.SecondHandBetAmount);
                            chip.transform.SetParent(boxbet.SecondBoxBet.GetCardPosition, true);
                            chip.SetInfo(chipIndex, boxbet.SecondBoxBet.ChipPosition.position);
                            chip.transform.localScale = Vector2.one * 0.8f;
                            AnimateMoveChipBackward(chip, playerView.GetAvatarTransform().position, () =>
                            {
                                boxbet.SecondBoxBet.HideImageChipWin();
                                BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                                int chipIndex = GetChipIndex((int)data.SecondHandWinAmount);
                                chip.transform.SetParent(boxbet.SecondBoxBet.GetCardPosition.parent, true);
                                chip.SetInfo(chipIndex, boxbet.SecondBoxBet.ChipWinPosition.localPosition);
                                chip.transform.localScale = Vector2.one * 0.8f;
                                AnimateMoveChipBackward(chip, playerView.GetAvatarTransform().position, () =>
                                {
                                    boxbet.SecondBoxBet.AnimateFlyMoney(data.SecondHandWinAmount);
                                    playerView.SetCurrentChip(playerWallet[kvp.Key]);
                                });
                            });
                        }
                        else if (data.IsWinSecondHand == 0)
                        {
                            boxbet.SecondBoxBet.HideImageChip();
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            int chipIndex = GetChipIndex((int)data.SecondHandWinAmount);
                            chip.transform.SetParent(boxbet.SecondBoxBet.GetCardPosition.parent, true);
                            chip.SetInfo(chipIndex, boxbet.SecondBoxBet.ChipPosition.localPosition);
                            chip.transform.localScale = Vector2.one * 0.8f;
                            AnimateMoveChipBackward(chip, playerView.GetAvatarTransform().position, () =>
                            {
                                boxbet.SecondBoxBet.AnimateFlyMoney(data.SecondHandBetAmount);
                                playerView.SetCurrentChip(playerWallet[kvp.Key]);
                            });
                        }
                        else
                        {
                            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
                            int chipIndex = GetChipIndex((int)data.SecondHandBetAmount);
                            chip.transform.SetParent(boxbet.SecondBoxBet.GetCardPosition, true);
                            chip.SetInfo(chipIndex, boxbet.SecondBoxBet.ChipPosition.localPosition);
                            AnimateMoveChipBackward(chip, bankerBoxBet.GetCardPosition.position, null, 0.5f);
                            boxbet.SecondBoxBet.HideImageChip();
                        }
                    }
                }

            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                AnimateReturnAllCards();
            });
    }

    private void ShowAllPlayersLoading()
    {
        foreach (Player player in rearrangedPlayers)
        {
            if (player.Id == currentPlayerView.id) continue;
            if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
            {
                if (!boxBet.HasBet)
                {
                    boxBet.ShowAnimationWaiting();
                }
            }
        }
    }
    private void HideAllPlayersLoading()
    {
        foreach (Player player in rearrangedPlayers)
        {
            if (player.Id == currentPlayerView.id) continue;
            if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
            {
                boxBet.HideAnimationWaiting();
            }
        }
    }

    public void RotateImageLight(string currentPlayerId)
    {
        imageLight.gameObject.SetActive(true);
        int playerIndex = GetPlayerIndexById(currentPlayerId);
        Debug.Log("playerIndex:" + playerIndex);
        float angle = listLightAngle[playerIndex];
        imageLight.transform.DORotate(new Vector3(0, 0, angle), 1f);
    }

    private void ShrinkCurrentPlayerBoxbet()
{
        if (currentPlayerBoxBet.IsEnlarging)
        {
            currentPlayerBoxBet.ShrinkCards();
        }
        else if (currentPlayerBoxBet.SecondBoxBet.IsEnlarging)
        {
            currentPlayerBoxBet.SecondBoxBet.ShrinkCards();
        }
    }
    #endregion

    #region Helpers
    private int GetChipIndex(long value)
    {
        for (int i = 0; i < listValueChipBets.Count - 1; i++)
        {
            if (value < listValueChipBets[i + 1])
            {
                return i;
            }
        }
        return 4;
    }

    private void HideAllImageChip()
    {
        foreach (var boxBet in userIdToBoxBetView.Values)
        {
            boxBet.HideImageChip();
        }
    }

    private void SetEnableRebetButton(bool isEnable)
    {
        if (isEnable)
        {
            buttonRebet.interactable = true;
            imageButtonRebet.color = Color.white;
        }
        else
        {
            buttonRebet.interactable = false;
            imageButtonRebet.color = Color.gray;
        }
    }

    private void SetEnableDoubleBetButton(bool isEnable)
    {
        if (isEnable)
        {
            buttonDoubleBet.interactable = true;
            imageButtonDoubleBet.color = Color.white;
        }
        else
        {
            buttonDoubleBet.interactable = false;
            imageButtonDoubleBet.color = Color.gray;
        }
    }

    private void CheckCurrentPlayerFinish(BlackjackUpdateDeal data)
    {
        var finalHandTypes = new HashSet<BlackjackHandType>
        {
            BlackjackHandType.Blackjack,
            BlackjackHandType.Busted,
            BlackjackHandType._21P
        };
        bool isFirstHandFinal = finalHandTypes.Contains(data.Hand.First.Type);
        bool isSecondHandOk = data.Hand.Second.Type == BlackjackHandType.Unspecified || finalHandTypes.Contains(data.Hand.Second.Type);
        if (data.UserId == User.userProfile.UserId && isFirstHandFinal && isSecondHandOk)
        {
            isCurrentPlayerFinished = true;
            buttonDouble.gameObject.SetActive(false);
            buttonSplit.gameObject.SetActive(false);
            buttonHit.gameObject.SetActive(false);
            buttonStand.gameObject.SetActive(false);
        }
    }

    private int GetPlayerIndexById(string userId)
    {
        Debug.Log("rearrangedPlayers.count:" + rearrangedPlayers.Count);
        for (int i = 0; i < rearrangedPlayers.Count; i++)
        {
            if (rearrangedPlayers[i].Id == userId)
            {
                Debug.Log("GetPlayerIndexById: " + i);
                return i;
            }
        }
        return -1;
    }

    private void ShowNotEnoughChipDialog()
    {
        UIManager.Instance.ShowAlertDialog("You do not have enough chip!");
    }
    #endregion

    #region Setups

    protected override void CreatePlayerView(Player player, Vector2 anchoredPos)
    {
        base.CreatePlayerView(player, anchoredPos);
        string localUserId = User.userProfile.UserId;
        if (GameState == GameState.Play)
        {
            int playerIndex = players.IndexOf(player);
            if (!userIdToBoxBetView.TryGetValue(player.Id, out var boxBetView) || boxBetView == null)
            {
                boxBetView = Instantiate(boxBetPrefab, boxBetContainer).GetComponent<BlackjackBoxBet>();
                userIdToBoxBetView[player.Id] = boxBetView;
                boxBetView.transform.localPosition = listBoxBetPosition[playerIndex];
                boxBetView.gameObject.SetActive(true);
                boxBetView.SetInfo(playerIndex, listBoxBetPosition[playerIndex]);
            }
        }
        else
        {
            int playerIndex = rearrangedPlayers.IndexOf(player);
            if (!userIdToBoxBetView.TryGetValue(player.Id, out var boxBetView) || boxBetView == null)
            {
                boxBetView = Instantiate(boxBetPrefab, boxBetContainer).GetComponent<BlackjackBoxBet>();
                userIdToBoxBetView[player.Id] = boxBetView;
                boxBetView.transform.localPosition = listBoxBetPosition[playerIndex];
                boxBetView.gameObject.SetActive(true);
                boxBetView.SetInfo(playerIndex, listBoxBetPosition[playerIndex]);
                // boxBetView.SetInfo(this, 1);
            }
            else
            {
                boxBetView.transform.localPosition = listBoxBetPosition[rearrangedPlayers.IndexOf(player)];
                boxBetView.SetInfo(playerIndex, listBoxBetPosition[playerIndex]);

            }
            // boxBetView.ShowSecondBox();

        }
        if (userIdToBoxBetView.TryGetValue(localUserId, out BlackjackBoxBet boxBet))
        {
            currentPlayerBoxBet = boxBet;
        }
    }

    public CardModel InitCard()
    {
        CardModel cardModel = PoolService.Instance.Get<CardModel>(PrefabType.Card);
        cardModel.transform.SetParent(dealCardContainer);
        cardModel.transform.localPosition = Vector3.zero;
        cardModel.transform.localScale = Vector3.zero;
        cardModel.gameObject.SetActive(true);
        DOTween.Kill(cardModel.transform);
        return cardModel;
    }

    private void SetInfoBet()
    {
        listValueChipBets = new List<long> { MarkUnit, MarkUnit * 5, MarkUnit * 10, MarkUnit * 50, MarkUnit * 100 };
        for (int i = 0; i < 5; i++)
        {
            listChipBet[i].textValue.text = Utility.FormatMoney(listValueChipBets[i], true);
            listChipBet[i].value = listValueChipBets[i];
            listChipBet[i].SetEnable(playerWallet >= listValueChipBets[i]);
        }
    }

    private void UpdateChipBetInteractivity()
    {
        for (int i = 0; i < 5; i++)
        {
            listChipBet[i].SetEnable(playerWallet >= listValueChipBets[i]);
        }
        SetEnableDoubleBetButton(playerWallet >= totalBetValue && totalBetValue > 0);
        SetEnableRebetButton(playerWallet >= lastBetValue && lastBetValue > 0 && !hasBet);

    }

    private void InitPool()
    {
        PoolService.Instance.Register(PrefabType.Card, cardContainer, cardPrefab, 20, 50, 30);
        PoolService.Instance.Register(PrefabType.ChipPlayerBlackjack, chipContainer, chipPrefab, 20, 30, 15);
        // cardPool = new UnityEngine.Pool.ObjectPool<CardModel>(
        //     createFunc: () =>
        //     {
        //         var card = Instantiate(cardPrefab, cardContainer).GetComponent<CardModel>();
        //         card.gameObject.SetActive(false);
        //         return card;
        //     },
        //     actionOnGet: (card) =>
        //     {
        //         if (card == null) return;
        //         card.gameObject.SetActive(true);
        //         card.transform.localScale = Vector3.one;
        //         card.HideSparkleAnimation();
        //         card.HideShadowCard();
        //     },
        //     actionOnRelease: (card) =>
        //     {
        //         card.gameObject.SetActive(false);
        //     },
        //     actionOnDestroy: (card) =>
        //     {
        //         Destroy(card.gameObject);
        //     },
        //     collectionCheck: true,
        //     defaultCapacity: 20,
        //     maxSize: 50
        // );
        // chipPool = new UnityEngine.Pool.ObjectPool<BlackjackChip>(
        //     createFunc: () =>
        //     {
        //         var chip = Instantiate(chipPrefab, chipContainer).GetComponent<BlackjackChip>();
        //         chip.gameObject.SetActive(false);
        //         return chip;
        //     },
        //     actionOnGet: (chip) =>
        //     {
        //         if (chip == null) return;
        //         chip.gameObject.SetActive(true);
        //         chip.transform.localScale = Vector3.one;
        //     },
        //     actionOnRelease: (chip) =>
        //     {
        //         DOTween.Kill(chip.transform);
        //         chip.gameObject.SetActive(false);
        //     },
        //     actionOnDestroy: (chip) =>
        //     {
        //         Destroy(chip.gameObject);
        //     },
        //     collectionCheck: true,
        //     defaultCapacity: 20,
        //     maxSize: 50
        // );
    }

    private void ResetGame(bool isRejoin = false)
    {
        Debug.Log("RESET GAME");
        lastPlayerTurnId = "";
        currentPlayerTurnId = "";
        currentBetValue = playerReceiveCardCount = 0;
        currentChipIndex = -1;
        hasDealtCardsForBanker = hasDealtCardsForPlayers = isCurrentPlayerFinished = isRebet = isSplitingHand = isCurrentPlayerTurnPassed = isRejoinTable = isClickDoubleBet = false;
        hasBet = false;
        listBankerCard.Clear();

        buttonBetContainer.gameObject.SetActive(false);
        boxBetContainer.gameObject.SetActive(true);
        buttonClear.SetActive(false);
        buttonDeal.SetActive(false);
        // buttonDoubleBet.SetActive(false);
        // buttonRebet.SetActive(lastBetValue > 0);
        SetEnableDoubleBetButton(false);
        SetEnableRebetButton(lastBetValue > 0 && playerWallet > lastBetValue);

        bankerBoxBet.Reset();
        foreach (Player player in rearrangedPlayers)
        {
            if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
            {
                boxBet.Reset(isRejoin);
                boxBet.SecondBoxBet.Reset(isRejoin);
            }
        }

        foreach (Transform child in chipContainer)
        {
            if (child.TryGetComponent<BlackjackChip>(out var chip))
            {
                if (chip.gameObject.activeSelf)
                {
                    PoolService.Instance.Release(PrefabType.ChipPlayerBlackjack, chip);
                }
            }
        }

        userIdToInsuranceChip.Clear();


        // foreach (Transform child in chipContainer)
        // {
        //     if (child.gameObject.activeSelf)
        //     {
        //         chipPool.Release(child.GetComponent<BlackjackChip>());
        //     }
        // }
    }

    protected override void RemovePlayerBoxBet(string leavePlayerId)
    {
        base.RemovePlayerBoxBet(leavePlayerId);
        if (userIdToBoxBetView.TryGetValue(leavePlayerId, out var view))
        {
            Destroy(view.gameObject);
            userIdToBoxBetView.Remove(leavePlayerId);
        }
    }

    #endregion

    private struct BalanceResultData
    {
        public long FirstHandBetAmount;
        public long SecondHandBetAmount;
        public long FirstHandWinAmount;
        public long SecondHandWinAmount;
        public int IsWinFirstHand;
        public int IsWinSecondHand;

        public BalanceResultData(long firstHandBetAmount, long secondHandBetAmount, long firstHandWinAmount, long secondHandWinAmount, int isWinFirstHand, int isWinSecondHand)
        {
            FirstHandBetAmount = firstHandBetAmount;
            SecondHandBetAmount = secondHandBetAmount;
            FirstHandWinAmount = firstHandWinAmount;
            SecondHandWinAmount = secondHandWinAmount;
            IsWinFirstHand = isWinFirstHand;
            IsWinSecondHand = isWinSecondHand;
        }
    }
}

