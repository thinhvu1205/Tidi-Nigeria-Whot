using System;
using System.Collections;
using System.Collections.Generic;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class FriendItem : MonoBehaviour
{
    public event Action<FriendListItem> OnClickChat;
    [SerializeField] private Avatar avatar;
    [SerializeField] private TextMeshProUGUI textInfo, textIntimacy, textOnline;
    [SerializeField] private Image imageIntimacy, imageOnline, imageCheck;
    [SerializeField] private Button buttonSendChip, buttonSt, buttonSendGift, buttonChat, buttonAccept, buttonDecline, buttonCancel;
    private bool isSelected;
    private FriendListItem item;

    private void Awake()
    {
        buttonChat.onClick.AddListener(OnChatClicked);
        buttonAccept.gameObject.SetActive(false);
        buttonDecline.gameObject.SetActive(false);
        buttonCancel.gameObject.SetActive(false);
    }

    private void OnChatClicked()
    {
        OnClickChat?.Invoke(item);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        OnClickChat = null;
        buttonChat.onClick.RemoveListener(OnChatClicked);
    }

    public void SetInfo(FriendListItem item)
    {
        this.item = item;
    }

}
