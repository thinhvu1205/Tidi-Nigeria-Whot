using System;
using System.Collections;
using System.Collections.Generic;
using Proto;
using Cysharp.Threading.Tasks;
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
        betTitleText.text = Utility.FormatNumber(dataItem.AgJoin);
        if (dataItem.Enable)
        {
            backgroundImage.sprite = backgroundSpriteList[index % 4];
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _ = UIManager.Instance.HandleFindAndJoinMatch((int)dataItem.MarkUnit));
        }
        else
        {
            backgroundImage.sprite = disabledBackgroundSpriteList[index % 4];
            betAmountText.font = disableFont;
            betTitleText.color = disabledTitleTextColor;
            playerCountText.color = disablePlayerCountTextColor;
        }
    }
}
