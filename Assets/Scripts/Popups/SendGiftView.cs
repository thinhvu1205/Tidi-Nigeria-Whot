using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SendGiftView : BaseView
{
    [SerializeField] private GameObject sendGiftTab, historyTab;
    [SerializeField] private GameObject selectedSendGiftTab, selectedHistoryTab;
    [SerializeField] private TextMeshProUGUI textChip;
    protected override void OnEnable()
    {
        base.OnEnable();
        OnClickSendGiftTab();
        textChip.text = User.userMain.accountChip.ToString();

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
    }
}
