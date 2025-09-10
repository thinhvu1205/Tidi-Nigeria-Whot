using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SendGiftView : BaseView
{
    [SerializeField] private GameObject sendGiftTab, historyTab;
    [SerializeField] private GameObject selectedSendGiftTab, selectedHistoryTab;
    [SerializeField] private ItemHistoryGift itemHistoryGiftPrefab;
    [SerializeField] private Transform historyItemContainer;
    [SerializeField] private Button sendGiftBtn;
    [SerializeField] private TextMeshProUGUI currentChipTxt;
    [SerializeField] private TextMeshProUGUI idFriendTxt;
    [SerializeField] private TextMeshProUGUI amountChipTxt;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        sendGiftBtn.onClick.AddListener(() => _ = OnClickSendGift());
        OnClickSendGiftTab();
        currentChipTxt.text = User.userProfile.AccountChip.ToString();
    }
    
    private void OnDisable()
    {
        sendGiftBtn.onClick.RemoveAllListeners();
    }

    public void OnClickSendGiftTab()
    {
        selectedSendGiftTab.SetActive(true);
        selectedHistoryTab.SetActive(false);
        sendGiftTab.SetActive(true);
        historyTab.SetActive(false);
    }

    public void OnClickHistoryTab()
    {
        selectedSendGiftTab.SetActive(false);
        selectedHistoryTab.SetActive(true);
        sendGiftTab.SetActive(false);
        historyTab.SetActive(true);
        _ = LoadDataHistoryGift();
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
            _ = SendGiftSuccessful();
        }
        catch (Exception e)
        {
            UIManager.Instance.ShowAlertDialog("Error : " + e.Message);
        }
       
    }

    private async UniTask LoadDataHistoryGift()
    {
        try
        {
            UIManager.Instance.ShowProgressing();
            string metaBankActionStr = ((int)Constants.MetaBankAction.SendGift).ToString();
            Constants.WalletTransaction walletTransaction =
                await DataSender.GetTransactionHistory(20,
                    metaBankAction: metaBankActionStr);
            Debug.Log($"Receive data history {walletTransaction}");
            LoadHistoryGiftSuccessful(walletTransaction);
            UIManager.Instance.HideProgressing();
        }
        catch (Exception e)
        {
            UIManager.Instance.HideProgressing();
            UIManager.Instance.ShowAlertDialog($"Error : {e.Message}");
        }
    }

    private async UniTask SendGiftSuccessful()
    {
        idFriendTxt.text = "";
        amountChipTxt.text = "";
        UIManager.Instance.ShowAlertDialog("Send gift successfully!");
        await UIManager.Instance.LoadProfileUser();
        currentChipTxt.text = User.userProfile.AccountChip.ToString();
    }

    private void LoadHistoryGiftSuccessful(Constants.WalletTransaction transaction)
    {
        foreach (var walletLedgerItem in transaction.transactions)
        {
            ItemHistoryGift itemHistoryGift = Instantiate(itemHistoryGiftPrefab, historyItemContainer);
            itemHistoryGift.SetData(walletLedgerItem);
        }
    }
    
}
