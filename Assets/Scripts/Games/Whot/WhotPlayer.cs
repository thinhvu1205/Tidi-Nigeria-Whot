using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Api;
using DG.Tweening;
using Globals;
using System;

public class WhotPlayer : MonoBehaviour
{
    [SerializeField] private Image avatarImage, countdownImage, lightImage, holdOnImage, suspensionImage, scoreImage;
    [SerializeField] private TextMeshProUGUI nameText, chipText, cardsLeftText, effectText, scoreText;
    [SerializeField] private GameObject lastCardNoti, effectNoti, cardsDisplay, cardPrefab, chipPrefab;
    [SerializeField] private Transform remainingCardsParent;
    [HideInInspector] public bool isCurrentPlayer = false;
    [HideInInspector] public bool isWinner = false;
    public string playerId { get; private set; } = string.Empty;
    public int cardsLeft { get; private set; } = 0;
    private WhotGame whotGame;
    private PlayerLayout playerLayout;
    private float turnTimer = 10f; // Default turn timer duration
    private float countDownTimer = 10f;
    private bool isCountingDown = false;
    private const float ANIMATION_TIME = 0.35f;
    private const float ANIMATION_DURATION = 0.4f;
    private const float CARD_SCALE = 0.86f;
    private const float SCORE_IMAGE_OFFSET = 120f;
    private float CARD_SPACING = CARD_SCALE / 2 * 100f;

    private void Awake()
    {
        playerLayout = GetComponent<PlayerLayout>();
        lastCardNoti.SetActive(false);
        countdownImage.gameObject.SetActive(false);
        lightImage.gameObject.SetActive(false);
        holdOnImage.gameObject.SetActive(false);
        suspensionImage.gameObject.SetActive(false);
        effectNoti.SetActive(false);
        scoreImage.gameObject.SetActive(false);
        // HideCardsLeft();
        UpdateCardsLeftVisual();

    }

    private void Update()
    {
        if (isCountingDown)
        {
            countDownTimer -= Time.deltaTime;
            if (countDownTimer <= 0f)
            {
                isCountingDown = false;
                countdownImage.gameObject.SetActive(false);
                lightImage.gameObject.SetActive(false);
            }
            else
            {
                countdownImage.fillAmount = countDownTimer / turnTimer;
                lightImage.fillAmount = countDownTimer / turnTimer;
            }
        }
    }

    public void SetPlayerInfo(
        string playerId,
        string avatarSprite,
        string playerName,
        string chipAmount
    )
    {
        this.playerId = playerId;
        // avatarImage.sprite = avatarSprite;
        nameText.text = playerName;
        chipText.text = chipAmount;
    }

    public void SetWhotGame(WhotGame whotGame)
    {
        this.whotGame = whotGame;
        whotGame.OnNextTurn += WhotGame_OnNextTurn;
    }

    public void ToggleLastCardNoti()
    {
        lastCardNoti.SetActive(!lastCardNoti.activeSelf);
    }

    public void AddACard()
    {
        cardsLeft++;
        UpdateCardsLeftVisual();
    }

    public void RemoveACard()
    {
        cardsLeft--;
        UpdateCardsLeftVisual();
    }

    public void PlayACard(WhotCard card)
    {
        cardsLeft--;
        card.SetSelectable(false);
        whotGame.PlayACard(this, card, GetPlayedCardParent());
        UpdateCardsLeftVisual();
        StopCountDown();
        if (cardsLeft == 1)
        {
            AnimateShowLastCardNoti();
        }
        else if (cardsLeft == 0)
        {
            AnimateHideLastCardNoti();
            whotGame.AnimateShowRemainingCards();
            isWinner = true;
        }
        else
        {
            AnimateHideLastCardNoti();
        }
    }

    #region Visuals

