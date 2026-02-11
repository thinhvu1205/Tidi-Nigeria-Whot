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
    public event Action<FriendItem> OnClickChat;
    [SerializeField] private Avatar avatar;
    [SerializeField] private TextMeshProUGUI textInfo, textIntimacy, textOnline;
    [SerializeField] private Image imageIntimacy, imageOnline, imageOffline, imageCheck;
    [SerializeField] private Button buttonSendChip, buttonSt, buttonSendGift, buttonChat, buttonAccept, buttonDecline, buttonCancel;
    [SerializeField] private Sprite[] listIntimacyImage; // 0: Yellow, 1: Blue, 2: Red
    private bool isSelected;
    private FriendItem itemView;

    private void Awake()
    {
        buttonChat.onClick.AddListener(OnChatClicked);
        buttonAccept.gameObject.SetActive(false);
        buttonDecline.gameObject.SetActive(false);
        buttonCancel.gameObject.SetActive(false);
    }

    private void OnChatClicked()
    {
        OnClickChat?.Invoke(itemView);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        OnClickChat = null;
        buttonChat.onClick.RemoveListener(OnChatClicked);
    }

    public void SetInfo(FriendItem itemView)
    {
        this.itemView = itemView;
        avatar.LoadAvatar(itemView.AvatarId, itemView.VipLevel);
        imageCheck.gameObject.SetActive(false);
        textInfo.text = $"{itemView.Username} \nID: {itemView.Sid} \nVip Level: {itemView.VipLevel}";
        textIntimacy.text = $"{itemView.IntimacyPoint}";
        textOnline.text = itemView.IsOnline ? "Online" : "Offline";
        imageOnline.gameObject.SetActive(itemView.IsOnline);
        imageOffline.gameObject.SetActive(!itemView.IsOnline);

        switch (itemView.IntimacyStatus)
        {
            case IntimacyStatus.Normal:
                imageIntimacy.sprite = listIntimacyImage[0];
                break;
            case IntimacyStatus.Frozen:
                imageIntimacy.sprite = listIntimacyImage[1];
                break;
            default:
                imageIntimacy.sprite = listIntimacyImage[0];
                break;
        }
    }

}
