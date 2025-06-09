using System;
using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotGame : MonoBehaviour
{
    public event Action<OnCardPlayedEventArg> OnCardPlayed;
    public class OnCardPlayedEventArg : EventArgs
    {
        public bool isCurrentPlayer;
        public WhotCard lastCard;
    }
    [SerializeField] private GameObject whotPlayerPrefab, cardPrefab;
    [SerializeField] private List<Transform> playerPositionsList;
    [SerializeField] private SkeletonGraphic betterLuckNextTimeAnimation, victoryAnimation, matchSymbolAnimation, effectAnimation, lastCardAnimation;
    [SerializeField] private Transform matchResultTransform, yourTurnTransform, wheelTransform, effectAnimationParent,
    victoryAnimationParent, loseAnimationParent, matchSymbolAnimationParent, lastCardAnimationParent, deckOfCardParent,
    lastCardParent, playAreaParent;
    [SerializeField] private TextMeshProUGUI betText, cardsLeftText, betterLuckNextTimeText;
    [SerializeField] private Image waitImage, victoryImage;
    [SerializeField] WhotSuitPicker suitPicker;
    public WhotCard LastCard { get; private set; }
    private List<WhotPlayer> playersList;
    private List<WhotCard> mockCardsList;

    private WhotPlayerHand playerHand;
    private Tween waitRotationTween;
    private const float ANIMATION_TIME = 0.5f;
    private const float ANIMATION_WAIT_ROTATION_SPEED = 270f;
    private const string
        MATCH_SYMBOL_SQUARE_ANIMATION_NAME = "vuong",
        MATCH_SYMBOL_CROSS_ANIMATION_NAME = "thap",
        MATCH_SYMBOL_TRIANGLE_ANIMATION_NAME = "tamgiac",
        MATCH_SYMBOL_CIRCLE_ANIMATION_NAME = "tron",
        MATCH_SYMBOL_STAR_ANIMATION_NAME = "sao",
        EFFECT_HOLD_ON_ANIMATION_NAME = "hold",
        EFFECT_GENERAL_MARKET_ANIMATION_NAME = "general",
        LAST_CARD_EFFECT_ANIMATION_NAME = "1",
        LOSE_ANIMATION_NAME = "better",
        VICTORY_MARKET_ANIMATION_NAME = "victory";
        

    private void Awake()
    {
        playersList = new();
        mockCardsList = new();

        HandleResetGame();
    }

    public void HandleResetGame()
    {
        DOTween.KillAll(true);
        playerHand = GetComponent<WhotPlayerHand>();
        playerHand.Reset();
        playersList.Clear();
        mockCardsList.Clear();
        suitPicker.OnSuitPicked += WhotSuitPicker_OnSuitPicked;
        playAreaParent.gameObject.SetActive(true);
        suitPicker.gameObject.SetActive(false);
        LastCard = null;
        foreach (Transform child in lastCardParent)
        {
            Destroy(child.gameObject);
        }
        PrepareMockData();
        InitPlayer();
        StartCoroutine(DealCards());
    }

    public void OnDrawCard()
    {
        DrawACard(playerHand.cardsInHand, playerHand.GetCardsParent(), true);
    }

    public IEnumerator DealCards()
    {
        int length = playersList.Count;
        for (int i = 0; i < length * 6; i++)
        {
            int index = i % length;
            WhotPlayer player = playersList[index];
            yield return new WaitForSeconds(0.1f);

            GameObject card = Instantiate(cardPrefab, deckOfCardParent);
            WhotCard whotCard = card.GetComponent<WhotCard>();
            whotCard.SetFaceDown();
            if (player.isCurrentPlayer)
            {
                whotCard.transform.SetParent(playerHand.GetCardsParent());
                AnimateDealACard(whotCard, playerHand.GetCardsParent(), player, i / length);
            }
            else
            {
                AnimateDealACard(whotCard, player.GetDealedCardParent(), player, i / length);
            }
        }
    }

    public void DrawACard(List<WhotCard> cardsList, Transform targetPosition, bool isCurrentPlayer)
    {
        GameObject card = Instantiate(cardPrefab, deckOfCardParent);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        card.transform.SetParent(targetPosition);
        WhotCard whotCard = card.GetComponent<WhotCard>();
        whotCard.SetInfo(CardSuit.SuitCross, CardRank.Rank1);
        whotCard.SetFaceDown();

        if (isCurrentPlayer)
        {
            whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
            AnimateCurrentPlayerDrawACard(whotCard);
        }
        else
        {
            AnimateOtherPlayerDrawACard(whotCard, targetPosition);   
        }
        cardsList.Add(whotCard);
    }

    public void PlayACard(WhotCard card, Transform startingPosition, bool isCurrentPlayer)
    {
        GameObject cardInstance = Instantiate(cardPrefab, startingPosition);
        cardInstance.transform.localPosition = Vector3.zero;
        cardInstance.transform.localScale = Vector3.one;
        cardInstance.transform.SetParent(lastCardParent);
        WhotCard whotCard = cardInstance.GetComponent<WhotCard>();
        whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        whotCard.SetSelectable(false);
        LastCard = whotCard;
        AnimatePlayACard(whotCard);
        HandleCardEffect(whotCard);

        WhotPlayer whotPlayer = playersList.Find(player => player.isCurrentPlayer);
        if (isCurrentPlayer)
        {
            whotPlayer.cards.Remove(card);
            whotPlayer.UpdateCardsLeftVisual();

            if (whotPlayer.cards.Count == 0)
            {
                whotPlayer.isWinner = true;
                
            }
        }

        OnCardPlayed?.Invoke(new OnCardPlayedEventArg
        {
            isCurrentPlayer = isCurrentPlayer,
            lastCard = LastCard
        });
    }

    private void HandleCardEffect(WhotCard card)
    {
        switch (card.GetCardRank())
        {
            case CardRank.Rank20:
                AnimateWait();
                AnimateOpenSuitPickerWheel();
                break;
            case CardRank.Rank1:
                HandleHoldOnEffect();
                break;
            case CardRank.Rank2:
                HandlePick2Effect();
                break;
            case CardRank.Rank5:
                HandlePick3Effect();
                break;
            case CardRank.Rank8:
                HandleSuspensionEffect();
                break;
            case CardRank.Rank14:
                HandleGeneralMarketEffect();
                break;
            default:
                Debug.LogWarning($"Unhandled card effect for rank: {card.GetCardRank()}");
                break;
        }
    }
    #region Card Effect
    private void HandleHoldOnEffect()
    {
        foreach (WhotPlayer player in playersList)
        {
            if (!player.isCurrentPlayer)
            {
                player.AnimateShowHoldOn();
                player.UpdateEffectNoti("Hold On");
            }
        }
        AnimateHoldOn();
    }

    private void HandlePick2Effect()
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.isCurrentPlayer)
            {
                continue;
            }
            else
            {
                player.DrawACard();
                player.DrawACard();
                player.UpdateEffectNoti("Pick 2");
            }
        }
    }

    private void HandlePick3Effect()
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.isCurrentPlayer)
            {
                continue;
            }
            else
            {
                player.DrawACard();
                player.DrawACard();
                player.DrawACard();
                player.UpdateEffectNoti("Pick 3");
            }
        }
    }

    private void HandleSuspensionEffect()
    {
        foreach (WhotPlayer player in playersList)
        {
            if (!player.isCurrentPlayer)
            {
                player.AnimateShowSuspension();
                player.UpdateEffectNoti("Suspension");
            }
        }
    }
    private void HandleGeneralMarketEffect()
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.isCurrentPlayer)
            {
                continue;
            }
            else
            {
                player.DrawACard();
            }
        }
        AnimateGeneralMarket();
    }
    #endregion

    #region Animations
    private void AnimateGeneralMarket()
    {
        effectAnimationParent.gameObject.SetActive(true);
        Utility.PlayAnimation(effectAnimation, EFFECT_GENERAL_MARKET_ANIMATION_NAME, false);
        effectAnimation.AnimationState.Complete += delegate
        {
            effectAnimationParent.gameObject.SetActive(false);
        };
    }

    private void AnimateHoldOn()
    {
        effectAnimationParent.gameObject.SetActive(true);
        Utility.PlayAnimation(effectAnimation, EFFECT_HOLD_ON_ANIMATION_NAME, false);
        effectAnimation.AnimationState.Complete += delegate
        {
            effectAnimationParent.gameObject.SetActive(false);
        };
    }

    private void AnimateWait()
    {
        waitImage.gameObject.SetActive(true);
        float duration = 360f / ANIMATION_WAIT_ROTATION_SPEED; 

        waitRotationTween = waitImage.transform
            .DORotate(new Vector3(0, 0, -360), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    public void StopWaitAnimation()
    {
        if (waitRotationTween != null && waitRotationTween.IsActive())
        {
            waitRotationTween.Kill();
            waitImage.gameObject.SetActive(false);
        }
    }

    public void AnimateLastCard()
    {
        lastCardAnimationParent.gameObject.SetActive(true);
        Utility.PlayAnimation(lastCardAnimation, LAST_CARD_EFFECT_ANIMATION_NAME, false);
        lastCardAnimation.AnimationState.Complete += delegate
        {
            lastCardAnimationParent.gameObject.SetActive(false);
        };
    }

    private void AnimateMatchSymbol(CardSuit suit)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                matchSymbolAnimationParent.gameObject.SetActive(true);

                string animationName = suit switch
                {
                    CardSuit.SuitSquare => MATCH_SYMBOL_SQUARE_ANIMATION_NAME,
                    CardSuit.SuitCross => MATCH_SYMBOL_CROSS_ANIMATION_NAME,
                    CardSuit.SuitTriangle => MATCH_SYMBOL_TRIANGLE_ANIMATION_NAME,
                    CardSuit.SuitCircle => MATCH_SYMBOL_CIRCLE_ANIMATION_NAME,
                    CardSuit.SuitStar => MATCH_SYMBOL_STAR_ANIMATION_NAME,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(suit), suit, null)
                };

                Utility.PlayAnimation(matchSymbolAnimation, animationName, false);
                matchSymbolAnimation.AnimationState.Complete += delegate
                {
                    matchSymbolAnimationParent.gameObject.SetActive(false);
                };

            });
    }

    private void AnimateOpenSuitPickerWheel()
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                wheelTransform.gameObject.SetActive(true);
            });
    }


    public void AnimateShowRemainingCards(bool isVictory)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                foreach (WhotPlayer player in playersList)
                {
                    if (!player.isCurrentPlayer)
                    {
                        player.HideCardsLeft();
                        player.AnimateShowRemainingCards();
                    }
                    else
                    {
                        playerHand.DisplayScore();
                    }
                }
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                if (isVictory)
                {
                    AnimateVictory();
                }
                else
                {
                    AnimateBetterLuckNextTime();
                }

            });
    }
    public void AnimateVictory()
    {
        victoryAnimationParent.gameObject.SetActive(true);
        StopWaitAnimation();
        playAreaParent.gameObject.SetActive(false);
        foreach (WhotPlayer player in playersList)
        {
            if (!player.isCurrentPlayer)
            {
                player.HideRemainingCards();
            }
        }
        victoryImage.transform.localPosition = Vector3.zero;
        victoryImage.transform.localScale = Vector3.zero;
        victoryImage.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack);
        Utility.PlayAnimation(victoryAnimation, VICTORY_MARKET_ANIMATION_NAME, false);
        victoryAnimation.AnimationState.Complete += delegate
        {
            victoryAnimationParent.gameObject.SetActive(false);
            Sequence chipSequence = DOTween.Sequence();
            foreach (WhotPlayer player in playersList)
            {
                if (!player.isCurrentPlayer)
                {

                    player.AnimateChipTransfer(GetCurrentPlayer().GetPlayedCardParent());
                }
            }
            chipSequence.AppendInterval(2.5f);
            chipSequence.OnComplete(() =>
            {
                HandleShowMatchResult(true);
            });
        };
                
    }

    public void AnimateBetterLuckNextTime()
    {
        Debug.Log("Length: " + playersList.Count);
        loseAnimationParent.gameObject.SetActive(true);
        StopWaitAnimation();
        playAreaParent.gameObject.SetActive(false);

        foreach (WhotPlayer player in playersList)
        {
            if (!player.isCurrentPlayer)
            {
                player.HideRemainingCards();
            }
        }

        betterLuckNextTimeText.transform.localPosition = Vector3.zero;
        betterLuckNextTimeText.transform.localScale = Vector3.zero;
        betterLuckNextTimeText.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack);
        Utility.PlayAnimation(betterLuckNextTimeAnimation, LOSE_ANIMATION_NAME, false);
        betterLuckNextTimeAnimation.AnimationState.Complete += delegate
        {
            loseAnimationParent.gameObject.SetActive(false);

            // Animation transfer chips from other players to the winner
            Sequence chipSequence = DOTween.Sequence();
            foreach (WhotPlayer player in playersList)
            {
                if (!player.isWinner)
                {
                    player.AnimateChipTransfer(GetWinner().GetPlayedCardParent());
                }
            }
            chipSequence.AppendInterval(2.5f);
            chipSequence.OnComplete(() =>
            {
                HandleShowMatchResult(false);
            });
        };
    }
    private void AnimateDealACard(WhotCard card, Transform targetTransform, WhotPlayer player, int index = 0)
    {
        card.transform
            .DOMove(targetTransform.position, ANIMATION_TIME)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
        {
            WhotCard whotCard = mockCardsList[index];
            player.cards.Add(whotCard);
            player.UpdateCardsLeftVisual();
            if (player.isCurrentPlayer)
            {
                whotCard.transform.SetParent(playerHand.GetCardsParent(), worldPositionStays: false);
                whotCard.transform.localPosition = playerHand.GetNewCardPosition(whotCard);
                whotCard.transform.localScale = Vector3.one;
                playerHand.cardsInHand.Add(whotCard);
                playerHand.SpreadCards();

                if (index == mockCardsList.Count - 1)
                {
                    playerHand.AnimateSortCards();
                }
            }
            Destroy(card.gameObject);
        });
    }

    private void AnimateCurrentPlayerDrawACard(WhotCard card)
    {
        card.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            card.SetFaceUp();
            card.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        card.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            card.transform.localRotation = newRotation;
            card.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        card.transform.DOLocalMove(playerHand.GetNewCardPosition(card), ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            playerHand.SortCards();
            playerHand.SpreadCards();
        });
    }

    private void AnimateOtherPlayerDrawACard(WhotCard card, Transform targetPosition)
    {
        card.transform.localScale = Vector3.one * 0.6f;
        card.transform.DOMove(targetPosition.position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            Destroy(card.gameObject);
        });
    }

    private void AnimatePlayACard(WhotCard card)
    {
        card.SetFaceDown();
        card.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            card.SetFaceUp();
            card.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        card.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            card.transform.localRotation = newRotation;
            card.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        card.transform.DOMove(lastCardParent.position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            playerHand.SortCards();
            playerHand.SpreadCards();
        });
    }
    #endregion

    #region Events
    private void WhotSuitPicker_OnSuitPicked(CardSuit cardSuit)
    {

        AnimateMatchSymbol(cardSuit);
        StopWaitAnimation();
    }
    #endregion

    private void HandleShowMatchResult(bool isVictory)
    {
        matchResultTransform.gameObject.SetActive(true);
        WhotMatchResult matchResult = matchResultTransform.GetComponent<WhotMatchResult>();
        matchResult.SetInfo(this, playersList, isVictory);
    }

    public Transform GetLastCardParent() => lastCardParent;
    public Transform GetDeckOfCardParent() => deckOfCardParent;
    private WhotPlayer GetCurrentPlayer() => playersList.Find(player => player.isCurrentPlayer);
    private WhotPlayer GetWinner() => playersList.Find(player => player.isWinner);

    private void InitPlayer()
    {
        for (int i = 0; i < playersList.Count; i++)
        {
            WhotPlayer player = playersList[i];
            PlayerLayout playerLayout = player.GetComponent<PlayerLayout>();
            switch (i)
            {
                case 0:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Top);
                    break;
                case 1:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Top);
                    break;
                case 2:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Left);
                    break;
                case 3:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Right);
                    break;

            }
        }
    }

    #region Mock Data
    private void PrepareMockData()
    {
        CardSuit[] suits =
        {
            CardSuit.SuitCircle,
            CardSuit.SuitTriangle,
            CardSuit.SuitCross,
            CardSuit.SuitCircle,
            CardSuit.SuitCircle,
            CardSuit.SuitUnspecified
        };
        CardRank[] ranks =
        {
            CardRank.Rank8,
            CardRank.Rank1,
            CardRank.Rank14,
            CardRank.Rank5,
            CardRank.Rank2,
            CardRank.Rank20
        };
        for (int i = 0; i < 6; i++)
        {
            GameObject cardInstance = Instantiate(cardPrefab);
            if (cardInstance.TryGetComponent<WhotCard>(out var card))
            {
                card.OnCardSelected += playerHand.WhotCard_OnCardSelected;

                card.SetInfo(
                    suits[i],
                    ranks[i]
                );
                mockCardsList.Add(card);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            foreach (Transform transform in playerPositionsList[i])
            {
                // transform.gameObject.SetActive(false);
                Destroy(transform.gameObject);
            }
            GameObject playerInstance = Instantiate(whotPlayerPrefab, playerPositionsList[i]);
            WhotPlayer player = playerInstance.GetComponent<WhotPlayer>();
            playerInstance.transform.localPosition = Vector3.zero;
            player.SetPlayerInfo(
                null, // Avatar sprite can be set later
                $"Player {i + 1}",
                1000
            );
            player.SetWhotGame(this);
            player.isWinner = false;
            if (i == 0)
            {
                player.isCurrentPlayer = true;
                player.HideCardsLeft();
            }
            else
            {
                player.isCurrentPlayer = false;
                player.ShowCardsLeft();
            }
            playersList.Add(player);
        }
    }
    #endregion
}
