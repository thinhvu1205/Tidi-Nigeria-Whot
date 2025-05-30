using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Api;

public class WhotPlayer : MonoBehaviour
{
    [SerializeField] private Image avatarImage, countdownImage, lightImage;
    [SerializeField] private TextMeshProUGUI nameText, chipText, cardsLeftText;
    [SerializeField] private GameObject lastCardNoti, cards;
    private float countDownTimer = 10f;
    private bool isCountingDown = false;

    private void Awake()
    {
        lastCardNoti.SetActive(false);
        countdownImage.gameObject.SetActive(false);
        lightImage.gameObject.SetActive(false);

        HideCardsLeft();
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
        int chipAmount,
        int cardsLeftCount
    )
    {
        avatarImage.sprite = avatarSprite;
        nameText.text = playerName;
        chipText.text = chipAmount.ToString();
        cardsLeftText.text = cardsLeftCount.ToString();
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

    private void HideCardsLeft()
    {
        cards.SetActive(false);
    }
}
