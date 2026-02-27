using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FriendsView : BaseView
{
    public enum SortMode
    {
        INTIMACY_POINT,
        VIP_LEVEL,
        ONLINE_STATUS
    }
    private const int PageSize = 20;

    [SerializeField] private int pageIndex = 0;
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private List<Toggle> tabs = new();
    [SerializeField] private List<CanvasGroup> pages = new();
    [SerializeField] private FriendNotificationView friendNotificationView;
    [SerializeField] private FriendMissionView friendMissionView;
    [SerializeField] private FriendChatView friendChatView;
    [SerializeField] private FriendSendChipView friendSendChipView;
    [SerializeField] private FriendSendGiftView friendSendGiftView;
    [SerializeField] private FriendSortBox friendSortBox;
    [SerializeField] private FriendDeleteConfirmation friendDeleteConfirmation;
    [SerializeField] private FriendInviteView friendInviteView;
    [SerializeField] private FriendFortuneGiftView friendFortuneGiftView;
    [SerializeField] private FriendItemView friendItemViewPrefab;
    [SerializeField] private GameObject loadMoreButton;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private List<Transform> parents;
    [SerializeField] private List<ScrollRect> scrollRects;
    [SerializeField] private List<TextMeshProUGUI> tabBadgeLabels;
    [SerializeField] private Button buttonDelete, buttonAddMore;

    private FriendPresenter friendPresenter;
    private SortMode sortMode;
    private readonly Dictionary<int, List<FriendItem>> _tabItems = new();
    private readonly Dictionary<int, string> _tabNextCursor = new();
    private List<FriendItem> listUserToDelete = new();
    private List<FriendGiftItem> listFriendGift = new();
    private List<string> listUserIdToDelete = new();
    private bool _isLoading;
    private bool isShowingSortBox;

    public UnityEvent<int> OnPageIndexChanged;
    public FriendPresenter FriendPresenter => friendPresenter;

    private void OnValidate()
    {
        OpenPage(pageIndex);
        if (tabs != null && pageIndex < tabs.Count)
            tabs[pageIndex].SetIsOnWithoutNotify(true);
    }

    protected override void Awake()
    {
        base.Awake();
        friendPresenter = new FriendPresenter();
        friendPresenter.Init(this);
        friendSortBox.Setup(this);
        friendDeleteConfirmation.Setup(this);
        buttonDelete.gameObject.SetActive(false);
        friendSortBox.gameObject.SetActive(false);


        foreach (Toggle toggle in tabs)
        {
            toggle.onValueChanged.AddListener(CheckForTab);
            if (toggle.group == null) toggle.group = toggleGroup;
        }

        if (scrollRects[0] != null)
            scrollRects[0].onValueChanged.AddListener(OnScrollChanged);

        _ = LoadFriendsForTab(pageIndex, append: false);
        _ = GetFriendConfig();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _ = LoadFriendTabCountsAsync();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (Toggle toggle in tabs)
            toggle.onValueChanged.RemoveListener(CheckForTab);
        if (scrollRects[0] != null)
            scrollRects[0].onValueChanged.RemoveListener(OnScrollChanged);
    }

    #region API

    /// <summary>
    /// Map pageIndex (0=Friend, 1=Close, 2=Best, 3=Soulmate, 4=InviteSent, 5=InviteReceived) -> FriendTab.
    /// </summary>
    private static FriendTab PageIndexToFriendTab(int index)
    {
        int v = Mathf.Clamp(index + 1, 1, 6);
        return (FriendTab)v;
    }

    private Transform GetParentForTab(int tabIndex)
    {
        return tabIndex switch
        {
            0 => parents[0],
            1 => parents[1],
            2 => parents[2],
            3 => parents[3],
            4 => parents[4],
            5 => parents[5]
        };
    }

    public async UniTask RefreshCurrentTab()
    {
        await LoadFriendTabCountsAsync();
        await LoadFriendsForTab(pageIndex, false);
    }

    /// <summary>
    /// Load friend list theo tab: append=false là load trang đầu (cursor rỗng), append=true là load thêm (dùng nextCursor).
    /// </summary>
    
    private async UniTask LoadFriendsForTab(int tabIndex, bool append)
    {
        if (_isLoading) return;
        _isLoading = true;
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        try
        {
            FriendTab tab = PageIndexToFriendTab(tabIndex);
            string cursor = append && _tabNextCursor.TryGetValue(tabIndex, out var c) ? c : "";
            if (append && string.IsNullOrEmpty(cursor)) { _isLoading = false; if (loadingIndicator != null) loadingIndicator.SetActive(false); return; }

            FriendListResponse response = await friendPresenter.GetListFriend(tab, cursor);
            if (response == null || response.Friends == null) { _isLoading = false; if (loadingIndicator != null) loadingIndicator.SetActive(false); return; }

            if (!_tabItems.ContainsKey(tabIndex)) _tabItems[tabIndex] = new List<FriendItem>();
            if (!append) _tabItems[tabIndex].Clear();
            _tabItems[tabIndex].AddRange(response.Friends);
            _tabNextCursor[tabIndex] = response.NextCursor ?? "";

            RefreshTabContent(tabIndex);
            UpdateLoadMoreButton(tabIndex);
        }
        finally
        {
            _isLoading = false;
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
        }
    }

    private void RefreshTabContent(int tabIndex)
    {
        Debug.Log("REFRESH TAB CONTENT");
        Transform parent = GetParentForTab(tabIndex);
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != null && child.gameObject != null)
                Destroy(child.gameObject);
        }

        if (!_tabItems.TryGetValue(tabIndex, out var list) || list == null) return;

        List<FriendItem> sortedList = list.ToList(); 
        switch(sortMode)
        {
            case SortMode.INTIMACY_POINT:
                sortedList = sortedList
                    .OrderByDescending(x => x.IntimacyPoint)
                    .ToList();
                break;

            case SortMode.VIP_LEVEL:
                sortedList = sortedList
                    .OrderBy(x => x.VipLevel)
                    .ToList();
                break;

            case SortMode.ONLINE_STATUS:
                sortedList = sortedList
                    .OrderByDescending(x => x.IsOnline) // true lên trước
                    .ThenByDescending(x => x.IntimacyPoint) // optional tie-break
                    .ToList();
                break;

            default:
                break;
        }
        foreach (FriendItem friendItem in sortedList)
        {
            FriendItemView view = Instantiate(friendItemViewPrefab, parent);
            view.SetInfo(this, friendItem, tabIndex);
            view.OnClickCheck += (item) =>
            {
                if (listUserIdToDelete.Contains(item.UserId))
                {
                    listUserToDelete.Remove(item);
                    listUserIdToDelete.Remove(item.UserId);
                }
                else
                {
                    listUserToDelete.Add(item);
                    listUserIdToDelete.Add(item.UserId);
                }
                buttonDelete.gameObject.SetActive(listUserIdToDelete.Count > 0);
            };
            view.OnClickChat += (item)=>
            {
                FriendItem_OnClickChat(item).Forget();
            };
            view.OnClickSendChip += (item)=>
            {
                FriendItem_OnClickSendChip(item).Forget();
            };
            view.OnClickSendGift += (item)=>
            {
                FriendItem_OnClickSendGift(item).Forget();
            };
        }
    }

    private void UpdateLoadMoreButton(int tabIndex)
    {
        if (loadMoreButton == null) return;
        bool hasMore = _tabNextCursor.TryGetValue(tabIndex, out var cursor) && !string.IsNullOrEmpty(cursor);
        loadMoreButton.SetActive(hasMore);
    }

    private async UniTask GetFriendConfig()
    {
        AdminFriendConfigGetResponse response = await friendPresenter.GetFriendConfig();
        listFriendGift = response.Config.GiftItems.ToList();
        friendSendGiftView.Setup(this, listFriendGift);
    }

    private async UniTask FriendItem_OnClickChat(FriendItem itemView)
    {
        
        // var aChatChannelResponse =  await DataSender.GetFriendChatChannel(itemView.UserId);
        FriendChatView friendChatViewPref = Instantiate(friendChatView, transform);
        friendChatViewPref.gameObject.SetActive(true);
        await friendChatViewPref.Setup(this, itemView);
    }

    private async UniTask FriendItem_OnClickSendGift(FriendItem itemView)
    {
        
        // var aChatChannelResponse =  await DataSender.GetFriendChatChannel(itemView.UserId);
        friendSendGiftView.Show();
        friendSendGiftView.SetTextRecipientInfo(itemView);
        // await friendSendGiftView.Setup(this, itemView);
    }
    private async UniTask FriendItem_OnClickSendChip(FriendItem itemView)
    {
        friendSendChipView.Show();
        friendSendChipView.Setup(this, itemView);
    }

    /// <summary>
    /// Gọi khi vào UI Friends: chỉ lấy tab counts, cập nhật badge (nếu có tabBadgeLabels).
    /// </summary>
    private async UniTask LoadFriendTabCountsAsync()
    {
        FriendListResponse response = await friendPresenter.GetFriendTabCounts();
        if (response?.TabCounts == null) return;
        ApplyTabCounts(response.TabCounts);
    }

    private void ApplyTabCounts(MapField<int, FriendTabCount> tabCounts)
    {
        if (tabCounts == null || tabBadgeLabels == null) return;
        for (int i = 0; i < tabBadgeLabels.Count && i < 6; i++)
        {
            if (tabBadgeLabels[i] == null) continue;
            int key = i + 1;
            if (tabCounts.TryGetValue(key, out var tc))
                if (i < 4)
                {
                    tabBadgeLabels[i].text = $"({tc.Count}/{tc.Max})";
                }
                else
                {
                    tabBadgeLabels[i].text = $"{tc.Count}";
                }
            else
                tabBadgeLabels[i].text = "";
        }
    }

    // Auto load more khi scroll gần cuối list (cho tier tabs & invite tabs).
    private void OnScrollChanged(Vector2 pos)
    {
        // ScrollRect: y = 1 ở đầu, y = 0 ở cuối.
        if (_isLoading) return;

        // Không có tiếp thì thôi.
        if (!_tabNextCursor.TryGetValue(pageIndex, out var cursor) || string.IsNullOrEmpty(cursor))
            return;

        // Khi kéo gần cuối (0.05f tuỳ chỉnh).
        if (pos.y <= 0.05f)
        {
            _ = LoadFriendsForTab(pageIndex, append: true);
        }
    }

    #endregion

    #region Buttons
    public void OnClickSortButton()
    {
        if (isShowingSortBox)
            friendSortBox.Hide();
        else
            friendSortBox.Show();

        isShowingSortBox = !isShowingSortBox;
    }

    public void OnClickDeleteButton()
    {
        friendDeleteConfirmation.Show();
        friendDeleteConfirmation.SetText(listUserToDelete);
    }

    public async void ConfirmDelete()
    {
        await friendPresenter.RejectFriendRequest(listUserIdToDelete);
        await RefreshCurrentTab();
    }

    public void OnClickAddMoreButton()
    {
        friendInviteView.Show();
        // _ = LoadFriendsForTab(pageIndex, append: true);
    }

    public async void ConfirmSendGift(string userId, long itemId)
    {
        try
        {
            await friendPresenter.SendGift(userId, itemId);
            UIManager.Instance.ShowAlertDialog("Gift sent successfully!");
        }
        catch (Exception)
        {  
            UIManager.Instance.ShowAlertDialog("Error sending gift!");
            throw;
        }
    }

    public void OnClickNotification()
    {
        friendNotificationView.Show();
    }

    public void OnClickMission()
    {
        friendMissionView.Show();
    }

    public void OnClickFortuneGift()
    {
        friendFortuneGiftView.Show();
    }

    public void ChangeSortMode(SortMode sortMode)
    {
        this.sortMode = sortMode;
        RefreshTabContent(pageIndex);
    }

    public void OnClickGiftItem()
    {
        // TODO
    }

    #endregion

    #region Tabs

    private void CheckForTab(bool value)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (!tabs[i].isOn) continue;
            pageIndex = i;
            break;
        }
        OpenPage(pageIndex);
        _ = LoadFriendsForTab(pageIndex, append: false);
    }

    private void OpenPage(int index)
    {
        EnsureIndexIsInRange(index);

        for (int i = 0; i < pages.Count; i++)
        {
            bool isActivePage = i == pageIndex;
            pages[i].alpha = isActivePage ? 1.0f : 0f;
            pages[i].interactable = isActivePage;
            pages[i].blocksRaycasts = isActivePage;
        }

        if (Application.isPlaying)
            OnPageIndexChanged?.Invoke(pageIndex);
    }

    private void EnsureIndexIsInRange(int index)
    {
        if (tabs.Count == 0 || pages.Count == 0)
        {
            return;
        }
        pageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
    }

    public void JumpToPage(int index)
    {
        EnsureIndexIsInRange(index);
        tabs[pageIndex].isOn = true;
    }
    #endregion

}
