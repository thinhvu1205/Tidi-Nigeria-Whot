using System;
using System.Collections;
using System.Collections.Generic;
using Api;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI betTitleText, betAmountText, playerCountText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button button;
    [SerializeField] private List<Sprite> backgroundSpriteList, disabledBackgroundSpriteList;
    [SerializeField] private TMP_FontAsset disableFont;
    [SerializeField] private UnityEngine.Color disabledTitleTextColor, disablePlayerCountTextColor;

    public void SetData(Bet dataItem, int index)
    {
        gameObject.name = "" + dataItem.MarkUnit;
        betAmountText.text = Utility.FormatMoney((int)dataItem.MarkUnit, true);
        playerCountText.text = dataItem.CountPlaying.ToString();
        if (dataItem.Enable)
        {
            backgroundImage.sprite = backgroundSpriteList[index % 4];
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(async () =>
            {
                try
                {
                    RpcFindMatchResponse response = await DataSender.FindMatch(Config.currentGameId, (int)dataItem.MarkUnit, true);
                    Debug.Log("Find match response: " + response.ToString());
                    if (response.Matches.Count > 0)
                    {
                        try
                        {
                            await DataSender.JoinMatch(response.Matches[0].MatchId);
                            UIManager.Instance.HandleOpenGame();
                        }
                        catch (Exception joinEx)
                        {
                            Debug.Log("Error joining match: " + joinEx.Message);
                            UIManager.Instance.OpenDialog(joinEx.Message, null, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UIManager.Instance.OpenDialog(ex.Message, null, null);
                }
            });
        }
        else
        {
            backgroundImage.sprite = disabledBackgroundSpriteList[index % 4];
            betAmountText.font = disableFont;
            betTitleText.color = disabledTitleTextColor;
            playerCountText.color = disablePlayerCountTextColor;


        }
    }

    private async void OnClickBetItem(int markUnit)
    {
        RpcFindMatchResponse response = await DataSender.FindMatch(Constants.WHOT_GAME_ID, markUnit, true);
        if (response != null && response.Matches.Count > 0)
        {
            await DataSender.JoinMatch(response.Matches[0].MatchId);
            UIManager.Instance.OpenGame("whot");
        }
    }
}
