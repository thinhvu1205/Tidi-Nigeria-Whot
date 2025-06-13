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
    [SerializeField] private Color disabledTitleTextColor, disablePlayerCountTextColor;

    public void SetData(Bet dataItem, int index)
    {
        gameObject.name = "" + dataItem.MarkUnit;
        betAmountText.text = Utility.FormatMoney((int)dataItem.MarkUnit, true);
        if (dataItem.Enable)
        {
            backgroundImage.sprite = backgroundSpriteList[index % 4];
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(async () =>
            {
                RpcFindMatchResponse response = await DataSender.FindMatch(Constants.WhotGameID, (int)dataItem.MarkUnit);
                Debug.Log("Find match response: " + response.ToString());
                if (response != null && response.Matches.Count > 0)
                {
                    await DataSender.JoinMatch(response.Matches[0].MatchId);
                    UIManager.Instance.OpenGame("whot");
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
        RpcFindMatchResponse response = await DataSender.FindMatch(Constants.WhotGameID, markUnit);
        if (response != null && response.Matches.Count > 0)
        {
            DataSender.JoinMatch(response.Matches[0].MatchId);
            UIManager.Instance.OpenGame("whot");
        }
    }
}
