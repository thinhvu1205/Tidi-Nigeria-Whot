using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using Globals;
using UnityEngine;
using UnityEngine.UI;

public class WhotMatchResult : MonoBehaviour
{
    [SerializeField] private Transform backgroundWin, backgroundLose, playerResultParent;
    [SerializeField] private Image victoryImage, loseImage;
    [SerializeField] private GameObject playerResultPrefab;
    private WhotView whotGame;

    public void SetInfo(WhotView whotGame, List<WhotPlayer> players, List<WhotPlayerResult> result, List<BalanceUpdate> balanceUpdates, bool isVictory)
    {
        this.whotGame = whotGame;
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
                        r.RemainingCards?.ToList() ?? new List<Card>(),
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

    public void OnClickWinMore()
    {

    }

    public void OnClickPlayAgain()
    {
        whotGame.hasPreparedNewGame = false;
        gameObject.SetActive(false);
    }

    private struct PlayerResultData
    {
        public List<Card> RemainingCards;
        public long TotalPoints;
        public bool IsWinner;
        public long AmountChipAdd;

        public PlayerResultData(List<Card> remainingCards, long totalPoints, bool isWinner, long amountChipAdd)
        {
            RemainingCards = remainingCards;
            TotalPoints = totalPoints;
            IsWinner = isWinner;
            AmountChipAdd = amountChipAdd;
        }
    }
}
