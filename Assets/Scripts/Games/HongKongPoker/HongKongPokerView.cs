using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Games.Card;
using Globals;
using Nakama;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using GameState = Proto.GameState;

public class HongKongPokerView : BaseDiceGameView
{
    public enum BetStatus
    {
        ALL_IN = 1,
        RAISE = 2,
        CALL = 3,
        CHECK = 4,
        FOLD = 5 
    }
    [SerializeField] private Transform cardContainer, boxBetContainer, arrowSwapContainer, buttonChangeCardContainer, chipContainer, dealer;
    // [SerializeField] private PlayerViewHongKongPoker dealerHkPoker;
    [SerializeField] private HongKongPokerPot pot;
    [SerializeField] private HongKongPokerToggleBetContainer toggleContainer;
    [SerializeField] private HongKongPokerButtonBetContainer buttonBetContainer;
    [SerializeField] private HongKongPokerChip chipPrefab;
    [SerializeField] private HongKongPokerBoxBet boxBetPrefab;
    [SerializeField] private SkeletonGraphic animationStart;
    [SerializeField] private Sprite spriteFrameMask;
    [SerializeField] private Sprite[] listImageWinLose;
    [SerializeField] private TextMeshProUGUI textTipChip, textThanks, textCountDown;
    [SerializeField] private GameObject cardPrefab;
    private List<HongKongPokerBoxBet> listBoxBet = new() { null, null, null, null, null };
    private List<List<CardModel>> listPlayerCards = new()
    {
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>()
    };

    private Vector2[] listCardPosition = new Vector2[]
    {
        new(-12, -207),
        new(-404, -49),
        new(-357, 168),
        new(350, 168),
        new(403, -60)
    };

    private Vector2[] listBoxBetPosition = new Vector2[]
    {
        new(10, -105),
        new(-352, -126),
        new(-309, 93),
        new(306, 72),
        new(395, -144)
    };

    private UnityEngine.Pool.ObjectPool<HongKongPokerBoxBet> boxBetPool;
    private UnityEngine.Pool.ObjectPool<HongKongPokerChip> chipPool;
    private UnityEngine.Pool.ObjectPool<CardModel> cardPool;
    private GameState gameState = GameState.Preparing;

    protected override void Awake()
    {
        base.Awake();
        InitPool();
        InitPlayers();
        InitPlayerCards();
        buttonBetContainer.SetValues(10000, 2000, 4000);
    }

    #region API Handlers

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        Debug.Log("Match: " + match);

