using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using UnityEngine;
using UnityEngine.UI;

public class WhotMatchResult : MonoBehaviour
{
    [SerializeField] private Transform backgroundWin, backgroundLose, playerResultParent;
    [SerializeField] private Image victoryImage, loseImage;
    [SerializeField] private GameObject playerResultPrefab;
    private WhotView whotGame;

    public void SetInfo(WhotView whotGame, List<WhotPlayer> players, List<WhotPlayerResult> result, bool isVictory)
    {
        this.whotGame = whotGame;
        backgroundWin.gameObject.SetActive(isVictory);
        backgroundLose.gameObject.SetActive(!isVictory);
        victoryImage.gameObject.SetActive(isVictory);
        loseImage.gameObject.SetActive(!isVictory);

        foreach (Transform child in playerResultParent)
        {
            Destroy(child.gameObject);
        }

        foreach (WhotPlayer player in players)
        {
            GameObject playerResult = Instantiate(playerResultPrefab, playerResultParent);
            WhotPlayerResultItem resultComponent = playerResult.GetComponent<WhotPlayerResultItem>();
            long totalPoints = result.Find(r => r.UserId == player.playerId)?.TotalPoints ?? 0;
            List<Card> remainingCards = result.Find(r => r.UserId == player.playerId)?.RemainingCards.ToList();
            
            resultComponent.SetInfo(
                player: player,
                cash: "2000",
                score: totalPoints.ToString(),
                remainingCards: remainingCards,
                isVictory: isVictory
            );
        }
    }

    public void OnClickWinMore()
    {

    }

    public void OnClickPlayAgain()
    {
        gameObject.SetActive(false);
    }
}
