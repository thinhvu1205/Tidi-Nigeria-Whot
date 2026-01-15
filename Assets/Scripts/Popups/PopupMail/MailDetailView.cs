using System;
using System.Collections;
using System.Collections.Generic;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailDetailView : BaseView
{
    [SerializeField] private TextMeshProUGUI textTitle, textContent, textDate;

    public void SetData(Notification notification)
    {
        textTitle.text = notification.Title;
        textContent.text = notification.Content;
        textDate.text = "Sent at: " + Utility.ConvertUnixTimeToDDMMYYYYHHMM(notification.CreateTimeUnix);
    }
    
}
