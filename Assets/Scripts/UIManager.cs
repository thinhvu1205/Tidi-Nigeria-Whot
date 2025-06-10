using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Transform parentPopups, parentGames, parentBanners, parentLobby;
    private BaseView currentView;
    protected override void Awake()
    {
        base.Awake();

        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Config.UpdateConfigSettings();   
    }

    #region Games
    public void OpenGame(string game)
    {
        currentView = null;
        switch (game)
        {
            case "whot":
                currentView = Instantiate(LoadPrefabGame("Whot/WhotView"), parentGames).GetComponent<WhotView>();
                break;
            default:
                Debug.LogError("Game not found: " + game);
                break;
        }
    }
    #endregion

    #region Popups

    public void OpenShop()
    {
        ShopView shopView = Instantiate(LoadPrefabLobby("ShopView"), parentLobby).GetComponent<ShopView>();
        shopView.transform.localScale = Vector3.one;
    }

    public void OpenLoto()
    {
        LotoView lotoView = Instantiate(LoadPrefabPopup("LotoView"), parentLobby).GetComponent<LotoView>();
        lotoView.transform.localScale = Vector3.one;
    }

    public void OpenExchange()
    {
        ExchangeView exchangeView = Instantiate(LoadPrefabLobby("ExchangeView"), parentLobby).GetComponent<ExchangeView>();
        exchangeView.transform.localScale = Vector3.one;
    }

    public void OpenSelectTable()
    {
        SelectTableView selectTableView = Instantiate(LoadPrefabLobby("SelectTableView"), parentLobby).GetComponent<SelectTableView>();
        selectTableView.transform.localScale = Vector3.one;
    }
    
    public void OpenLeaderboard()
    {
        LeaderBoardView leaderBoardView = Instantiate(LoadPrefabLobby("LeaderboardView"), parentLobby).GetComponent<LeaderBoardView>();
        leaderBoardView.transform.localScale = Vector3.one;
    }
    public void OpenFreeChips()
    {
        FreeChipView freeChipView = Instantiate(LoadPrefabPopup("PopupFreechips"), parentPopups).GetComponent<FreeChipView>();
        freeChipView.transform.localScale = Vector3.one;
    }
    public void OpenMail()
    {
        MailView mailView = Instantiate(LoadPrefabPopup("PopupMail"), parentPopups).GetComponent<MailView>();
        mailView.transform.localScale = Vector3.one;
    }

    public void OpenJackpot()
    {
        JackpotView jackpotView = Instantiate(LoadPrefabPopup("PopupJackpot"), parentPopups).GetComponent<JackpotView>();
        jackpotView.transform.localScale = Vector3.one;
    }
    public void OpenSetting()
    {
        SettingsView settingsView = Instantiate(LoadPrefabPopup("PopupSettings"), parentPopups).GetComponent<SettingsView>();
        settingsView.transform.localScale = Vector3.one;
    }
    public void OpenChipOnline()
    {
        ChipOnlineView chipOnlineView = Instantiate(LoadPrefabPopup("PopupChipOnline"), parentPopups).GetComponent<ChipOnlineView>();
        chipOnlineView.transform.localScale = Vector3.one;
    }

    public void OpenFeedback()
    {
        FeedbackView feedbackView = Instantiate(LoadPrefabPopup("PopupFeedback"), parentPopups).GetComponent<FeedbackView>();
        feedbackView.transform.localScale = Vector3.one;
    }

    public void OpenChangePassword()
    {
        ChangePasswordView changePasswordView = Instantiate(LoadPrefabPopup("PopupChangePassword"), parentPopups).GetComponent<ChangePasswordView>();
        changePasswordView.transform.localScale = Vector3.one;
    }

    public void OpenChangeName()
    {
        ChangeNameView changeNameView = Instantiate(LoadPrefabPopup("PopupChangeName"), parentPopups).GetComponent<ChangeNameView>();
        changeNameView.transform.localScale = Vector3.one;
    }

    public void OpenProfile()
    {
        ProfileView profileView = Instantiate(LoadPrefabPopup("PopupProfile"), parentPopups).GetComponent<ProfileView>();
        profileView.transform.localScale = Vector3.one;
    }

    public void OpenIAP()
    {
        IAPView iapView = Instantiate(LoadPrefabPopup("PopupIAP"), parentPopups).GetComponent<IAPView>();
        iapView.transform.localScale = Vector3.one;
    }

    public void OpenFriend()
    {
        FriendsView friendsView = Instantiate(LoadPrefabLobby("FriendsView"), parentLobby).GetComponent<FriendsView>();
        friendsView.transform.localScale = Vector3.one;
    }
    #endregion

    #region Helpers
    private GameObject LoadPrefab(string path)
    {
        return Resources.Load(path) as GameObject;
    }

    public GameObject LoadPrefabPopup(string name)
    {
        return LoadPrefab("Prefabs/Popups/" + name);
    }

    public GameObject LoadPrefabLobby(string name)
    {
        return LoadPrefab("Prefabs/LobbyViews/" + name);
    }

    public GameObject LoadPrefabGame(string name)
    {
        return LoadPrefab("Prefabs/Games/" + name);
    }

    // private SkeletonDataAsset LoadSkeletonData(string path)
    // {
    //     return Resources.Load<SkeletonDataAsset>(path);
    // }
    #endregion
}
