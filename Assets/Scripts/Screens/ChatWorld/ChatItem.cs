using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;
public class ChatItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textNameLeft;
    [SerializeField] private TextMeshProUGUI textTimeLeft;
    [SerializeField] private TextMeshProUGUI textMessageLeft;
    [SerializeField] private Avatar avatarLeft;
    [SerializeField] private GameObject contentLeft;
    [SerializeField] private GameObject chatContainerLeft;
    [SerializeField] private Transform imageNarrowLeft;
    [SerializeField] private TextMeshProUGUI textNameRight;
    [SerializeField] private TextMeshProUGUI textTimeRight;
    [SerializeField] private TextMeshProUGUI textMessageRight;
    [SerializeField] private Avatar avatarRight;
    [SerializeField] private GameObject contentRight;
    [SerializeField] private GameObject chatContainerRight;
    [SerializeField] private Transform imageNarrowRight;
    [SerializeField] private GameObject groupLineLeft, groupLineRight;
    [SerializeField] private GameObject audioButtonLeft, audioButtonRight;
    private RectTransform rectTransform;
    private AudioSource audioSource;
    private ChatPayload chatContentData;
    private static ChatItem currentlyPlayingItem;
    private Button activeAudioButton;
    private Color defaultAudioBtnColor = Color.white;
    private Transform activeLineGroup;
    private const float PADDING = 12f;
    private const float EXTRA_PADDING = 10f;
    private const float MIN_WIDTH = 50f;
    private const float DEFAULT_MAX_WIDTH = 350f;
    private const float LOBBY_MAX_WIDTH = 500f;
    private bool isLobbyChat = true, isPlayingAudio = false;
    private float width, height;
    private float audioDuration = 0f;
    private float startPlayTime = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetInfo(ChatPayload data, AudioSource audioSource = null, bool isLobbyChat = true, Action<float, float> onSizeCalculated = null)
    {
        chatContentData = data;
        this.audioSource = audioSource;
        this.isLobbyChat = isLobbyChat;
        // var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
        if (string.IsNullOrEmpty(data.Content))
        {
            Destroy(gameObject);
        }
        Debug.Log("SENDER AVATAR ID: " + data.Content);
        bool isMe = data.ID == User.userProfile.UserId;
        bool isAudio = data.IsAudio;

        contentLeft.SetActive(!isMe);
        contentRight.SetActive(isMe);
        if (!isMe)
        {
            textNameLeft.text = data.Name;
            textTimeLeft.text = data.Time;
            textMessageLeft.gameObject.SetActive(!isAudio);
            groupLineLeft.SetActive(isAudio);
            audioButtonLeft.SetActive(isAudio);
            if (isAudio)
            {
                
            }
            else
            {
                textMessageLeft.text = data.Content;
                AdjustFrameContent(chatContainerLeft, textMessageLeft);
                onSizeCalculated?.Invoke(width, height);
            }
            avatarLeft.LoadAvatar(data.Avatar, data.Vip);
        }
        else
        {
            textNameRight.text = data.Name;
            textTimeRight.text = data.Time;
            textMessageRight.gameObject.SetActive(!isAudio);
            groupLineRight.SetActive(isAudio);
            audioButtonRight.SetActive(isAudio);
            if (isAudio)
            {
                
            }
            else
            {
                textMessageRight.text = data.Content;
                AdjustFrameContent(chatContainerRight, textMessageRight);
                onSizeCalculated?.Invoke(width, height);
            }
            avatarRight.LoadAvatar(data.Avatar, data.Vip);
        }
        // float textWidth = textMessage.preferredWidth;
        // float textHeight = textMessage.preferredHeight;

        // Debug.Log("TEXT HEIGHT:" + textHeight);
        // else if (textWidth + 30 < 300)
        // {
        //     rectTransform.sizeDelta = new Vector2(textWidth + 30, rectTransform.sizeDelta.y);
        // }
    }
    
    private void AdjustFrameContent(GameObject frameContent, TextMeshProUGUI messageText)
    {
        if (messageText == null || frameContent == null) return;

        messageText.textWrappingMode = TextWrappingModes.Normal;
        messageText.ForceMeshUpdate();

        float textWidth = messageText.preferredWidth;

        float maxWidth = isLobbyChat ? LOBBY_MAX_WIDTH : DEFAULT_MAX_WIDTH;
        float finalWidth = Mathf.Clamp(textWidth + PADDING, MIN_WIDTH, maxWidth);
        messageText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);
        messageText.ForceMeshUpdate();

        float finalHeight = messageText.preferredHeight + PADDING;
        width = finalWidth;
        height = finalHeight;
        RectTransform frameRect = frameContent.GetComponent<RectTransform>();
        if (frameRect != null)
        {
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth + PADDING * 6);
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight + PADDING);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight + PADDING * 3);
            
            // height = finalHeight + PADDING;
            // width = finalWidth + PADDING * 6;
        }
    }
    public void OnClickPlayAudio()
    {
        HandleClickPlayAudio();
    }
    public async void HandleClickPlayAudio()
    {
        Debug.Log("OnClickPlayAudio");
   
        // Dừng thằng đang phát nếu khác mình
        if (currentlyPlayingItem != null && currentlyPlayingItem != this)
        {
            currentlyPlayingItem.StopPlaybackAndReset();
        }

        // Gán lại
        currentlyPlayingItem = this;

        if (audioSource == null || string.IsNullOrEmpty(chatContentData.Content))
            return;

        // Nếu đang phát -> dừng và reset
        if (isPlayingAudio && audioSource.isPlaying)
        {
            StopPlaybackAndReset();
            return;
        }

        activeLineGroup = contentRight != null && contentRight.activeSelf ? groupLineRight?.transform : groupLineLeft?.transform;
        activeAudioButton = (contentRight != null && contentRight.activeSelf)
            ? audioButtonRight?.GetComponent<Button>()
            : audioButtonLeft?.GetComponent<Button>();

        if (activeAudioButton != null)
            defaultAudioBtnColor = activeAudioButton.image != null ? activeAudioButton.image.color : Color.white;

        try
        {
            
            AudioClip clip = await GetAudioClipFromUrl(chatContentData.Content);
            audioSource.clip = clip;
            if (audioSource.isPlaying) audioSource.Stop();

            audioSource.clip = clip;
            audioSource.Play();

            isPlayingAudio = true;
            startPlayTime = Time.time;
            audioDuration = clip.length > 0 ? clip.length : 0.001f; // tránh chia 0

            // change button color to active
            if (activeAudioButton != null && activeAudioButton.image != null)
                activeAudioButton.image.color = Color.green;

            // đảm bảo nhóm vạch hiện được bật
            if (activeLineGroup != null)
            {
                activeLineGroup.gameObject.SetActive(true);
            }
            
        }
        catch (Exception ex)
        {
            Debug.LogError("DoClickAudio error: " + ex.Message);
            // an toàn: reset trạng thái
            StopPlaybackAndReset();
        }
    }

    private async UniTask<AudioClip> GetAudioClipFromUrl(string url)
    {
        UnityWebRequest unityWebRequest =
            UnityWebRequest.Get(url);

        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                        

        await unityWebRequest.SendWebRequest();

        if (unityWebRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Voice download failed: {unityWebRequest.error}");
            return null;
        }

        byte[] voiceBytes = unityWebRequest.downloadHandler.data;
        int totalSamples = voiceBytes.Length / 4;
        if (totalSamples <= 0) return null;
        float[] samples = new float[totalSamples];
        Debug.Log("xem lúc mới trả về" + samples.Length / 48000);
        Buffer.BlockCopy(voiceBytes, 0, samples, 0, voiceBytes.Length);
        AudioClip aAC = AudioClip.Create("AudioClip", samples.Length, 1, 16000, false);
        Debug.Log("xem lúc mới trả về sau này" + AudioSettings.outputSampleRate);
        aAC.SetData(samples, 0);
        CreateAudioVisual(samples);
        return aAC;

    }

    private void CreateAudioVisual(float[] samples)
    {
        Transform lineGroup = activeLineGroup;
        if (lineGroup != null)
        {
            int barCount = lineGroup.childCount;
            int segmentLength = samples.Length / barCount;

            for (int i = 0; i < barCount; i++)
            {
                float max = 0f;

                for (int j = 0; j < segmentLength; j++)
                {
                    int idx = i * segmentLength + j;
                    if (idx >= samples.Length) break;
                    max = Mathf.Max(max, Mathf.Abs(samples[idx]));
                }

                float amplified = max * 20f; // thử hệ số khuếch đại 10 lần
                float normalized = Mathf.Clamp01(amplified);

                Transform bar = lineGroup.GetChild(i);

                if (bar != null)
                {
                    if (samples.Length != 0)
                    {
                        bar.localScale = new Vector3(1f, Mathf.Lerp(0.15f, 1f, normalized), 1f);
                    }
                    else
                    {
                        bar.localScale = new Vector3(1f, 0.1f, 0f);
                    }
                    // tránh quá nhỏ
                    Image img = bar.GetComponent<Image>();
                    if (img != null) img.color = Color.gray; // reset màu ban đầu
                }
            }

            lineGroup.gameObject.SetActive(true);
        }
    }

    private void StopPlaybackAndReset()
    {
        if (currentlyPlayingItem == this)
            currentlyPlayingItem = null;

        try
        {
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();
        }
        catch { }
        isPlayingAudio = false;
        startPlayTime = 0f;
        audioDuration = 0f;

        // reset nút
        if (activeAudioButton != null && activeAudioButton.image != null)
            activeAudioButton.image.color = defaultAudioBtnColor;

        // reset vạch về màu xám
        Transform lineGroup = activeLineGroup ?? (contentRight.activeSelf ? groupLineRight?.transform : groupLineLeft?.transform);
        if (lineGroup != null)
        {
            for (int i = 0; i < lineGroup.childCount; i++)
            {
                Image img = lineGroup.GetChild(i).GetComponent<Image>();
                if (img != null) img.color = defaultAudioBtnColor;
                img.transform.localScale = new Vector3(1, 0.1f, 0);
            }
        }

        activeLineGroup = null;
    }

    private void Update()
    {
        if (currentlyPlayingItem != this)
            return;
        if (isPlayingAudio && audioSource != null && audioSource.isPlaying)
        {
            float elapsed = Time.time - startPlayTime;
            Transform lineGroup = activeLineGroup ??
                (contentRight.activeSelf ? groupLineRight?.transform : groupLineLeft?.transform);
            if (lineGroup == null) return;

            int totalLines = Mathf.Max(1, lineGroup.childCount);
            float progress = Mathf.Clamp01(elapsed / audioDuration);
            int activeLines = Mathf.FloorToInt(progress * totalLines);

            for (int i = 0; i < totalLines; i++)
            {
                Image img = lineGroup.GetChild(i).GetComponent<Image>();
                if (img == null) continue;

                img.color = (i <= activeLines) ? Color.green : Color.gray;
            }
        }
        else if (isPlayingAudio && (audioSource == null || !audioSource.isPlaying))
        {
            StopPlaybackAndReset();
        }
    }

}


