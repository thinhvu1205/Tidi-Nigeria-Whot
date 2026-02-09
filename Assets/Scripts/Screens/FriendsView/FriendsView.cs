using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Proto;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FriendsView : BaseView
{
    [SerializeField] private int pageIndex = 0;

    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private List<Toggle> tabs = new();
    [SerializeField] private List<CanvasGroup> pages = new();
    [SerializeField] private FriendNotificationView friendNotificationView;
    [SerializeField] private FriendMissionView friendMissionView;
    [SerializeField] private FriendChatView friendChatView;
    [SerializeField] private FriendSortBox friendSortBox;

    [SerializeField] private FriendItem friendItemPrefab;
    [SerializeField] private Transform friendParent, bestFriendParent, closeFriendParent, soulmateParent;
    private FriendPresenter friendPresenter;
    private List<FriendListItem> listFriendItem;
    private bool isShowingSortBox;

    public UnityEvent<int> OnPageIndexChanged;

    private void OnValidate()
    {
        OpenPage(pageIndex);
        tabs[pageIndex].SetIsOnWithoutNotify(true);
    }

    protected override void Awake()
    {
        base.Awake();
        friendPresenter = new();
        friendPresenter.Init(this);

        foreach(Toggle toggle in tabs)
        {
            toggle.onValueChanged.AddListener(CheckForTab);
            toggle.group = toggleGroup;
        }

        _ = GetListFriends();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach(Toggle toggle in tabs)
        {
            toggle.onValueChanged.RemoveListener(CheckForTab);
        }
    }

    #region API
    private async UniTask GetListFriends()
    {
        FriendListResponse friendListResponse = await friendPresenter.GetListFriend();
        listFriendItem = friendListResponse.Friends.ToList();

        foreach(FriendListItem friendListItem in listFriendItem)
        {
            FriendItem friendItem = Instantiate(friendItemPrefab, friendParent);
            friendItem.OnClickChat += FriendItem_OnClickChat;
        }
    }


    #region Events
    private void FriendItem_OnClickChat(FriendListItem item)
    {
        friendChatView.Show();
        friendChatView.Setup(item);
    }
    #endregion

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
        
    }

    public void OnClickAddMoreButton()
    {
        
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
        
    }

    #endregion

    #region Tabs

    private void CheckForTab(bool value)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (!tabs[i].isOn) continue;
            pageIndex = i;
        }
        OpenPage(pageIndex);
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
        {
            OnPageIndexChanged?.Invoke(pageIndex);
        }
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
