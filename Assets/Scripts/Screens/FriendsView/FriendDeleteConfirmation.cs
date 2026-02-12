using System;
using System.Collections.Generic;
using System.Text;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendDeleteConfirmation : BaseView
{
    [SerializeField] private TextMeshProUGUI textDesc;
    [SerializeField] private Button buttonCancel, buttonOk;
    private FriendsView friendsView;

    public void Setup(FriendsView friendsView)
    {
        this.friendsView = friendsView;
    }

    protected override void Awake()
    {
        base.Awake();
        buttonOk.onClick.AddListener(() => OnClickOk());
        buttonCancel.onClick.AddListener(() => Hide());
    }

    public void SetText(List<FriendItem> listFriend)
    {
        StringBuilder sb = new();
        foreach(FriendItem friend in listFriend)
        {
            sb.Append($"{friend.Username} - ID: {friend.Sid}, ");
        }
        textDesc.text = $"You are deleting {sb.ToString()[..(sb.Length - 2)]} from your Friend List. Please check and confirm your action.";
    }

    public void OnClickOk()
    {
        friendsView.ConfirmDelete();
        Hide();

    }

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }
}