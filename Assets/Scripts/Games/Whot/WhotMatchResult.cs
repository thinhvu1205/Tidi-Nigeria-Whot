using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotMatchResult : MonoBehaviour
{
    [SerializeField] private Transform backgroundWin, backgroundLose, playerResultParent;
    [SerializeField] private Image victoryImage, loseImage;
    [SerializeField] private GameObject playerResultPrefab, betMoreNote;
    [SerializeField] private TextMeshProUGUI timerText, betMoreNoteText;
    [SerializeField] private Button winMoreButton;

    private List<Bet> betItemList = new();
    private WhotView whotGame;
    private int timer = 10;

    private void OnEnable()
    {
        if (User.userProfile.VipLevel == 0)
        {
            winMoreButton.gameObject.SetActive(false);
            return;
        }
        winMoreButton.gameObject.SetActive(true);
    }
    
    public void UpdateTimerCountdown(int countdown)
    {
        timer = countdown;
        timerText.text = timer.ToString();

        if (timer == 1 && gameObject.activeInHierarchy)
        {
            OnClickPlayAgain();
        }
    }

    public void SetInfo(WhotView whotGame, List<WhotPlayer> players, List<WhotPlayerResult> result, List<BalanceUpdate> balanceUpdates, bool isVictory)
    {
        this.whotGame = whotGame;
        UpdateBetNoteText();
        SetWinMoreButtonListener();
        backgroundWin.gameObject.SetActive(isVictory);
        backgroundLose.gameObject.SetActive(!isVictory);
        victoryImage.gameObject.SetActive(isVictory);
        loseImage.gameObject.SetActive(!isVictory);

        Dictionary<string, PlayerResultData> playerResults = result
            .Where(r => !string.IsNullOrEmpty(r.UserId))
            .ToDictionary(
                r => r.UserId,
                r =>
                {
                    var balance = balanceUpdates.Find(b => b.UserId == r.UserId);
                    return new PlayerResultData(
                        r.RemainingCards?.ToList() ?? new List<WhotCard>(),
                        r.TotalPoints,
                        r.IsWinner,
                        balance?.AmountChipAdd ?? 0
                    );
                }
            );
        foreach (Transform child in playerResultParent)
        {
            Destroy(child.gameObject);
        }

        foreach (WhotPlayer player in players)
        {
            if (playerResults.TryGetValue(player.Id, out PlayerResultData resultData))
            {
                WhotPlayerResultItem playerResult = Instantiate(playerResultPrefab, playerResultParent).GetComponent<WhotPlayerResultItem>();
                playerResult.SetInfo(
                    player: player,
                    cash: resultData.AmountChipAdd.ToString(),
                    score: resultData.TotalPoints.ToString(),
                    remainingCards: resultData.RemainingCards,
                    isVictory: isVictory
                );

            }
        }
    }

    public void OnClickPlayAgain()
    {
        whotGame.ResetAllPlayers();
        gameObject.SetActive(false);
    }

    public void OnClickBetMoreNote()
    {
        betMoreNote.SetActive(false);
    }
    
    private void UpdateBetNoteText()
    {
        betMoreNote.SetActive(true);
        betMoreNoteText.text = $"Bet more win more, click here to the higher bet ({whotGame.HigherMarkUnit}) games!";
    }

    private void SetWinMoreButtonListener()
    {
        winMoreButton.onClick.RemoveAllListeners();
        winMoreButton.onClick.AddListener(() =>
        {
            UniTask.Void(async () =>
            {
                if (whotGame != null) whotGame.TypeWinMore = true;
                await UIManager.Instance.HandleLeaveGame();
            });
        });
    }


    private struct PlayerResultData
    {
        public List<WhotCard> RemainingCards;
        public long TotalPoints;
        public bool IsWinner;
        public long AmountChipAdd;

        public PlayerResultData(List<WhotCard> remainingCards, long totalPoints, bool isWinner, long amountChipAdd)
        {
            RemainingCards = remainingCards;
            TotalPoints = totalPoints;
            IsWinner = isWinner;
            AmountChipAdd = amountChipAdd;
        }
    }
}
