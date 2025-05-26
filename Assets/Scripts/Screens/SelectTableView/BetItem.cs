using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private List<Sprite> backgroundSpriteList;

    public void SetData(JObject dataItem, int index)
    {
        print("SetData: " + dataItem.ToString());
        gameObject.name = "" + (int)dataItem["mark"];
        betAmountText.text = Utility.FormatMoney((int)dataItem["mark"], true);
        betTitleText.text = Utility.FormatMoney((int)dataItem["mark"], true);
        backgroundImage.sprite = backgroundSpriteList[index % 4];
        // if (dataItem.ContainsKey("minAgCon"))
        // {
        //     if (Globals.User.userMain.AG >= (int)_dataItem["minAgCon"])
        //     {
        //         //bkg.sprite = lsBkg[1];
        //         backgroundImage.color = Color.white;
        //         txtBet.color = Color.white;

        //         //ColorUtility.TryParseHtmlString("#F2A433", out colorLine);
        //         //txtBet.GetComponent<Outline>().effectColor = colorLine;
        //         //txtBet.outlineColor = colorLine;
        //         // txtBet.fontMaterial = ndex%2==0? Mat_green: Mat_yellow;
        //     }
        //     else
        //     {
        //         bkg.sprite = lsBkg[2];

        //         //bkg.color = Color.gray;
        //         //txtBet.color = Color.gray;
        //         //ColorUtility.TryParseHtmlString("#986C2C", out colorLine);
        //         //txtBet.GetComponent<Outline>().effectColor = colorLine;
        //         //txtBet.outlineColor = colorLine;
        //         // txtBet.fontMaterial = Mat_gray;
        //     }
        // }
    }
}
