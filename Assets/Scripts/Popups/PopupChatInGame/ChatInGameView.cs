using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DG.Tweening;
using Globals;
using Nakama;
using Newtonsoft.Json;
using Proto;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UniTask = Cysharp.Threading.Tasks.UniTask;

public class ChatInGameView : BaseView
{
    [SerializeField] private ChatInGameItem messagePrefab;
    [SerializeField] private Transform messageContentParent, chatContainer, recorderContainer;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private VerticalPool verticalPoolGroup;
    [SerializeField] private MicrophoneRecorder microphoneRecorder;
    [SerializeField] private AudioSource audioSource;
    private List<PoolInfo> listPoolInfo = new();
    private ChatInGamePresenter chatInGamePresenter;
    private bool isSwitching = false;

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
            chatItem.SetInfo((ChatPayload)data.Data, audioSource, false, (cellW, cellH) =>
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
            try
            {
                byte[] voiceBytes = microphoneRecorder.GetBytes();

                long timeNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string fileName = $"voice_{User.Profile.Username}_{timeNow}.mp3";

                // 1️⃣ Get PUT URL
                PreSignPutResponse putResponse =
                    await DataSender.GetVoiceUploadPresignedUrl(fileName);

                if (string.IsNullOrEmpty(putResponse?.Url))
                    throw new Exception("Put URL is null");

                // 2️⃣ Upload
                var uploadReq = UnityWebRequest.Put(putResponse.Url, voiceBytes);
                uploadReq.SetRequestHeader("Content-Type", "audio/mpeg");
                await uploadReq.SendWebRequest();

                if (uploadReq.result != UnityWebRequest.Result.Success)
                    throw new Exception(uploadReq.error);

                if (string.IsNullOrEmpty(putResponse?.GetUrl))
                    throw new Exception("Get URL is null");
                // 4️⃣ Send chat
                await chatInGamePresenter.SendChatVoice(
                    User.Profile.Username,
                    putResponse.GetUrl
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Voice upload failed: {e}");
            }
            finally
            {
                microphoneRecorder.OnClickClose();
            }
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
        if (isSwitching) return;
        StartCoroutine(SwitchUI(true));
    }

    public void OnClickSendChatVoice()
    {
        if (isSwitching) return;
        StartCoroutine(SwitchUI(false));
    }

    IEnumerator SwitchUI(bool recorder)
    {
        isSwitching = true;

        chatContainer.gameObject.SetActive(!recorder);
        recorderContainer.gameObject.SetActive(recorder);

        yield return new WaitForSeconds(0.3f); // thời gian animation
        isSwitching = false;
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
        ChatPayload chatPayload = new ChatPayload();

        try
        {
            // Parse message content (JSON string từ server)
            if (!string.IsNullOrEmpty(message.Content))
            {
                // Parse JSON content
                var contentData = JsonConvert.DeserializeObject<ChatContentData>(message.Content);

                if (contentData != null)
                {
                    // 1. Check voice message first
                    if (!string.IsNullOrEmpty(contentData.voice_url))
                    {
                        chatPayload.IsAudio = true;
                        chatPayload.Content = contentData.voice_url; 
                    //    _ = Test(contentData.voice_url);
                    }
                    else
                    {
                        // 2. Text message
                        chatPayload.IsAudio = false;
                        chatPayload.Content = contentData.text ?? "";
                    }

                    // 3. Sender info từ sender_profile (server tự thêm)
                    if (contentData.sender_profile != null)
                    {
                        chatPayload.Name = message.Username; // Fallback to message.Username
                        chatPayload.Avatar = contentData.sender_profile.avt ?? "";
                        chatPayload.Vip = (int) contentData.sender_profile.vip_level;
                        chatPayload.Time = Utility.ConvertISOToHHMM(message.CreateTime);
                    }
                    else
                    {
                        chatPayload.Name = message.Username;
                    }

                    // 4. Sender ID
                    chatPayload.ID = message.SenderId;
                    
                    
                }
                else
                {
                    // Fallback: treat as plain text if JSON parse fails
                    chatPayload.Content = message.Content;
                    chatPayload.IsAudio = false;
                }
            }
            else
            {
                // Empty content
                chatPayload.IsAudio = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing chat message: {e.Message}\nContent: {message.Content}");
            // Fallback to basic info
            chatPayload.Content = message.Content ?? "";
            chatPayload.IsAudio = false;
        }

        return chatPayload;
    }

    private async UniTask Test(string url = "")
    {
        UnityWebRequest unityWebRequest =
            UnityWebRequest.Get(url);

        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                        

        await unityWebRequest.SendWebRequest();

        if (unityWebRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Voice download failed: {unityWebRequest.error}");
            return;
        }

        byte[] voiceBytes = unityWebRequest.downloadHandler.data;
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
                    sequence.Append(background.rectTransform.DOScale(targetScale, ANIMATION_TIME).SetEase(Ease.InBack)
                        .SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_LEFT:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveX(-Screen.width, ANIMATION_TIME)
                        .SetEase(Ease.OutSine).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_RIGHT:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveX(Screen.width, ANIMATION_TIME)
                        .SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_UP:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveY(Screen.height, ANIMATION_TIME)
                        .SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_DOWN:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveY(-Screen.height, ANIMATION_TIME)
                        .SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
            }

            sequence.AppendCallback(() => { onCompleteCallback?.Invoke(); });
        }
    }
// Data classes để parse JSON từ server
}
[Serializable]
public class ChatContentData
{
    public string text; // Text message (optional)
    public string voice_url; // Voice URL (optional)
    public ChatSenderProfile sender_profile; // Added by server hook
    public string sender_id; // Optional
    public string sender; // Optional
}

[Serializable]
public class ChatSenderProfile
{
    public string avt; // Avatar ID
    public int vip_level; // VIP level
    public string updated_at; // Timestamp (optional)
}

