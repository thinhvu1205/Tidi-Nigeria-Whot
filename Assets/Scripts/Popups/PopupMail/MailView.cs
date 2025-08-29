using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System;
using Cysharp.Threading.Tasks;
using Proto;
using System.Linq;
using System.Threading.Tasks;

public class MailView : BaseView
{
    [SerializeField] private GameObject mailItemPrefab;
    [SerializeField] private Transform mailItemParent;
    [SerializeField] private Image checkAllTickImage;
    [SerializeField] private Button checkAllButton;
    private List<Notification> listNotification;
    private List<MailItem> listMailSelected = new();
    private List<MailItem> listMail = new();
    private bool isCheckAll = false;

    protected override void Start()
    {
        base.Start();
        _ = LoadListMail();
    }

    #region Event    
    private void MailItem_OnCheckboxClicked(object sender, MailItem.OnCheckboxClickedEventArgs e)
    {
        bool isChecked = e.isChecked;
        Notification notification = e.notification;
        MailItem mailItem = sender as MailItem;
        if (isChecked)
        {
            listMailSelected.Add(mailItem);
        }
        else
        {
            listMailSelected.Remove(mailItem);
        }
        if (listMailSelected.Count == listMail.Count)
        {
            isCheckAll = true;
            checkAllTickImage.gameObject.SetActive(true);
        }
        else
        {
            checkAllTickImage.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Button
    public void OnClickCheckAll()
    {
        isCheckAll = !isCheckAll;
        checkAllTickImage.gameObject.SetActive(isCheckAll);
        foreach (var mailItem in listMail)
        {
            if (isCheckAll)
            {
                mailItem.TickCheckbox();
                listMailSelected.Add(mailItem);
            }
            else
            {
                mailItem.UntickCheckbox();
                listMailSelected.Remove(mailItem);
            }
        }
    }

    public void OnClickAdd()
    {
        // Handle add button click
        Debug.Log("Add button clicked");
    }

    public void OnClickDelete()
    {
        // Handle add button click
        _ = HandleClickDelete();
    }

    public async Task HandleClickDelete()
    {
        foreach (var mailItem in listMailSelected)
        {
            Destroy(mailItem.gameObject);
            listMail.Remove(mailItem);
        }
        if (isCheckAll)
        {
            await DataSender.DeleteAllNotifications();
        }
        else
        {
            foreach (MailItem mailItem in listMailSelected)
            {
                long notiId = mailItem.GetNotificationId();
                await DataSender.DeleteNotification(notiId);
            }

        }
        listMailSelected.Clear();
        isCheckAll = false;
        checkAllTickImage.gameObject.SetActive(false);
    }
    #endregion

    #region Data
    private async UniTask LoadListMail()
    {
        foreach (var mailItem in listMail)
        {
            Destroy(mailItem.gameObject);
        }
        try
        {
            ListNotification listNotification = await DataSender.GetListNotification();
            this.listNotification = listNotification.Notifications.ToList();
            checkAllButton.gameObject.SetActive(this.listNotification.Count > 0);
            Debug.Log("GAME LIST: " + listNotification.ToString());
            foreach (Notification notification in this.listNotification)
            {
                MailItem mailItem = Instantiate(mailItemPrefab, mailItemParent).GetComponent<MailItem>();
                mailItem.SetData(notification);
                mailItem.OnCheckboxClicked += MailItem_OnCheckboxClicked;
                listMail.Add(mailItem);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("err load list noti : " + ex.Message);
        }
    }
    #endregion
}