        Card card1 = new()
        {
            Rank = CardRank.RankJ,
            Suit = CardSuit.SuitHearts

        };
        DOTween.Sequence()
            .AppendCallback(() => DealACard(card1, 0))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 0))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 0))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 1))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 1))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 2))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 2))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 3))
            .AppendInterval(0.25f)
            .AppendCallback(() => DealACard(card1, 3))
            .AppendInterval(0.25f)
            .AppendCallback(() =>
            {
                for (int i = 0; i < listPlayerCards[0].Count; i++)
                {
                    // player.vectorCardP1[i].setTextureWithCode(0);
                    // player.vectorCardP1[i].setDark(true, this.spriteFrameMask);
                    StartCoroutine(FoldDown(0, listPlayerCards[0][i], i * 0.1f));
                }
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                ShowBoxBet(BetStatus.CALL, 0, 100);
                PlayerGiveChipsToBoxbet(0);
                FoldUp(0, listPlayerCards[0][^1], card1);
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                BoxBetGiveChipsToDealer(0);
                ReturnAllCardsToDealer();
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                DealerGiveChipsToPlayer();
                // listPlayerView[0].effectFlyMoney(100000);
                // listPlayerView[0].setEffectWin();
                pot.SetValue(4000);
            });
    
    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        // UpdateListPlayer(updateTable.Players.ToList());
    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        UpdateTable data = UpdateTable.Parser.ParseFrom(matchState.State);

        Debug.Log("Update Table: " + data);
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
        var data = UpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Deal: " + data);
   

    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
        var data = UpdateGameState.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Game State: " + data);
        gameState = data.State;
        switch (gameState)
        {
            case GameState.Preparing:
                textCountDown.gameObject.SetActive(true);
                textCountDown.text = data.CountDown.ToString();
                break;
            case GameState.Play:
                HandleStartGame();
                break;
            case GameState.Matching:

                break;
            case GameState.Idle:
                break;
            case GameState.Reward:

                break;
            case GameState.Finish:
                break;
            default:
                break;
        }
    }

    public override void HandleUpdateCardState(IMatchState matchState)
    {
        base.HandleUpdateCardState(matchState);
        var data = UpdateCardState.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Card State: " + data);

    }

    public override void HandleUpdateTurn(IMatchState matchState)
    {
        base.HandleUpdateTurn(matchState);
        var data = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Turn: " + data);
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var data = UpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Finish: " + data);
    }
    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var data = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Wallet: " + data);
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        Destroy(gameObject);
    }

    #endregion

    #region Button
    public void OnClickSwap()
    {

    }

    public void OnClickCancel()
    {

    }

    public void OnClickSendTip()
    {

    }

    #endregion

    #region Card Actions
    private void DealACard(Card pokerCard, int playerIndex, float delay = 0f, bool isAnimate = true)
    {
        List<CardModel> playerCards = listPlayerCards[playerIndex];
        Vector2 basePosition = listCardPosition[playerIndex];
        Vector2 targetPosition;

        bool isLeftTable = !(basePosition.x > 0);

        CardModel card = cardPool.Get();
        card.HideCardPusoy();

        if (!isLeftTable)
        {
            card.transform.SetSiblingIndex(20);
        }

        // Xác định vị trí đích của lá bài
        float cardWidth = card.GetComponent<RectTransform>().rect.width;
        float offset = cardWidth / 2 * 0.38f * playerCards.Count;

        if (isLeftTable)
        {
            targetPosition = new Vector2(basePosition.x + offset, basePosition.y);
            if (playerCards.Count > 0)
            {
                card.transform.SetSiblingIndex(playerCards[^1].transform.GetSiblingIndex() + 1);
            }
        }
        else
        {
            targetPosition = new Vector2(basePosition.x - offset, basePosition.y);
            if (playerCards.Count > 0)
            {
                card.transform.SetSiblingIndex(playerCards[^1].transform.GetSiblingIndex() - 1);
            }
        }

        // Bài di chuyển từ dealer về tay player
        if (isAnimate)
        {
            if (pokerCard.Rank != CardRank.RankUnspecified && pokerCard.Suit != CardSuit.SuitUnspecified)
            {
                // SoundManager.Instance.PlaySound("sound_cardFlipBJ");
                // playSound(SOUND_GAME.CARD_FLIP_2);
                card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
                card.ShowCardPusoy();
            }

            card.transform.localRotation = Quaternion.Euler(0, 0, -90);
            card.transform.localScale = new Vector3(0, 0.45f, 1);

            card.transform
                .DOLocalMove(targetPosition, 0.7f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
            card.transform
                .DOLocalRotate(Vector3.zero, 0.6f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
            card.transform
                .DOScale(new Vector3(0.45f, 0.45f, 1), 0.6f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
        }
        else
        {
            card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
            card.transform.localPosition = targetPosition;
        }
        playerCards.Add(card);
    }

    private void ReturnAllCardsToDealer()
    {
        int zIndex = 0;
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < listPlayerCards.Count; i++)
        {
            List<CardModel> currentPlayerCards = listPlayerCards[i];
            for (int j = currentPlayerCards.Count - 1; j >= 0; j--)
            {
                CardModel card = currentPlayerCards[j];
                currentPlayerCards.RemoveAt(j);
                sequence
                    .AppendCallback(() =>
                    {
                        ReturnACard(card, zIndex);
                    })
                    .AppendInterval(0.1f);
                zIndex++;
            }
        }
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                foreach (Transform child in cardContainer.transform)
                {
                    child.gameObject.SetActive(false);
                }
            });
        // await UniTask.Delay((delay + 700) / 1000);
    }

    private void ReturnACard(CardModel card, int zIndex)
    {
        card.transform.SetSiblingIndex(50 + zIndex);
        card.transform.DOMove(new Vector2(dealer.transform.position.x, dealer.transform.position.y - 40), 0.4f)
            .SetEase(Ease.OutCubic);
        card.transform.DOScale(new Vector3(0.04f, 0.4f, 1f), 0.15f)
            .OnComplete(() => card.transform.DOScale(new Vector3(0.4f, 0.4f, 1f), 0.15f));
        card.transform.DOLocalRotate(new Vector3(0, -15, 0), 0.15f)
            .OnComplete(() =>
            {
                card.HideCardPusoy();
                card.transform.localRotation = Quaternion.Euler(0, 15, 0);
                card.transform.DOLocalRotate(Vector3.zero, 0.15f);
            });


        // await UniTask.Delay(TimeSpan.FromSeconds(0.6f));
    }
    #endregion

    #region Boxbet Actions
    private void ShowBoxBet(BetStatus status, int index, int chip)
    {
        HongKongPokerBoxBet boxbet = boxBetPool.Get();
        listBoxBet[index] = boxbet;
        boxbet.transform.localPosition = listBoxBetPosition[index];
        boxbet.SetInfo(status, index, chip);
    }
    #endregion

    #region Chip Actions
    private void PlayerGiveChipsToBoxbet(int index)
    {
        int valueBoxBet = listBoxBet[index].Chip;
        // if ((int)data["chipStack"] - preNextStack != 0)
        // {
        //     currentPlayer.playerView.effectFlyMoney(-valueBoxBet);
        // }

        int numberChip = 0;
        if (valueBoxBet > 0)
        {
            numberChip = Mathf.Clamp((int)Mathf.Ceil((float)valueBoxBet / MarkUnit), 1, 4);
        }

        Vector2 targetPosition = listBoxBetPosition[index];
        Vector2 startPosition = listCardPosition[index];

        for (int i = 0; i < numberChip; i++)
        {
            HongKongPokerChip chip = chipPool.Get();
            chip.transform.localScale = Vector3.one * 0.4f;
            chip.transform.localPosition = startPosition;

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(i * 0.1f).AppendCallback(() => chip.MoveToBoxBet(targetPosition));
                
        }
    }

    private void BoxBetGiveChipsToDealer(int index)
    {
        if (listBoxBet[index].Chip > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                HongKongPokerChip chip = chipPool.Get();
                chip.transform.localScale = Vector3.one * 0.4f;
                chip.transform.localPosition = listBoxBetPosition[index];
                Vector3 targetPos = pot.transform.localPosition + new Vector3(
                    Random.Range(-7f, 7f),
                    -40 + Random.Range(-7f, 7f),
                    0
                );
                Vector3 potPosition = pot.transform.position;
                chip.MoveToPot(targetPos, dealer.transform.position);
            }
        }
    }

    private void DealerGiveChipsToPlayer()
    {
        HongKongPokerChip chip = chipPool.Get();
        chip.transform.position = pot.transform.position;

        Vector2 vtemp = new Vector2(0, 115);
        chip.MoveToPlayer(vtemp, listCardPosition[0], 3000);
    }
    #endregion

    #region Animation
    private IEnumerator AnimateArrowSwapLoop()
    {
        for (int i = 0; i < 10; i++)
        {
            if (arrowSwapContainer == null) yield break;

            Vector2 startPos = new(0, 0);
            Vector2 endPos = new(0, -25);

            yield return arrowSwapContainer.transform.DOLocalMove(startPos, 0.4f).SetEase(Ease.OutCubic).WaitForCompletion();

            yield return new WaitForSeconds(0.05f);

            yield return arrowSwapContainer.transform.DOLocalMove(endPos, 0.15f).SetEase(Ease.OutCubic).WaitForCompletion();

        }
    }

    private IEnumerator FoldDown(int index, CardModel card, float delay)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;
        yield return new WaitForSeconds(delay);

        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.55f, 1f), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    card.HideCardPusoy();
                    card.SetDark(true);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.15f).SetEase(Ease.OutCubic));

        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutCubic));
    }

    private void FoldUp(int index, CardModel card, Card pokerCard)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;

        // Scale animation
        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.65f, 1f), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    card.ShowCardPusoy();
                    card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.2f).SetEase(Ease.OutCubic));

        // Skew animation (simulated via rotation)
        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
    }
    #endregion

    private void HandleStartGame()
    {
        Debug.Log("handle start game");
        textCountDown.gameObject.SetActive(false);
        Utility.PlayAnimation(animationStart, "startgame", false);
    }

    private void InitPool()
    {
        chipPool = new UnityEngine.Pool.ObjectPool<HongKongPokerChip>(
            createFunc: () =>
            {
                var chip = Instantiate(chipPrefab, chipContainer);
                chip.gameObject.SetActive(false); // bắt đầu ẩn
                return chip.GetComponent<HongKongPokerChip>();
            },
            actionOnGet: (chip) =>
            {
                chip.gameObject.SetActive(true);
                chip.transform.localScale = Vector3.one;
            },
            actionOnRelease: (chip) =>
            {
                chip.gameObject.SetActive(false);
            },
            actionOnDestroy: (chip) =>
            {
                Destroy(chip);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
        boxBetPool = new UnityEngine.Pool.ObjectPool<HongKongPokerBoxBet>(
            createFunc: () =>
            {
                var boxBet = Instantiate(boxBetPrefab, boxBetContainer);
                boxBet.gameObject.SetActive(false); // bắt đầu ẩn
                return boxBet.GetComponent<HongKongPokerBoxBet>();
            },
            actionOnGet: (boxBet) =>
            {
                boxBet.gameObject.SetActive(true);
                boxBet.transform.localScale = Vector3.one;
            },
            actionOnRelease: (boxBet) =>
            {
                boxBet.gameObject.SetActive(false);
            },
            actionOnDestroy: (boxBet) =>
            {
                Destroy(boxBet);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
        cardPool = new UnityEngine.Pool.ObjectPool<CardModel>(
            createFunc: () =>
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.SetActive(false); // bắt đầu ẩn
                return card.GetComponent<CardModel>();
            },
            actionOnGet: (card) =>
            {
                card.gameObject.SetActive(true);
                card.transform.localScale = new Vector3(0.45f, 0.45f, 1);
                card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            },
            actionOnRelease: (card) =>
            {
                card.gameObject.SetActive(false);
            },
            actionOnDestroy: (card) =>
            {
                Destroy(card);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
    }

    private void InitPlayers()
    {

    }

    private void InitPlayerCards()
    {
        // CardModel card1 = new();
        // card1.SetData(1, 1);
        // CardModel card2 = new();
        // card1.SetData(2, 2);
        // CardModel card3 = new();
        // card1.SetData(3, 3);
        // CardModel card4 = new();
        // card1.SetData(4, 4);
        // List<CardModel> cardModels = new() {card1, card2, card3, card4};
        // listPlayerCards[0] = cardModels;

    }
}
