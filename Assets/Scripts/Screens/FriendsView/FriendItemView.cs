using System;
using System.Collections;
using System.Collections.Generic;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class FriendItemView : MonoBehaviour
{
    public event Action<FriendItem> OnClickCheck;
    public event Action<FriendItem> OnClickChat;
    public event Action<FriendItem> OnClickSendGift;
    public event Action<FriendItem> OnClickSendChip;
    [SerializeField] private Avatar avatar;
    [SerializeField] private TextMeshProUGUI textInfo, textIntimacy, textOnline;
    [SerializeField] private Image imageIntimacy, imageOnline, imageOffline, imageCheck;
    [SerializeField] private Button buttonSendChip, buttonSt, buttonSendGift, buttonChat, buttonAccept, buttonDecline, buttonCancel, buttonCheck;
    [SerializeField] private Sprite[] listIntimacyImage; // 0: Yellow, 1: Blue, 2: Red
    private bool isSelected;
    private FriendsView friendsView;
    private FriendItem itemView;
    private List<string> userIds = new();

    private void Awake()
    {
        buttonCheck.onClick.AddListener(OnCheckClicked);
        buttonChat.onClick.AddListener(OnChatClicked);
        buttonSendChip.onClick.AddListener(OnSendChipClicked);
        buttonSendGift.onClick.AddListener(OnSendGiftClicked);
        buttonAccept.onClick.AddListener(() => OnClickAccept());
        buttonDecline.onClick.AddListener(() => OnClickDecline());
        buttonCancel.onClick.AddListener(() => OnClickCancel());
    }

    private void OnCheckClicked()
    {
        OnClickCheck?.Invoke(itemView);
        imageCheck.gameObject.SetActive(!imageCheck.gameObject.activeSelf);
    }

    private void OnChatClicked()
    {
        OnClickChat?.Invoke(itemView);
    }

    private void OnSendGiftClicked()
    {
        OnClickSendGift?.Invoke(itemView);
    }

    private void OnSendChipClicked()
    {
        OnClickSendChip?.Invoke(itemView);
    }

    private void OnDisable()
    {
        OnClickChat = null;
        buttonChat.onClick.RemoveListener(OnChatClicked);
    }

    public void SetInfo(FriendsView friendsView, FriendItem itemView, int tabIndex)
    {
        this.friendsView = friendsView;
        this.itemView = itemView;
        userIds.Add(itemView.UserId);
        bool isFriend = tabIndex < 4;
        avatar.LoadAvatar(itemView.AvatarId, itemView.VipLevel);
        imageCheck.gameObject.SetActive(false);
        textInfo.text = $"{itemView.Username} \nID: {itemView.Sid} \nVip Level: {itemView.VipLevel}";
        textIntimacy.text = $"{itemView.IntimacyPoint}";
        textOnline.text = itemView.IsOnline ? "Online" : "Offline";
        textOnline.gameObject.SetActive(isFriend);
        imageOnline.gameObject.SetActive(isFriend && itemView.IsOnline);
        imageOffline.gameObject.SetActive(isFriend && !itemView.IsOnline);

        switch (itemView.IntimacyStatus)
        {
            case IntimacyStatus.Normal:
                imageIntimacy.sprite = listIntimacyImage[0];
                break;
            case IntimacyStatus.Frozen:
                imageIntimacy.sprite = listIntimacyImage[1];
                break;
            case IntimacyStatus.Reduced:
                imageIntimacy.sprite = listIntimacyImage[2];
                break;
            default:
                imageIntimacy.sprite = listIntimacyImage[0];
                break;
        }

        buttonSendChip.gameObject.SetActive(isFriend);
        buttonSt.gameObject.SetActive(isFriend);
        buttonSendGift.gameObject.SetActive(isFriend);
        buttonChat.gameObject.SetActive(isFriend);
        buttonAccept.gameObject.SetActive(tabIndex == 5);
        buttonDecline.gameObject.SetActive(tabIndex == 5);
        buttonCancel.gameObject.SetActive(tabIndex == 4);
        
    }

    public async void OnClickAccept()
    {
        await friendsView.FriendPresenter.AcceptFriendRequest(userIds);
        await friendsView.RefreshCurrentTab();
    }

    public async void OnClickDecline()
    {
        await friendsView.FriendPresenter.RejectFriendRequest(userIds);
        await friendsView.RefreshCurrentTab();
    }

    public async void OnClickCancel()
    {
        await friendsView.FriendPresenter.RejectFriendRequest(userIds);   
        await friendsView.RefreshCurrentTab(); 
    }

}
