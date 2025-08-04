using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Globals;
using Proto;
using Google.Protobuf;

public class SiXiangBuyGemsPopup : BaseView
{
    // Start is called before the first frame update
    [SerializeField] Image imageGem;
    [SerializeField] TextMeshProUGUI textInfo;
    [SerializeField] Sprite[] listSpriteGem;
    private long betCurrent = 0;
    private int typeGameBonus = 0;
    private long price = 0;
    
    public void OnClickConfirm()
    {
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.CLICK);
        long playerBalance = User.userMain.currentChip;
        if (playerBalance >= price)
        {
            InfoBet infoBet = new()
            {
                Id = UnityEngine.Random.Range(0, 4),
                Chips = betCurrent
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
        price = pricePearl;
     
        //btnConfirm.interactable = agPlayer >= price;
        //btnConfirm.GetComponent<Image>().color = agPlayer >= price ? Color.white : Color.gray;
        typeGameBonus = indexGem;
        betCurrent = bet;
        imageGem.sprite = listSpriteGem[indexGem];
        Debug.Log("price=" + price);
        textInfo.text = "Pay " + Utility.FormatNumber(price) + " chips to receive this gem!";//Globals.Config.formatStr(Globals.Config.getTextConfig("text_sixiang_buy_gem"), Globals.Config.FormatNumber(price));
    }
}
