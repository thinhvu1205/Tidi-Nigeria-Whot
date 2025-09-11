using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Proto;
using DG.Tweening;
using Globals;
using System;
using Common.Pool;
using Games.Whot;
using Avatar = Common.Objects.Avatar;

public class WhotPlayer : MonoBehaviour
{
    [SerializeField] private Image countdownImage, lightImage, holdOnImage, suspensionImage, scoreImage;
    [SerializeField] private TextMeshProUGUI nameText, chipText, cardsLeftText, effectText, scoreText, plusText, chipAddText;
    [SerializeField] private GameObject lastCardNoti, effectNoti, cardsDisplay, cardPrefab;
    [SerializeField] private Transform remainingCardsParent;
    [SerializeField] private TMP_FontAsset chipWinFont, chipLoseFont;
    [SerializeField] private Avatar avatar;
    [HideInInspector] public bool isCurrentPlayer = false;
    [HideInInspector] public bool isWinner = false;
    [HideInInspector] public bool isPlaying = true;
    public string Id { get; private set; } = string.Empty;
    public string AvatarId { get; private set; } = string.Empty;
    public int CardsLeft { get; set; } = 0;
    private WhotView whotGame;
    private PlayerLayout playerLayout;
    private float turnTimer = 10f; // Default turn timer duration
    private float countDownTimer = 10f;
    private bool isCountingDown = false;
    private const float ANIMATION_TIME = 0.35f;
    private const float ANIMATION_DURATION = 0.4f;
    private const float CARD_SCALE = 0.86f;
    private float SCORE_IMAGE_OFFSET = 120f;
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
        plusText.gameObject.SetActive(false);
        chipAddText.gameObject.SetActive(false);
        // HideCardsLeft();
    }

    private void OnDestroy()
    {
        whotGame.OnNextTurn -= WhotGame_OnNextTurn;
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
        string avatarId,
        string playerName,
        string chipAmount = "0"
    )
    {
        Id = playerId;
        AvatarId = avatarId;
        avatar.LoadAvatar(avatarId);
        nameText.text = playerName;
        // chipText.text = Utility.FormatMoney(Utility.ConvertStringToNumber(chipAmount), true);
        AnimateChipValue(long.Parse(chipAmount));
    }

    public void SetWhotGame(WhotView whotGame)
    {
        this.whotGame = whotGame;
        whotGame.OnNextTurn += WhotGame_OnNextTurn;
    }

    public void ToggleLastCardNoti()
    {
        lastCardNoti.SetActive(!lastCardNoti.activeSelf);
    }

    public void PlayACard(WhotCardModel cardModel)
    {
        cardModel.SetSelectable(false);
        whotGame.PlayACard(cardModel, GetPlayedCardParent());
    }

    public void Reset()
    {
        isPlaying = true;
        HideCardsLeft();
        cardsLeftText.text = "0";  
        remainingCardsParent.gameObject.SetActive(false); 
    }

    #region Visuals

    public void AnimateShowRemainingCards(List<WhotCard> cards)
    {
        if (cards == null) return;
        lastCardNoti.gameObject.SetActive(false);
        remainingCardsParent.gameObject.SetActive(true);
        SortRemainingCards(cards);
        if (cards.Count >= 10)
        {
            CARD_SPACING = CARD_SCALE / 2 * 75;
            SCORE_IMAGE_OFFSET = 80f;
        }
        float totalWidth = (cards.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            WhotCard card = cards[i];
            WhotCardModel whotCardModel = PoolService.Instance.Get<WhotCardModel>(PrefabType.WhotCard);
            whotCardModel.transform.SetParent(remainingCardsParent);
            whotCardModel.SetInfo(card.Suit, card.Rank);
            whotCardModel.SetSelectable(false);
            whotCardModel.transform.localScale = Vector3.one * CARD_SCALE;

            CanvasGroup cardCanvasGroup = whotCardModel.GetComponent<CanvasGroup>();
            CanvasGroup scoreCanvasGroup = scoreImage.GetComponent<CanvasGroup>();
            cardCanvasGroup.alpha = 0f;
            scoreCanvasGroup.alpha = 0f;

            Vector3 offset = Vector3.zero;
            Vector3 targetPos = Vector3.zero;

            switch (playerLayout.GetCurrentLayout())
            {
                case PlayerLayout.EPlayerLayout.Left:
                    targetPos = new Vector3(i * CARD_SPACING, 0f, 0f);
                    offset = new Vector3(-20f, 0f, 0f);
                    scoreImage.transform.localPosition = new Vector3(totalWidth + SCORE_IMAGE_OFFSET, 0f, 0f);
                    whotCardModel.transform.SetSiblingIndex(i);
                    break;
                case PlayerLayout.EPlayerLayout.Top:
                    targetPos = new Vector3(startX + i * CARD_SPACING, 0f, 0f);
                    offset = new Vector3(-20f, 0f, 0f);
                    scoreImage.transform.localPosition = new Vector3(startX + totalWidth + SCORE_IMAGE_OFFSET, 0f, 0f);
                    whotCardModel.transform.SetSiblingIndex(i);
                    break;
                case PlayerLayout.EPlayerLayout.Right:
                    targetPos = new Vector3(2 * startX + i * CARD_SPACING, 0f, 0f); // giống Left & Top
                    offset = new Vector3(20f, 0f, 0f);
                    scoreImage.transform.localPosition = new Vector3(-totalWidth - SCORE_IMAGE_OFFSET, 0f, 0f);
                    whotCardModel.transform.SetSiblingIndex(i); // thêm theo thứ tự chuẩn
                    break;      
            }

            whotCardModel.transform.localPosition = targetPos + offset;

            // Animate move & fade
            whotCardModel.transform.DOLocalMove(targetPos, ANIMATION_TIME).SetEase(Ease.OutCubic).SetDelay(i * 0.05f);
            cardCanvasGroup.DOFade(1f, ANIMATION_TIME / 2).SetDelay(i * 0.1f);

            if (i == cards.Count - 1 && cards.Count > 0)
            {
                scoreImage.gameObject.SetActive(true);
                scoreCanvasGroup.DOFade(1f, ANIMATION_TIME / 2).SetDelay((i + 1) * 0.1f);
            }
        }
    }

    public void DisplayScore(long totalPoints)
    {
        if (totalPoints > 0)
            scoreText.text = totalPoints.ToString();
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
            WhotCardModel whotCardModel = child.GetComponent<WhotCardModel>();
            if (whotCardModel != null)
            {
                // Destroy(child.gameObject);
                PoolService.Instance.Release(PrefabType.WhotCard, whotCardModel);
            }
        }
    }

    private void StartCountDown(int countdown)
    {
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

    public void UpdateCardsLeftVisual(bool isDealingCard = false)
    {
        if (CardsLeft > 0 && !isCurrentPlayer) ShowCardsLeft();
        cardsLeftText.text = CardsLeft.ToString();
        if (CardsLeft == 0)
        {
            whotGame.AnimateLastCardEffect()
;        }
        if (CardsLeft == 1 && !lastCardNoti.activeSelf && !isDealingCard)
        {
            AnimateShowLastCardNoti();
        }
        else
        {
            AnimateHideLastCardNoti();
        }
    }

    public void UpdateEffectNoti(string effect)
    {
        effectText.text = effect;
        AnimateShowEffectNoti();
    }

    #endregion

    #region Events
    private void WhotGame_OnNextTurn(WhotView.OnNextTurnEventArg e)
    {
        if (e.playerTurn == Id)
        {
            StartCountDown(e.countdown);
        }
        else
        {
            StopCountDown();
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

    public void AnimatePlusText(int amount)
    {
        plusText.text = $"+{amount}";
        plusText.transform.localPosition = GetAvatarImage().transform.localPosition + new Vector3(-12f, 50f, 0f); 
        CanvasGroup canvasGroup = plusText.GetComponent<CanvasGroup>();

        Sequence sequence = DOTween.Sequence();
        plusText.gameObject.SetActive(true);
        sequence.Join(plusText.transform.DOLocalMove(GetAvatarImage().transform.localPosition + new Vector3(-12f, 100f, 0f), 1.5f).SetEase(Ease.OutQuad));
        sequence.Insert(0.75f, canvasGroup.DOFade(0f, 0.75f)).OnComplete(() => 
        {
            plusText.gameObject.SetActive(false);
            canvasGroup.alpha = 1f; 
        });
    }

    public void AnimateAddChipText(long amount)
    {
        chipAddText.font = amount > 0 ? chipWinFont : chipLoseFont;
        chipAddText.text = $"{amount}";

        chipAddText.transform.localPosition = GetAvatarImage().transform.localPosition + new Vector3(-12f, 20f, 0f);

        Sequence sequence = DOTween.Sequence();
        chipAddText.gameObject.SetActive(true);
        sequence
            .Append(chipAddText.transform.DOLocalMove(GetAvatarImage().transform.localPosition + new Vector3(-12f, 70f, 0f), 1.2f)
            .SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                chipAddText.gameObject.SetActive(false);
            });
    }

    public void AnimateChipTransfer(WhotPlayer winner, bool isLast)
    {
        for (int i = 0; i < 5; i++)
        {
            // GameObject chipInstance = Instantiate(chipPrefab, transform);
            WhotChip chipInstance = PoolService.Instance.Get<WhotChip>(PrefabType.ChipPlayerWhot);
            chipInstance.transform.position = GetPlayedCardParent().position;
            chipInstance.transform.localScale = Vector3.one * 0.5f;
            
            Sequence chipSequence = DOTween.Sequence();
            chipSequence
                .AppendInterval(i * 0.12f)
                .Append(
                    chipInstance.transform
                        .DOMove(winner.GetPlayedCardParent().position, ANIMATION_TIME)
                        .SetEase(Ease.OutCubic)
                )
                .OnComplete(() =>
                {
                    // Destroy(chipInstance);
                    PoolService.Instance.Release(PrefabType.ChipPlayerWhot ,chipInstance);
                    if (isLast)
                    {
                        whotGame.AnimateAllPlayersAddChip();
                    }
                });
        }
    }
    public void AnimateChipValue(long toNumber = 0)
    {
        Utility.TweenNumberTo(chipText, toNumber, GetChipAmount(), 0.5f, false);
    }

    private void SortRemainingCards(List<WhotCard> cards)
    {
        cards.Sort((a, b) => GetSortValue(a).CompareTo(GetSortValue(b)));
    }

    private int GetSortValue(WhotCard card)
    {
        int suitOrder = Constants.WhotSuitSortOrder.TryGetValue(card.Suit, out var order) ? order : 999;
        int rank = (int)card.Rank;
        return suitOrder * 100 + rank;
    }
    #endregion

    #region Getters and Setters
    public Image GetAvatarImage()
    {
        return avatar.GetAvatar();
    }

    public string GetPlayerName()
    {
        return nameText.text;
    }   

    public long GetChipAmount()
    {
        if (long.TryParse(chipText.text, out long chipAmount))
        {
            return chipAmount;
        }
        return 0;
    }
    public Transform GetDealedCardParent()
    {
        return cardsDisplay.transform;
    }

    public Transform GetPlayedCardParent()
    {
        return avatar.transform;
    }
    #endregion

}
