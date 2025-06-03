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
    private float countDownTimer = 10f;
    private bool isCountingDown = false;
    private const float ANIMATION_TIME = 0.3f;

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
                countdownImage.fillAmount = countDownTimer / 10f;
                lightImage.fillAmount = countDownTimer / 10f;
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

    public void ToggleLastCardNoti()
    {
        lastCardNoti.SetActive(!lastCardNoti.activeSelf);
    }

    public void StartCountDown()
    {
        countDownTimer = 10f; // Reset the countdown timer
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

    public Transform GetDealedCardParent()
    {
        return cardsDisplay.transform;
    }
}
