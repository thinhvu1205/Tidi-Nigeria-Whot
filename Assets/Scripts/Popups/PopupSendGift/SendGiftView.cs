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
    [SerializeField] private TMP_InputField idInputField, amountChipInputField;
    private bool canSend = true;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        sendGiftBtn.onClick.AddListener(() => _ = OnClickSendGift());
        OnClickSendGiftTab();
        currentChipTxt.text = Utility.FormatNumber(User.userProfile.AccountChip);
        amountChipInputField.onValueChanged.AddListener(OnAmountChanged);
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

    private void OnAmountChanged(string input)
    {
        // if (isEditing) return; // tránh lặp vô hạn
        // isEditing = true;

        // // Xoá dấu phẩy cũ để parse lại
        // if (long.TryParse(input, out long value))
        // {
        //     amountChipInputField.text = Utility.FormatNumber(value);
        // }
        // else
        // {
        //     // Nếu không parse được (ví dụ nhập ký tự lạ), giữ nguyên
        //     amountChipInputField.text = input;
        // }

        // isEditing = false;
    }

    private async UniTask OnClickSendGift()
    {
        if (!canSend) return;
        canSend = false;
        string recipientId = Regex.Replace(idInputField.text, @"\p{C}+", "").Trim();
        
        string cleanInput = Regex.Replace(amountChipInputField.text, @"\p{C}+", "").Trim();

        if (string.IsNullOrEmpty(recipientId))
        {
            UIManager.Instance.ShowAlertDialog("Recipient ID is empty.");
            canSend = true;
            return;
        }
        
        if (string.IsNullOrEmpty(cleanInput))
        {
            UIManager.Instance.ShowAlertDialog("Value of chip is empty.");
            canSend = true;
            return;
        }

        if (!long.TryParse(cleanInput, out long amount))
        {
            UIManager.Instance.ShowAlertDialog("Value of chip invalid.");
            canSend = true;
            return;
        }

        if (User.userProfile.AccountChip < amount * 103 / 100)
        {
            UIManager.Instance.ShowAlertDialog("You do not have enough chips.");
            canSend = true;
            return;
        }
        
        UIManager.Instance.ShowProgressing();
        FreeChip freeChip = await DataSender.SendGift(amount, recipientId);
        if (freeChip != null)
        {
            Debug.Log("Send Chip Successfully : " + freeChip);
            idInputField.text = "";
            amountChipInputField.text = "";
            currentChipTxt.text = Utility.FormatNumber(User.userProfile.AccountChip);

            await OnSuccess("Send gift successfully!", true);
            
            OnClickHistoryTab();
            
        }
        else
        {
            UIManager.Instance.HideProgressing();
        }
        canSend = true;
    }

    private async UniTask LoadDataHistoryGift()
    {
        try
        {
            UIManager.Instance.ShowProgressing();
            string metaBankActionStr = ((int)MetaBankAction.SendGift).ToString();
            Constants.WalletTransaction walletTransaction =
                await DataSender.GetTransactionHistory(20,
                    metaBankAction: metaBankActionStr);
            Debug.Log($"Receive data history {walletTransaction}");
            LoadHistoryGift(walletTransaction);
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
        
        // UIManager.Instance.ShowAlertDialog("Send gift successfully!");
        // await UIManager.Instance.LoadProfileUser();
    }

    private void LoadHistoryGift(Constants.WalletTransaction transaction)
    {
        foreach(Transform child in historyItemContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var walletLedgerItem in transaction.transactions)
        {
            ItemHistoryGift itemHistoryGift = Instantiate(itemHistoryGiftPrefab, historyItemContainer);
            itemHistoryGift.SetData(walletLedgerItem);
        }
    }
    
}
