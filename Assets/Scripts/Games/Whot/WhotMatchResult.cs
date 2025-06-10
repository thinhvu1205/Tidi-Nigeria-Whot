using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhotMatchResult : MonoBehaviour
{
    [SerializeField] private Transform backgroundWin, backgroundLose, playerResultParent;
    [SerializeField] private Image victoryImage, loseImage;
    [SerializeField] private GameObject playerResultPrefab;
    private WhotGame whotGame;

    public void SetInfo(WhotGame whotGame, List<WhotPlayer> players, bool isVictory)
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
            resultComponent.SetInfo(
                player: player,
                cash: "2000",
                score: "20",
                isVictory: isVictory
            );
        }
    }

    public void OnClickWinMore()
    {

    }

    public void OnClickPlayAgain()
    {
        whotGame.HandleResetGame();
        gameObject.SetActive(false);
    }
}
