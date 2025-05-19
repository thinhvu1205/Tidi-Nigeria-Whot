using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FriendsView : BaseView
{
    [SerializeField] private GameObject friendItemPrefab;
    [SerializeField] private Transform friendItemParent;
    [SerializeField] private Image checkAllTickImage;
    private List<JObject> listMailData = new();
    private List<FriendItem> listFriendSelected = new();
    private List<FriendItem> listFriend = new();
    private JArray mockArray = new();
    private bool isCheckAll = false;

    protected override void Start()
    {
        base.Start();
        MockData();
        LoadListFriend();
    }

    #region Data
    private void LoadListFriend()
    {
        foreach (var friendItem in listFriend)
        {
            Destroy(friendItem.gameObject);
        }
        foreach (var mailData in mockArray)
        {
            string avatar = mailData["avatar"].ToString();
            string name = mailData["name"].ToString();
            GameObject friendItemObj = Instantiate(friendItemPrefab, friendItemParent);
            FriendItem friendItem = friendItemObj.GetComponent<FriendItem>();
            friendItem.SetData(name, avatar);
            friendItem.OnCheckboxClicked += FriendItem_OnCheckboxClicked;
            listFriend.Add(friendItem);
        }
    }

    private void MockData()
    {

        for (int i = 0; i < 5; i++)
        {
            JObject friendItem = new()
            {
                ["avatar"] = "",
                ["name"] = "Minh Quan",
            };

            mockArray.Add(friendItem);
        }

        Debug.Log(mockArray.ToString());
    }
    #endregion

    #region Button
    public void OnClickCheckAll()
    {
        isCheckAll = !isCheckAll;
        checkAllTickImage.gameObject.SetActive(isCheckAll);
        foreach (var friendItem in listFriend)
        {
            if (isCheckAll)
            {
                friendItem.TickCheckbox();
                listFriendSelected.Add(friendItem);
            }
            else
            {
                friendItem.UntickCheckbox();
                listFriendSelected.Remove(friendItem);
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
        foreach (var friendItem in listFriendSelected)
        {
            Destroy(friendItem.gameObject);
            listFriend.Remove(friendItem);
        }
        listFriendSelected.Clear();
        isCheckAll = false;
        checkAllTickImage.gameObject.SetActive(false);
    }
    #endregion

    #region Event    
    private void FriendItem_OnCheckboxClicked(object sender, FriendItem.OnCheckboxClickedEventArgs e)
    {
        bool isChecked = e.isChecked;
        FriendItem friendItem = sender as FriendItem;
        if (isChecked)
        {
            listFriendSelected.Add(friendItem);
        }
        else
        {
            listFriendSelected.Remove(friendItem);
        }
        if (listFriendSelected.Count == listFriend.Count)
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
}
