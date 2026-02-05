using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class AlertMessage : Singleton<AlertMessage>, IPointerClickHandler
{
    [SerializeField] TMP_Text textAlert;
    [SerializeField] RectTransform rectTfParent;
    [SerializeField] GameObject background;
    private RectTransform rectTf;
    private bool isRunningAnnouncement = false;
    private Rect parentRect;

    // Announcement Ticker variables
    private Queue<InAppMessage> announcementTickerQueue = new Queue<InAppMessage>();
    private List<InAppMessage> currentRoundAnnouncements = new List<InAppMessage>(); // Danh sách announcements trong vòng hiện tại
    private Coroutine announcementTickerCoroutine;
    private DateTime roundStartTime = DateTime.MinValue; // Thời gian bắt đầu vòng hiện tại
    private int defaultFrequencySeconds = 30; // Default frequency nếu không có trong params
    private int delayBetweenAnnouncements = 15; // Delay cố định 15s giữa các announcements trong cùng 1 vòng
    private LobbyPresenter lobbyPresenter;
    
    // URL detection regex
    private static readonly Regex UrlRegex = new Regex(@"(https?://[^\s]+)", RegexOptions.Compiled);
    
    protected override void Awake()
    {
        base.Awake();
        
        // Setup rect transforms for animation
        if (rectTfParent == null)
        {
            rectTfParent = transform.parent.GetComponent<RectTransform>();
        }
        if (rectTfParent != null)
        {
            parentRect = rectTfParent.rect;
        }
        rectTf = GetComponent<RectTransform>();
        
        // Initialize announcement ticker
        lobbyPresenter = new LobbyPresenter();
        background.SetActive(false);
        // Subscribe to network events
        if (NetworkManager.INSTANCE != null)
        {
            NetworkManager.INSTANCE.OnAnnouncementTickerUpdated += OnAnnouncementTickerUpdated;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from network events
        if (NetworkManager.INSTANCE != null)
        {
            NetworkManager.INSTANCE.OnAnnouncementTickerUpdated -= OnAnnouncementTickerUpdated;
        }
        
        // Stop coroutine
        if (announcementTickerCoroutine != null)
        {
            StopCoroutine(announcementTickerCoroutine);
            announcementTickerCoroutine = null;
        }
    }
    
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, 0);
        Camera canvasWorldCamera = null;
        if (textAlert.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasWorldCamera = textAlert.canvas.worldCamera;
        }
        var linkTaggedText = TMP_TextUtilities.FindIntersectingLink( textAlert, mousePosition, canvasWorldCamera);
            
        if (linkTaggedText != -1)
        {
            TMP_LinkInfo linkInfo = textAlert.textInfo.linkInfo[linkTaggedText];
            // OnClickedOnLinkEvent?.Invoke(linkInfo.GetLinkText());
            OnLinkClicked(linkInfo.GetLinkID(), linkInfo.GetLinkText(), linkTaggedText);
        }
    }

    /// <summary>
    /// Hiển thị announcement ticker với animation chạy ngang
    /// </summary>
    public void ShowAnnouncementTicker(string content)
    {
        if (textAlert == null || rectTfParent == null)
        {
            // Debug.LogWarning("AlertMessage: textAlert or rectTfParent is null");
            return;
        }

        // Chỉ hiển thị khi không có game view (trong lobby)
        // if (UIManager.Instance != null && UIManager.Instance.gameView != null)
        // {
        //     return;
        // }
        background.SetActive(true);
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        isRunningAnnouncement = true;
        
        // Transform content: detect links và wrap thành TMP link tags
        string processedContent = TransformLinks(content);
        textAlert.text = processedContent;

        // Setup vị trí ban đầu và kết thúc cho animation
        Vector2 posStart = new Vector2(parentRect.width / 2, textAlert.transform.localPosition.y);
        Vector2 posEnd = new Vector2(-parentRect.width / 2 - textAlert.preferredWidth, textAlert.transform.localPosition.y);

        textAlert.transform.localPosition = posStart;

        // Animation chạy ngang từ phải sang trái
        textAlert.transform.DOLocalMoveX(posEnd.x, 12.5f).OnComplete(() =>
        {
            isRunningAnnouncement = false;
            background.SetActive(false);
            // Delay 0.5s trước khi có thể hiển thị announcement tiếp theo
            DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
            {
                // Nếu queue còn announcement, coroutine sẽ tự động xử lý tiếp
            });
        });

        // Debug.Log($"Announcement Ticker displayed with animation: {content}");
    }

    /// <summary>
    /// Initialize và fetch announcements lần đầu
    /// </summary>
    public async UniTask InitializeAnnouncementTicker()
    {
        await RefreshAnnouncements();
    }

    /// <summary>
    /// Callback khi server gửi event announcement updated
    /// </summary>
    private void OnAnnouncementTickerUpdated()
    {
        _ = RefreshAnnouncements();
    }

    /// <summary>
    /// Refresh danh sách announcements từ server và filter
    /// </summary>
    public async UniTask RefreshAnnouncements()
    {
        try
        {
            ListInAppMessage listInAppMessage = await lobbyPresenter.GetAnnouncementTicker();
            // Debug.Log("GetAnnouncementTicker " + listInAppMessage);
            
            if (User.userProfile == null)
            {
                // Debug.LogWarning("User profile is null, cannot filter announcement ticker");
                return;
            }

            var userVipLevel = User.userProfile.VipLevel;
            var validAnnouncements = FilterValidAnnouncements(listInAppMessage, userVipLevel);
            
            if (validAnnouncements.Count > 0)
            {
                // Debug.Log($"Found {validAnnouncements.Count} valid announcement tickers (server already sorted by priority)");
                
                // Clear queue và thêm tất cả valid announcements
                announcementTickerQueue.Clear();
                foreach (var announcement in validAnnouncements)
                {
                    announcementTickerQueue.Enqueue(announcement);
                }
                
                // Lưu danh sách announcements của vòng hiện tại
                currentRoundAnnouncements = new List<InAppMessage>(validAnnouncements);
                
                // Start coroutine để xử lý hiển thị
                if (announcementTickerCoroutine == null)
                {
                    announcementTickerCoroutine = StartCoroutine(ProcessAnnouncementTickerQueue());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error refreshing announcements: {e}");
        }
    }

    /// <summary>
    /// Filter announcements dựa trên VIP level
    /// </summary>
    private List<InAppMessage> FilterValidAnnouncements(ListInAppMessage listInAppMessage, long userVipLevel)
    {
        var validAnnouncements = new List<InAppMessage>();
        
        foreach(InAppMessage inAppMessage in listInAppMessage.InAppMessages.ToList())
        {

            bool isVipValid = true;
            if (inAppMessage.Data?.Params != null)
            {
                if (inAppMessage.Data.Params.ContainsKey("vipMin") && 
                    int.TryParse(inAppMessage.Data.Params["vipMin"], out int vipMin))
                {
                    if (userVipLevel < vipMin)
                    {
                        // Debug.Log($"Announcement {inAppMessage.Id} VIP too low (required: {vipMin}, user: {userVipLevel})");
                        isVipValid = false;
                    }
                }
                
                if (isVipValid && inAppMessage.Data.Params.ContainsKey("vipMax") && 
                    int.TryParse(inAppMessage.Data.Params["vipMax"], out int vipMax))
                {
                    if (userVipLevel > vipMax)
                    {
                        // Debug.Log($"Announcement {inAppMessage.Id} VIP too high (required: {vipMax}, user: {userVipLevel})");
                        isVipValid = false;
                    }
                }
            }
            
            if (isVipValid)
            {
                validAnnouncements.Add(inAppMessage);
                // Debug.Log($"Valid Announcement Ticker: ID={inAppMessage.Id}, Content={inAppMessage.Data?.Params?.GetValueOrDefault("content", "N/A")}");
            }
        }
        
        return validAnnouncements;
    }

    /// <summary>
    /// Coroutine để xử lý queue announcement ticker
    /// Logic: Hiển thị tuần tự với delay 15s giữa mỗi announcement, sau khi hết 1 vòng thì check frequencySeconds
    /// </summary>
    private IEnumerator ProcessAnnouncementTickerQueue()
    {
        while (true)
        {
            // Lấy frequency từ announcement đầu tiên trong vòng (hoặc default)
            int frequencySeconds = defaultFrequencySeconds;
            if (currentRoundAnnouncements.Count > 0 && 
                currentRoundAnnouncements[0].Data?.Params != null &&
                currentRoundAnnouncements[0].Data.Params.ContainsKey("frequencySeconds") &&
                int.TryParse(currentRoundAnnouncements[0].Data.Params["frequencySeconds"], out int freq))
            {
                frequencySeconds = freq;
            }
            
            // Nếu đây là vòng mới, check rate limiting từ vòng trước
            if (roundStartTime != DateTime.MinValue)
            {
                var timeSinceRoundStart = (DateTime.UtcNow - roundStartTime).TotalSeconds;
                if (timeSinceRoundStart < frequencySeconds)
                {
                    var delaySeconds = frequencySeconds - (int)timeSinceRoundStart;
                    Debug.Log($"Rate limiting: waiting {delaySeconds} seconds before starting new round (frequency: {frequencySeconds}s)");
                    yield return new WaitForSeconds(delaySeconds);
                }
            }
            
            // Bắt đầu vòng mới
            roundStartTime = DateTime.UtcNow;
            Debug.Log($"Starting new round with {announcementTickerQueue.Count} announcements, frequency: {frequencySeconds}s");
            
            // Hiển thị tất cả announcements trong vòng
            int announcementsInRound = announcementTickerQueue.Count;
            for (int i = 0; i < announcementsInRound; i++)
            {
                if (announcementTickerQueue.Count == 0)
                    break;
                    
                var announcement = announcementTickerQueue.Dequeue();
                
                // Đợi nếu đang có animation chạy
                while (isRunningAnnouncement)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                if (announcement.Data?.Params != null && 
                    announcement.Data.Params.ContainsKey("content"))
                {
                    var content = announcement.Data.Params["content"];
                    ShowAnnouncementTicker(content);
                    Debug.Log($"Displayed announcement ticker ({i + 1}/{announcementsInRound}): {content}");
                    
                    // Đợi animation hoàn thành (12.5s animation + 0.5s delay = 13s)
                    yield return new WaitForSeconds(13f);
                    
                    // Delay 15s giữa các announcements (trừ announcement cuối cùng)
                    if (i < announcementsInRound - 1)
                    {
                        Debug.Log($"Waiting {delayBetweenAnnouncements}s before next announcement");
                        yield return new WaitForSeconds(delayBetweenAnnouncements);
                    }
                }
                else
                {
                    // Delay một chút nếu không có content
                    yield return new WaitForSeconds(1f);
                }
            }
            
            // Sau khi hết 1 vòng, quay lại thêm tất cả announcements vào queue để lặp lại
            if (currentRoundAnnouncements.Count > 0)
            {
                Debug.Log($"Round completed, re-queuing {currentRoundAnnouncements.Count} announcements for next round");
                foreach (var announcement in currentRoundAnnouncements)
                {
                    announcementTickerQueue.Enqueue(announcement);
                }
            }
            else
            {
                // Nếu không còn announcements, dừng coroutine
                Debug.Log("No more announcements, stopping ticker");
                break;
            }
        }
        
        // Clear coroutine reference khi dừng
        announcementTickerCoroutine = null;
        roundStartTime = DateTime.MinValue;
    }

    /// <summary>
    /// Clear queue và stop processing (có thể gọi khi vào game hoặc logout)
    /// </summary>
    public void ClearAnnouncementTickerQueue()
    {
        announcementTickerQueue.Clear();
        currentRoundAnnouncements.Clear();
        roundStartTime = DateTime.MinValue;
        
        if (announcementTickerCoroutine != null)
        {
            StopCoroutine(announcementTickerCoroutine);
            announcementTickerCoroutine = null;
        }
        
        // Stop animation nếu đang chạy
        if (isRunningAnnouncement && textAlert != null)
        {
            DOTween.Kill(textAlert.transform);
            isRunningAnnouncement = false;
        }
    }
    
    /// <summary>
    /// Transform links trong content thành TMP link tags
    /// </summary>
    private string TransformLinks(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Replace URLs với TMP link tags
        return UrlRegex.Replace(input, match =>
        {
            string url = CleanUrl(match.Value);
            // TMP link format: <link="url">text</link>
            // Color và underline để user biết là clickable
            return $"<link=\"{url}\"><color=#00AFFF><u>{url}</u></color></link>";
        });
    }
    
    /// <summary>
    /// Clean URL: remove trailing punctuation
    /// </summary>
    private string CleanUrl(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return raw;
        
        // Remove trailing punctuation that might be part of sentence
        return raw.TrimEnd('.', ',', '!', '?', ')', ']', '}', ';', ':');
    }
    
    /// <summary>
    /// Handle TMP link click
    /// </summary>
    private void OnLinkClicked(string linkID, string linkText, int linkIndex)
    {
        // linkID chứa URL từ tag <link="url">
        if (!string.IsNullOrEmpty(linkID))
        {
            Debug.Log($"Opening link: {linkID}");
            Application.OpenURL(linkID);
        }
        else
        {
            // Fallback: try to extract URL from linkText
            Debug.LogWarning($"Link ID is empty, trying to extract from text: {linkText}");
        }
    }
}
