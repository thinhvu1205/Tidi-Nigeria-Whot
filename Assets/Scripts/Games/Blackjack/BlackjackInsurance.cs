using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackInsurance : BaseView
{
    [Header(" Images ")]
    [SerializeField] private Image imageCountdown;
    [Header(" Texts ")]
    [SerializeField] private TextMeshProUGUI textCountdown;
    private BlackjackView gameView;
    private Coroutine countdownCoroutine;   

    protected override void OnEnable()
    {
        StartCountDown();
    }

    public void SetInfo(BlackjackView blackjackView)
    {
        gameView = blackjackView;
    }

    private void StartCountDown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownRoutine(5));
    }

    private IEnumerator CountdownRoutine(int startTime)
    {
        float timeLeft = startTime;
        imageCountdown.fillAmount = 1f;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            imageCountdown.fillAmount = timeLeft / startTime;
            textCountdown.text = Mathf.CeilToInt(timeLeft).ToString();
            yield return null;
        }

        // Hết thời gian
        OnClickNo();
    }

    public void OnClickYes()
    {
        BlackjackAction blackjackAction = new()
        {
            Code = BlackjackActionCode.BlackjackActionInsurance
        };
        DataSender.SendMatchState((long)OpCodeRequest.DeclareCards, blackjackAction.ToByteArray());
        Hide(false);
    }

    public void OnClickNo()
    {
        Hide(false);
    }

}
