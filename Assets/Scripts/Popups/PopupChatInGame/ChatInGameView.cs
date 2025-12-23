using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DG.Tweening;
using Globals;
using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatInGameView : BaseView
{
    [SerializeField] private ChatInGameItem messagePrefab;
    [SerializeField] private Transform messageContentParent, chatContainer, recorderContainer;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private VerticalPool verticalPoolGroup;
    [SerializeField] private MicrophoneRecorder microphoneRecorder;
    private List<PoolInfo> listPoolInfo = new();    
    private ChatInGamePresenter chatInGamePresenter;


    protected override void Awake()
    {
        base.Awake();
        chatInGamePresenter = new ChatInGamePresenter();
        chatInGamePresenter.Init(this);
        chatInputField.characterLimit = 200;
        // verticalPoolGroup.SetCellDataCallback<ChatData>((go, data, index) =>
        // {
        //     ChatInGameItem dataCIGI = go.GetComponent<ChatInGameItem>();
        //     dataCIGI.SetInfo(data);
        // });
        // ChatData a1 = new();
        // ChatData a2 = new();
        // ChatData a3 = new();
        // ChatData a4 = new();
        // _PoolData.Clear();
        // _PoolData.Add(a1);
        // _PoolData.Add(a2);
        // _PoolData.Add(a3);
        // _PoolData.Add(a4);
        // verticalPoolGroup.SetAdapter(_PoolData);
        // verticalPoolGroup.ReloadDataToVisibleCell();
    }

    protected override void Start()
    {
        base.Start();


    }

    protected override void OnEnable()
    {
        base.OnEnable();
        chatContainer.gameObject.SetActive(true);
        recorderContainer.gameObject.SetActive(false);
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.INSTANCE.OnMessageTableReceived -= NetworkManager_OnMessageTableReceived;
    }

    public void Init()
    {
        verticalPoolGroup.SetApplyDataCb((go, data, index) =>
        {
            ChatItem chatItem = go.GetComponent<ChatItem>();
            chatItem.SetInfo((ChatPayload)data.Data, index, true, (cellW, cellH) =>
            {
                data.SetCellWidth(verticalPoolGroup.GetComponent<RectTransform>().rect.width);
                data.SetCellHeight(cellH + 40);
            });
            // chatItem.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerWidth); 
            RectTransform childRect = chatItem.GetComponent<RectTransform>();
            childRect.anchorMin = new Vector2(0, childRect.anchorMin.y);
            childRect.anchorMax = new Vector2(1, childRect.anchorMax.y);
            childRect.offsetMin = new Vector2(0, childRect.offsetMin.y);
            childRect.offsetMax = new Vector2(0, childRect.offsetMax.y);
        }, true);

        microphoneRecorder.SetData(30, null, null, async () =>
        {
            byte[] returnedBytes;
            using (MemoryStream output = new())
            {
                using (DeflateStream deflate = new(output, System.IO.Compression.CompressionLevel.Optimal))
                    deflate.Write(microphoneRecorder.GetBytes(), 0, microphoneRecorder.GetBytes().Length);
                returnedBytes = output.ToArray();
            }
            Debug.Log("check byte " + microphoneRecorder.GetBytes().Length);
            string base64 = Convert.ToBase64String(returnedBytes);

            long timeNowInSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            List<string> splitBytes = new();
            for (int i = 0; i < base64.Length; i += 350000) splitBytes.Add(base64.Substring(i, Mathf.Min(350000, base64.Length - i)));

            if (splitBytes.Count <= 1) await chatInGamePresenter.SendChatVoice(User.userProfile.UserName, splitBytes[0]);
            else
            {
                for (int i = 0; i < splitBytes.Count; i++)
                    await chatInGamePresenter.SendChatVoice(User.userProfile.UserName, splitBytes[i], i + 1, splitBytes.Count, timeNowInSeconds);
            }
            microphoneRecorder.OnClickClose();
        });
        NetworkManager.INSTANCE.OnMessageTableReceived += NetworkManager_OnMessageTableReceived;
        
    }

    private void NetworkManager_OnMessageTableReceived(IApiChannelMessage message)
    {
        ChatPayload chatPayload = ConvertToChatPayload(message);
        if (!string.IsNullOrEmpty(chatPayload.Content))
        {
            listPoolInfo.Add(new PoolInfo { Data = chatPayload });
            verticalPoolGroup.SetControlInfo(listPoolInfo, listPoolInfo.Count - 1);

            // verticalPoolGroup.ScrollToLast(0);
            // chatWorldItem.SetInfo(message, isCurrentPlayer);
        }
    }
    
    public void OnClickMicro()
    {
        chatContainer.gameObject.SetActive(false);
        recorderContainer.gameObject.SetActive(true);    
    }

    public void OnClickSendChatVoice()
    {
        chatContainer.gameObject.SetActive(true);
        recorderContainer.gameObject.SetActive(false);    
    }

    public void OnClickSendMessage()
    {
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            _ = chatInGamePresenter.SendMessage(chatInputField.text);
            chatInputField.text = "";
        }
    }

    private ChatPayload ConvertToChatPayload(IApiChannelMessage message)
    {
        ContentData data = JsonUtility.FromJson<ContentData>(message.Content);
        ChatPayload chatPayload = new()
        {
            ID = message.SenderId,
            Name = message.Username,
            Time = Utility.ConvertISOToHHMM(message.CreateTime),
            Content = data.content,
            Avatar = data.sender_profile.avt,
            Vip = data.sender_profile.vip_level
        };
        return chatPayload;
    }

    public override void OnClickCloseButton()
    {
        Hide(false, null, true);
    }

    // public override void Show()
    // {
    //     gameObject.SetActive(true);
    //     Image background = transform.GetComponent<Image>();
    //     if (popupBackground != null)
    //     {
    //         popupBackground.gameObject.SetActive(true);
    //         popupBackground.DOKill();

    //         Sequence sequence = DOTween.Sequence();
    //         sequence.AppendCallback(() => SetStretch());

    //         switch (effectPopup)
    //         {
    //             case EFFECT_POPUP.NONE:
    //                 effectPopupReverse = EFFECT_POPUP.NONE;
    //                 SetStretch();
    //                 break;
    //             case EFFECT_POPUP.SCALE:
    //                 effectPopupReverse = EFFECT_POPUP.SCALE;
    //                 Vector3 initialScale = new(0.8f, 0.8f, 0);
    //                 Vector3 targetScale = new(1f, 1f, 0);
    //                 background.rectTransform.localScale = initialScale;
    //                 Fade();
    //                 sequence.Append(background.rectTransform.DOScale(targetScale, ANIMATION_TIME).SetEase(Ease.OutBack).SetAutoKill(true));
    //                 break;
    //             case EFFECT_POPUP.MOVE_LEFT:
    //                 // effectPopupReverse = EFFECT_POPUP.MOVE_RIGHT;
    //                 Fade();
    //                 background.transform.localPosition = new Vector3(-Screen.width, originY);
    //                 sequence.Append(background.transform.DOLocalMoveX(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
    //                 break;
    //             case EFFECT_POPUP.MOVE_RIGHT:
    //                 // effectPopupReverse = EFFECT_POPUP.MOVE_LEFT;
    //                 Fade();
    //                 background.rectTransform.localPosition = new Vector3(Screen.width, originY);
    //                 sequence.Append(background.rectTransform.DOLocalMoveX(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
    //                 break;
    //             case EFFECT_POPUP.MOVE_UP:
    //                 effectPopupReverse = EFFECT_POPUP.MOVE_DOWN;
    //                 Fade();
    //                 background.rectTransform.localPosition = new Vector3(originX, -Screen.height);
    //                 sequence.Append(background.rectTransform.DOLocalMoveY(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
    //                 break;
    //             case EFFECT_POPUP.MOVE_DOWN:
    //                 effectPopupReverse = EFFECT_POPUP.MOVE_UP;
    //                 Fade();
    //                 background.rectTransform.localPosition = new Vector3(originX, Screen.height);
    //                 sequence.Append(background.rectTransform.DOLocalMoveY(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
    //                 break;
    //         }
    //     }
    // }

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        Image background = transform.GetComponent<Image>();
        if (background != null)
        {
            background.DOKill();
            Sequence sequence = DOTween.Sequence();

            switch (effectPopupReverse)
            {
                case EFFECT_POPUP.NONE:
                    break;
                case EFFECT_POPUP.SCALE:
                    Vector3 targetScale = Vector3.zero;
                    sequence.Append(background.rectTransform.DOScale(targetScale, ANIMATION_TIME).SetEase(Ease.InBack).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_LEFT:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveX(-Screen.width, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_RIGHT:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveX(Screen.width, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_UP:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveY(Screen.height, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_DOWN:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveY(-Screen.height, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
            }
            sequence.AppendCallback(() =>
            {
                onCompleteCallback?.Invoke();
            });
        }
    }
}
