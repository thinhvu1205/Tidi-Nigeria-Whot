using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Api;
using DG.Tweening;

public class WhotPlayer : MonoBehaviour
{
    [SerializeField] private Image avatarImage, countdownImage, lightImage;
    [SerializeField] private TextMeshProUGUI nameText, chipText, cardsLeftText;
    [SerializeField] private GameObject lastCardNoti, cardsDisplay, cardPrefab;
    [HideInInspector] public List<WhotCard> cards;
    [HideInInspector] public bool isCurrentPlayer = false;
    private WhotGame whotGame;
    private float turnTimer = 10f; // Default turn timer duration
    private float countDownTimer = 10f;
    private bool isCountingDown = false;
    private const float ANIMATION_TIME = 0.5f;

    private void Awake()
    {
        lastCardNoti.SetActive(false);
        countdownImage.gameObject.SetActive(false);
        lightImage.gameObject.SetActive(false);
        cards = new();
        HideCardsLeft();
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
        Sprite avatarSprite,
        string playerName,
        int chipAmount
    )
    {
        avatarImage.sprite = avatarSprite;
        nameText.text = playerName;
        chipText.text = chipAmount.ToString();
    }

    public void SetWhotGame(WhotGame whotGame)
    {
        this.whotGame = whotGame;
    }

    public void ToggleLastCardNoti()
    {
        lastCardNoti.SetActive(!lastCardNoti.activeSelf);
    }

    public void DrawACard()
    {
        Debug.Log("Drawing a card for player: " + nameText.text);
        whotGame.DrawACard(cards, GetPlayedCardParent(), isCurrentPlayer);
        UpdateCardsLeftVisual();

    }

    public void PlayACard()
    {
        if (cards.Count > 0)
        {
            WhotCard card = cards[0];
            cards.RemoveAt(0);
            whotGame.PlayACard(card, GetPlayedCardParent(), isCurrentPlayer);
            UpdateCardsLeftVisual();

            if (cards.Count == 1)
            {
                AnimateShowLastCardNoti();
            }
            else
            {
                AnimateHideLastCardNoti();
            }
        }
    }

    public void StartCountDown()
    {
        countDownTimer = turnTimer; // Reset the countdown timer
        isCountingDown = true;
        countdownImage.gameObject.SetActive(true);
        lightImage.gameObject.SetActive(true);
        countdownImage.fillAmount = 1f;
        lightImage.fillAmount = 1f;
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
        Debug.Log("Cards count: " + cards.Count);
        cardsLeftText.text = cards.Count.ToString();
        if (!isCurrentPlayer && cards.Count > 0)
        {
            cardsDisplay.SetActive(true);
        }
    }

    public void AnimateShowLastCardNoti()
    {
        lastCardNoti.SetActive(true);
        lastCardNoti.transform.localScale = Vector3.zero;
        lastCardNoti.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack);
    }

    public void AnimateHideLastCardNoti()
    {
        lastCardNoti.transform.DOScale(Vector3.zero, ANIMATION_TIME).SetEase(Ease.InBack).OnComplete(() =>
        {
            lastCardNoti.SetActive(false);
        });
    }

    public Transform GetDealedCardParent()
    {
        return cardsDisplay.transform;
    }

    public Transform GetPlayedCardParent()
    {
        return avatarImage.transform;
    }
}
