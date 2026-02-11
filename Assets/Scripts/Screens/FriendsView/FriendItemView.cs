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
    [SerializeField] private Image imageIntimacy, imageOnline, imageCheck;
    [SerializeField] private Button buttonSendChip, buttonSt, buttonSendGift, buttonChat, buttonAccept, buttonDecline, buttonCancel;
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
    }

}