    public void AnimateShowRemainingCards()
    {
        // remainingCardsParent.gameObject.SetActive(true);
        // WhotCard[] cardsLeft = remainingCardsParent.GetComponentsInChildren<WhotCard>();
        // foreach (WhotCard card in cardsLeft)
        // {
        //     Destroy(card.gameObject);
        // }
        // if (this.cardsLeft >= 8)
        // {
        //     CARD_SPACING = CARD_SCALE / 2 * 75;
        // }
        // float totalWidth = (cards.Count - 1) * CARD_SPACING;
        // float startX = -totalWidth / 2f;
        // for (int i = 0; i < cards.Count; i++)
        // {
        //     WhotCard card = cards[i];
        //     WhotCard whotCard = Instantiate(cardPrefab, remainingCardsParent).GetComponent<WhotCard>();
        //     whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        //     whotCard.SetSelectable(false);
        //     whotCard.transform.localScale = Vector3.one * CARD_SCALE;

        //     CanvasGroup cardCanvasGroup = whotCard.GetComponent<CanvasGroup>();
        //     CanvasGroup scoreCanvasGroup = scoreImage.GetComponent<CanvasGroup>();
        //     cardCanvasGroup.alpha = 0f;
        //     scoreCanvasGroup.alpha = 0f;

        //     Vector3 offset = Vector3.zero;
        //     Vector3 targetPos = Vector3.zero;

        //     switch (playerLayout.GetCurrentLayout())
        //     {
        //         case PlayerLayout.EPlayerLayout.Left:
        //             targetPos = new Vector3(i * CARD_SPACING, 0f, 0f);
        //             offset = new Vector3(-20f, 0f, 0f);
        //             scoreImage.transform.localPosition = new Vector3(totalWidth + SCORE_IMAGE_OFFSET, 0f, 0f);
        //             whotCard.transform.SetSiblingIndex(i);
        //             break;
        //         case PlayerLayout.EPlayerLayout.Top:
        //             targetPos = new Vector3(startX + i * CARD_SPACING, 0f, 0f);
        //             offset = new Vector3(-20f, 0f, 0f);
        //             scoreImage.transform.localPosition = new Vector3(startX + totalWidth + SCORE_IMAGE_OFFSET, 0f, 0f);
        //             whotCard.transform.SetSiblingIndex(i);
        //             break;
        //         case PlayerLayout.EPlayerLayout.Right:
        //             targetPos = new Vector3(-i * CARD_SPACING, 0f, 0f);
        //             offset = new Vector3(20f, 0f, 0f);
        //             scoreImage.transform.localPosition = new Vector3(-totalWidth - SCORE_IMAGE_OFFSET, 0f, 0f);
        //             whotCard.transform.SetSiblingIndex(remainingCardsParent.childCount - 1);
        //             break;
        //     }

        //     whotCard.transform.localPosition = targetPos + offset;

        //     // Animate move & fade
        //     whotCard.transform.DOLocalMove(targetPos, ANIMATION_TIME).SetEase(Ease.OutCubic).SetDelay(i * 0.05f);
        //     cardCanvasGroup.DOFade(1f, ANIMATION_TIME / 2).SetDelay(i * 0.1f);

        //     if (i == this.cardsLeft - 1)
        //     {
        //         scoreImage.gameObject.SetActive(true);
        //         scoreCanvasGroup.DOFade(1f, ANIMATION_TIME / 2).SetDelay((i + 1) * 0.1f);
        //     }
        // }
    }

