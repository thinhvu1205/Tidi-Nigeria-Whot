using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Globals;
using Proto;
using Google.Protobuf;
using System.Collections.Generic;
using Color = UnityEngine.Color;

public class SiXiangBuyGemsPopup : BaseView
{
    // Start is called before the first frame update
    [SerializeField] Image imageGem;
    [SerializeField] TextMeshProUGUI textInfo;
    [SerializeField] Sprite[] listSpriteGem;
    [SerializeField] Button buttonConfirm;
    private int typeGameBonus = 0;
    private long price = 0;

    protected Dictionary<int, int> GemIndexDictionary => new()
    {
        { 0, 3 }, // Dragon Pearl
        { 1, 5 }, // Gold Pick
        { 2, 6 }, // Rapid Pay
        { 3, 4 }, // Lucky Draw
    };
    
    public void OnClickConfirm()
    {
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.CLICK);
        long playerBalance = User.userProfile.AccountChip;
        if (playerBalance >= price)
        {
            InfoBet infoBet = new()
            {
                Id = GemIndexDictionary[typeGameBonus],
            };
            Debug.Log("TYPEGAMEBONUS: " + typeGameBonus);
            DataSender.SendMatchState((long)OpCodeRequest.BuySixiangGem, infoBet.ToByteArray());


            // SiXiangView.Instance.agPlayer -= price;
            // SiXiangView.Instance.setAGPlayer();
        }
        else
        {
            // string textShow = Globals.Config.getTextConfig("txt_not_enough_money_gl");
            // UIManager.instance.showDialog(textShow, Globals.Config.getTextConfig("shop"), () =>
            //  {
            //      UIManager.instance.openShop();
            //  }, Globals.Config.getTextConfig("label_cancel"));
        }
        OnClickCloseButton();
    }
    public void SetInfo(int indexGem, long pricePearl, long bet)
    {
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.CLICK);
        long accountChip = User.userProfile.AccountChip;
        bool isEnoughChip = accountChip >= pricePearl;
        price = pricePearl;
     
        buttonConfirm.interactable = isEnoughChip;
        buttonConfirm.GetComponent<Image>().color = isEnoughChip ? Color.white : Color.gray;
        typeGameBonus = indexGem;
        imageGem.sprite = listSpriteGem[indexGem];
        textInfo.text = "Pay " + Utility.FormatNumber(price) + " chips to receive this gem!";//Globals.Config.formatStr(Globals.Config.getTextConfig("text_sixiang_buy_gem"), Globals.Config.FormatNumber(price));
    }
}
