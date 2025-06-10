using System.Collections;
using System.Collections.Generic;
using Globals;
using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotMatchMaking : MonoBehaviour
{
    [SerializeField] private Transform matchSuccesful, description, matchMakingTimeContainer;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI betText, matchingTimeText, responsiblyText;
    private int elapsedSeconds = 0;
    private Coroutine timerCoroutine;

    private void Awake()
    {
        StartTimer();
        matchSuccesful.gameObject.SetActive(false);

    }

    private void Start()
    {
        DataSender.MakingMatch("whot-game");
    }

    public void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        Debug.Log("WHOT Match Found: " + matchmakerMatched.ToString());

        matchSuccesful.gameObject.SetActive(true);
        matchMakingTimeContainer.gameObject.SetActive(false);
        description.gameObject.SetActive(false);
    }

    private void StartTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        elapsedSeconds = 0;
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    private void StopTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            matchingTimeText.text = Utility.ConvertSecondToMMSS(elapsedSeconds);
            yield return new WaitForSeconds(1f);
            elapsedSeconds++;
        }
    }
}
