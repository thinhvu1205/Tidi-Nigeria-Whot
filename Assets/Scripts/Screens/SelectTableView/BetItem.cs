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
using Bet = Yuujins.Cfg.Bet.V1.Bet;

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
        // Cfg Bet không có CountPlaying → hiển thị 0 hoặc "-"
        playerCountText.text = "0";
        betTitleText.text = Utility.FormatNumber(dataItem.AgJoin);
        // Cfg Bet không có Enable → tính theo VIP: user trong khoảng [MinVip, MaxVip] thì enable
        int userVip = (int)User.UserAccount.Profile.Vip;
        bool enable = userVip >= dataItem.MinVip && userVip <= dataItem.MaxVip;
        if (enable)
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