    public void HideRemainingCards()
    {
        remainingCardsParent.gameObject.SetActive(false);
        if (scoreImage != null)
        {
            scoreImage.gameObject.SetActive(false);
        }
        foreach (Transform child in remainingCardsParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void StartCountDown(int countdown)
    {
        turnTimer = countdown;
        countDownTimer = countdown; // Reset the countdown timer
        isCountingDown = true;
        countdownImage.gameObject.SetActive(true);
        lightImage.gameObject.SetActive(true);
        countdownImage.fillAmount = 1f;
        lightImage.fillAmount = 1f;
    }

    public void StopCountDown()
    {
        isCountingDown = false;
        countdownImage.gameObject.SetActive(false);
        lightImage.gameObject.SetActive(false);
    }

    public void ShowCardsLeft()
    {
        cardsDisplay.SetActive(true);
    }

    public void HideCardsLeft()
    {
        cardsDisplay.SetActive(false);
    }

    public void UpdateCardsLeftVisual()
    {
        cardsLeftText.text = cardsLeft.ToString();
        if (!isCurrentPlayer && cardsLeft > 0)
        {
            cardsDisplay.SetActive(true);
        }
    }

    public void UpdateEffectNoti(string effect)
    {
        effectText.text = effect;
        AnimateShowEffectNoti();
    }

    #endregion

    #region Events
    private void WhotGame_OnNextTurn(WhotGame.OnNextTurnEventArg e)
    {
        if (e.playerTurn == playerId)
        {
            StartCountDown(e.countdown);
        }
    }
    #endregion

    #region Animations 

    private void AnimateShowLastCardNoti()
    {
        lastCardNoti.SetActive(true);
        lastCardNoti.transform.localScale = Vector3.zero;
        lastCardNoti.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack);
    }

    private void AnimateHideLastCardNoti()
    {
        lastCardNoti.transform.DOScale(Vector3.zero, ANIMATION_TIME).SetEase(Ease.InBack).OnComplete(() =>
        {
            lastCardNoti.SetActive(false);
        });
    }

    public void AnimateShowHoldOn()
    {
        Sequence holdOnSequence = DOTween.Sequence();
        holdOnImage.gameObject.SetActive(true);
        holdOnImage.transform.localScale = Vector3.zero;
        holdOnSequence
            .Append(holdOnImage.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack))
            .AppendInterval(ANIMATION_DURATION)
            .OnComplete(() =>
            {
                AnimateHideHoldOn();
            });
    }

    public void AnimateHideHoldOn()
    {
        holdOnImage.transform.DOScale(Vector3.zero, ANIMATION_TIME).SetEase(Ease.InBack).OnComplete(() =>
        {
            holdOnImage.gameObject.SetActive(false);
        });
    }

    public void AnimateShowSuspension()
    {
        Sequence suspensionSequence = DOTween.Sequence();

        suspensionImage.gameObject.SetActive(true);
        suspensionImage.transform.localScale = Vector3.zero;
        suspensionSequence
            .Append(suspensionImage.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack))
            .AppendInterval(ANIMATION_DURATION)
            .OnComplete(() =>
            {
                AnimateHideSuspension();
            });
    }

    public void AnimateHideSuspension()
    {
        suspensionImage.transform.DOScale(Vector3.zero, ANIMATION_TIME).SetEase(Ease.InBack).OnComplete(() =>
        {
            suspensionImage.gameObject.SetActive(false);
        });
    }

    private void AnimateShowEffectNoti()
    {
        Sequence effectNotiSequence = DOTween.Sequence();

        effectNoti.SetActive(true);
        effectNoti.transform.localScale = Vector3.zero;
        effectNotiSequence
            .Append(effectNoti.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack))
            .AppendInterval(ANIMATION_DURATION)
            .OnComplete(() =>
            {
                AnimateHideEffectNoti();
            });
    }

    private void AnimateHideEffectNoti()
    {
        effectNoti.transform.DOScale(Vector3.zero, ANIMATION_TIME).SetEase(Ease.InBack).OnComplete(() =>
        {
            effectNoti.SetActive(false);
        });
    }

    public void AnimateChipTransfer(Transform otherPlayerTransform)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject chipInstance = Instantiate(chipPrefab, transform);
            chipInstance.transform.position = GetPlayedCardParent().position;
            chipInstance.transform.localScale = Vector3.one;

            Sequence chipSequence = DOTween.Sequence();
            chipSequence
                .AppendInterval(i * 0.1f)
                .Append(
                    chipInstance.transform
                        .DOMove(otherPlayerTransform.position, ANIMATION_TIME)
                        .SetEase(Ease.OutCubic)
                )
                .OnComplete(() =>
                {
                    Destroy(chipInstance);
                });
        }
    }
    #endregion

    #region Getters and Setters
    public Image GetAvatarImage()
    {
        return avatarImage;
    }

    public string GetPlayerName()
    {
        return nameText.text;
    }   
    public Transform GetDealedCardParent()
    {
        return cardsDisplay.transform;
    }

    public Transform GetPlayedCardParent()
    {
        return avatarImage.transform;
    }

    private Vector2 GetRemainingCardPosition()
    {
        return new Vector2(0f, 0f);
    }
    #endregion

}
