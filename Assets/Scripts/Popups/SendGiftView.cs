using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SendGiftView : BaseView
{
    [SerializeField] private Button sendGiftBtn, tabSendGiftBtn, tabHistoryGiftBtn;
    [SerializeField] private Transform sendGiftPanel, historyGiftPanel;
    [SerializeField] private TextMeshProUGUI idFriendTxt;
    [SerializeField] private TextMeshProUGUI amountChipTxt;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnEnable()
    {
        sendGiftBtn.onClick.AddListener(() => _ = OnClickSendGift());
        tabSendGiftBtn.onClick.AddListener(() =>
        {
            historyGiftPanel.gameObject.SetActive(false);
            sendGiftPanel.gameObject.SetActive(true);
        });
        tabHistoryGiftBtn.onClick.AddListener(() =>
        {
            historyGiftPanel.gameObject.SetActive(true);
            sendGiftPanel.gameObject.SetActive(false);
        });
    }

    private void OnDisable()
    {
        sendGiftBtn.onClick.RemoveAllListeners();
        tabSendGiftBtn.onClick.RemoveAllListeners();
        tabHistoryGiftBtn.onClick.RemoveAllListeners();
    }

    private async UniTask OnClickSendGift()
    {
        try
        {
            string recipientId = Regex.Replace(idFriendTxt.text, @"\p{C}+", "").Trim();
            
            string cleanInput = Regex.Replace(amountChipTxt.text, @"\p{C}+", "").Trim();
         
            if (string.IsNullOrEmpty(cleanInput))
            {
                UIManager.Instance.ShowAlertDialog("Value of chip is empty.");
                return;
            }

            if (!long.TryParse(cleanInput, out long amount))
            {
                UIManager.Instance.ShowAlertDialog("Value of chip invalid.");
                return;
            }
            FreeChip freeChip =  await DataSender.SendGift(amount, recipientId);
            Debug.Log("Recieve Chip Successfull : " + freeChip);
            SendGiftSuccessful();
        }
        catch (Exception e)
        {
            UIManager.Instance.ShowAlertDialog("Error : " + e.Message);
        }
       
    }

    private void SendGiftSuccessful()
    {
        idFriendTxt.text = "";
        amountChipTxt.text = "";
        UIManager.Instance.ShowAlertDialog("Send gift successfully!");
        _ = UIManager.Instance.LoadProfileUser();
    }
}
