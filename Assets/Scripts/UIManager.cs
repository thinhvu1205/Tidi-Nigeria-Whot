using System;
using System.Collections;
using System.Collections.Generic;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    private Transform parentPopups, parentGames, parentBanners, parentLobby, parentLoading;
    private BaseView currentView;
    private const string POPUP_PARENT_TAG = "Parent Popups";
    private const string GAME_PARENT_TAG = "Parent Games";
    private const string BANNER_PARENT_TAG = "Parent Banner";
    private const string LOBBY_PARENT_TAG = "Parent Lobby";
    private const string LOADING_PARENT_TAG = "Parent Loading";

    protected override void Awake()
    {
        base.Awake();
        SetUpParentTransforms();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Config.UpdateConfigSettings();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUpParentTransforms();
    }

    #region Games
    public void OpenGame(string game)
    {
        currentView = null;
        switch (game)
        {
            case "whot":
                currentView = Instantiate(LoadPrefabGame("Whot/WhotView"), parentGames).GetComponent<WhotView>();
                Config.currentGameView = (GameView)currentView;
                break;
            default:
                Debug.LogError("Game not found: " + game);
                break;
        }
    }

    public void HandleOpenGame()
    {
        switch (Config.currentGameId)
        {
            case Constants.WHOT_GAME_ID:
                OpenGame("whot");
                break;
            default:
                Debug.LogError("Unsupported game ID: " + Config.currentGameId);
                break;
        }
    }
    #endregion

    #region Popups
    public void OpenDialog(string content, Action confirmCallback = null, Action cancelCallback = null)
    {
        DialogView dialogView = Instantiate(LoadPrefabPopup("Dialog"), parentPopups).GetComponent<DialogView>();
        dialogView.transform.localScale = Vector3.one;
        dialogView.SetContent(content);
        dialogView.ConfigConfirmButton(true, "OK", confirmCallback);
        dialogView.ConfigCancelButton(false, "Cancel", cancelCallback);
    }

    public void OpenSelectTableView()
    {
        SelectTableView selectTableView = Instantiate(LoadPrefabLobby("SelectTableView"), parentGames).GetComponent<SelectTableView>();
        selectTableView.transform.localScale = Vector3.one;
    }
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

    public SkeletonDataAsset LoadSkeletonData(string name)
    {
        return Resources.Load<SkeletonDataAsset>("BundlePack/Anims/anim_iconGames/" + name);

    }

    private void SetUpParentTransforms()
    {
        parentPopups = GameObject.FindWithTag(POPUP_PARENT_TAG)?.transform;
        parentGames = GameObject.FindWithTag(GAME_PARENT_TAG)?.transform;
        parentBanners = GameObject.FindWithTag(BANNER_PARENT_TAG)?.transform;
        parentLobby = GameObject.FindWithTag(LOBBY_PARENT_TAG)?.transform;
        parentLoading = GameObject.FindWithTag(LOADING_PARENT_TAG)?.transform;
    }

    // private SkeletonDataAsset LoadSkeletonData(string path)
    // {
    //     return Resources.Load<SkeletonDataAsset>(path);
    // }
    #endregion
}
