using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class FriendChatTabItem : MonoBehaviour
{
    [SerializeField] private Avatar avatar;
    [SerializeField] private TextMeshProUGUI textInfo;
    [SerializeField] private Image imageBackground;
    [SerializeField] private Sprite[] listTabOnImage; // 0: first tab, 1, other

    public void Setup(ConversationEntry item)
    {
        avatar.LoadAvatar(item.AvatarId);
        // textInfo = $"{item.Username} \nID:{item.}";
    }
}


