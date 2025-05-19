using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System;

public class MailView : BaseView
{
    [SerializeField] private GameObject mailItemPrefab;
    [SerializeField] private Transform mailItemParent;
    [SerializeField] private Image checkAllTickImage;
    private List<JObject> listMailData = new();
    private List<MailItem> listMailSelected = new();
    private List<MailItem> listMail = new();
    private JArray mockArray = new();
    private bool isCheckAll = false;

    protected override void Start()
    {
        base.Start();
        MockData();
        LoadListMail();
    }

    #region Event    
    private void MailItem_OnCheckboxClicked(object sender, MailItem.OnCheckboxClickedEventArgs e)
    {
        bool isChecked = e.isChecked;
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
        foreach (var mailItem in listMailSelected)
        {
            Destroy(mailItem.gameObject);
            listMail.Remove(mailItem);
        }
        listMailSelected.Clear();
        isCheckAll = false;
        checkAllTickImage.gameObject.SetActive(false);
    }
    #endregion

    #region Data
    private void LoadListMail()
    {
        foreach (var mailItem in listMail)
        {
            Destroy(mailItem.gameObject);
        }
        foreach (var mailData in mockArray)
        {
            string senderName = mailData["senderName"].ToString();
            string content = mailData["content"].ToString();
            string date = mailData["date"].ToString();
            GameObject mailItemObj = Instantiate(mailItemPrefab, mailItemParent);
            MailItem mailItem = mailItemObj.GetComponent<MailItem>();
            mailItem.SetData(senderName, content, date, "");
            mailItem.OnCheckboxClicked += MailItem_OnCheckboxClicked;
            listMail.Add(mailItem);
        }
    }

    private void MockData()
    {

        for (int i = 0; i < 5; i++)
        {
            JObject mailItem = new JObject
            {
                ["senderName"] = $"Sender {i + 1}",
                ["content"] = $"This is the content of mail #{i + 1}",
                ["date"] = System.DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd")
            };

            mockArray.Add(mailItem);
        }

        Debug.Log(mockArray.ToString());
    }
    #endregion
}
