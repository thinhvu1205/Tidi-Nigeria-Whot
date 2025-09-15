using System;
using System.Collections;
using System.Collections.Generic;
using Globals;
using Proto;
using TMPro;
using UnityEngine;

public class ItemHistoryGift : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeTxt, contentTxt, amountChipTxt;

    public void SetData(Constants.WalletLedgerItem walletLedgerItem) {
        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(walletLedgerItem.createdTime).DateTime;
        timeTxt.text = dateTime.ToString("HH:mm:ss dd/MM/yyyy");
        contentTxt.text = "SEND TO " + walletLedgerItem.metadata["recv"];
        amountChipTxt.text = Utility.FormatNumber(walletLedgerItem.changeset["chips"]);
    }
}
