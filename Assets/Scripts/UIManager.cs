using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Popups;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using GameState = Proto.GameState;
using UnityEngine.Video;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Color = UnityEngine.Color;
using Newtonsoft.Json;

public class UIManager : Singleton<UIManager>
{
    public SpriteAtlas avatarAtlas, cardAtlas;
    [SerializeField] Sprite avtDefault, spriteToast;
    [SerializeField] TMP_FontAsset fontLabelToast;
    public SelectTableView selectTableView;
    public LobbyView lobbyView;
    public bool isShowingGlobalDialog = false;
    private Transform parentPopups, parentGames, parentBanners, parentLobby, parentLoading;
    [HideInInspector] public BaseGameView gameView;
    private GameObject currentToast;
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

    public async UniTask LoadProfileUser()
    {
        Profile profile = await DataSender.GetProfile();
        User.userProfile = profile;
        User.UpdateProfile();
        User.UpdateConfig();
    }
    
    public async UniTask LoadScene(string sceneName)
    {
        ShowProgressing();
        gameView = null;
        // Preload Scene (in Unity, use LoadSceneAsync)
        await PreloadAndLoadSceneAsync(sceneName);
    }
    
    private async UniTask PreloadAndLoadSceneAsync(string sceneName)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                await UniTask.Yield();
            }

            if (sceneName == Config.MAIN_SCENE)
            {
                await LoadProfileUser();
            }

            asyncLoad.allowSceneActivation = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUpParentTransforms();
    }

    public void ShowProgressing()
    {
        // parentLoading.GetChild(0).gameObject.SetActive(true);
        Progressing.Instance.gameObject.SetActive(true);
    }

    public void HideProgressing()
    {
        // parentLoading.GetChild(0).gameObject.SetActive(false);
        Progressing.Instance.gameObject.SetActive(false);
    }

    public void ShowGlobalDialog(string message)
    {
        GlobalDialog.Instance.gameObject.SetActive(true);
        GlobalDialog.Instance.SetInfo(message);
        isShowingGlobalDialog = true;
    }

    public void HideGlobalDialog()
    {
        GlobalDialog.Instance.gameObject.SetActive(false);
        isShowingGlobalDialog = false;
    }
    
    public Sprite GetAvatarDefault()
    {
        return avtDefault;
    }

    public void DestroyAllChildren(Transform transform)
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    
    #region Games

    public async UniTask HandleFindAndJoinMatch(int markUnit)
    {
        ShowProgressing();
        RpcFindMatchResponse response = await DataSender.FindMatch(Config.currentGameId, markUnit, true);
        if (response == null) return;
        Debug.Log("Find match response: " + response.ToString());
        if (response.Matches.Count > 0)
        {
            var labelMatch = await DataSender.JoinMatch(response.Matches[0].MatchId);
            if (Config.currentGameId == Constants.SIXIANG_GAME_ID)
            {
                lobbyView.PlayVideoSiXiang(labelMatch);
            }
            else
            {
                HandleOpenGame(labelMatch);
            }
        }
    }

    public async UniTask HandleQuickMatch()
    {
        ShowProgressing();
        RpcFindMatchResponse response = await DataSender.QuickMatch(Config.currentGameId);
        if (response == null)
        {
            HideProgressing();
            return;
        }
        Debug.Log("Quick match response: " + response.ToString());
        var labelMatch = await DataSender.JoinMatch(response.Matches[0].MatchId);
        
        if (labelMatch != null)
        {
            if (Config.currentGameId == Constants.SIXIANG_GAME_ID)
            {
                HideProgressing();
                lobbyView.PlayVideoSiXiang(labelMatch);
            }
            else
            {
                HandleOpenGame(labelMatch);
            }          
            
        }
    }
    
    public async UniTask HandleCreateMatch(string passWord, int markUnit, string customData)
    {
        ShowProgressing();
        RpcCreateMatchResponse response = await DataSender.CreateMatch(Config.currentGameId ,passWord, markUnit, customData);
        HideProgressing();
        if (response == null) return;
        Debug.Log("Create match response: " + response.ToString());
        var labelMatch = await DataSender.JoinMatch(response.MatchId);
        if (labelMatch != null)
        {
            HandleOpenGame(labelMatch);
        }
    }
    
    public void HandleOpenGame(Match labelMatch)
    {
        HideProgressing();
        
        if (gameView != null)
        {
            WhotView whotView = gameView as WhotView;
            if (whotView != null && whotView.TypeWinMore)
            {
                whotView.LoadInfoMatch(labelMatch);
                return;
            }
            Destroy(gameView.gameObject);
        }
        Debug.Log("CURRENT GAME: " + Config.currentGameId);
        _ = NetworkManager.INSTANCE.LeaveWorldChat();
        switch (Config.currentGameId)
        {
            case Constants.WHOT_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("WhotView"), parentGames).GetComponent<WhotView>();
                break;
            case Constants.BACCARAT_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("BaccaratView"), parentGames).GetComponent<BaccaratView>();
                break;
            case Constants.HK_POKER_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("HongKongPokerView"), parentGames).GetComponent<HongKongPokerView>();
                break;
            case Constants.ROULETTE_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("RouletteView"), parentGames).GetComponent<RouletteView>();
                break;
            case Constants.BLACKJACK_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("BlackjackView"), parentGames).GetComponent<BlackjackView>();
                break;
            case Constants.NOEL_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("SlotNoelView"), parentGames).GetComponent<SlotNoelView>();
                // gameView = Instantiate(LoadPrefabGame("SlotFruitView"), parentGames).GetComponent<SlotFruitView>();
                break;
            case Constants.TARZAN_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("SlotTarzanView"), parentGames).GetComponent<SlotTarzanView>();
                break;
            case Constants.FRUIT_SLOT_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("SlotFruitView"), parentGames).GetComponent<SlotFruitView>();
                break;
            case Constants.INCA_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("SlotIncaView"), parentGames).GetComponent<SlotIncaView>();
                break;
            case Constants.SIXIANG_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("SlotSixiangView"), parentGames).GetComponent<SlotSixiangView>();
                break;
            case Constants.JUICY_GARDEN_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("SlotJuicyGardenView"), parentGames).GetComponent<SlotJuicyView>();
                break;
            default:
                Debug.LogError("Unsupported game ID: " + Config.currentGameId);
                break;
        }
        gameView?.LoadInfoMatch(labelMatch);
    }

    public async UniTask HandleLeaveGame()
    {
        if (gameView != null)
        {
            if (Constants.SLOT_GAMES_ID.Contains(Config.currentGameId))
            {
                if (!gameView.CanLeaveTable)
                {
                    ShowToast("You can't return to the Lobby while a game is in progress!", 2, gameView.transform);
                    return;
                }
                await DataSender.LeaveMatch();
                // await NetworkManager.INSTANCE.LeaveRoomChat();
                await NetworkManager.INSTANCE.JoinWorldChat();
                await LoadProfileUser();
                Destroy(gameView.gameObject);
                SoundManager.Instance.PlayMusicLobby();
                
            }
            else
            {
                DataSender.SendMatchState((long) OpCodeRequest.LeaveGame, Array.Empty<byte>());
            }
        }
    }

   
    
    #endregion

    #region Popups
    public void ShowConfirmDialog(string content, Action confirmCallback = null, Action cancelCallback = null, string textOk = "OK", string textCancel = "Cancel")
    {
        DialogView dialogView = Instantiate(LoadPrefabPopup("Dialog"), parentPopups).GetComponent<DialogView>();
        dialogView.transform.localScale = Vector3.one;
        dialogView.SetContent(content);
        dialogView.ConfigConfirmButton(true, textOk, confirmCallback);
        dialogView.ConfigCancelButton(true, textCancel, cancelCallback);
    }

    public void ShowAlertDialog(string content, Action confirmCallback = null, bool isGlobal = true)
    {
        DialogView dialogView = Instantiate(LoadPrefabPopup("Dialog"), parentPopups).GetComponent<DialogView>();
        dialogView.transform.localScale = Vector3.one;
        dialogView.SetContent(content);
        dialogView.ConfigConfirmButton(true, "OK", confirmCallback);
        dialogView.ConfigCancelButton(false, "Cancel", null);
    }

    public void ShowToast(string message, float timeShow = 2, Transform parent = null)
    {
        if (currentToast != null)
        {
            return;
        }
        var compToast = Utility.CreateSprite(spriteToast);
        currentToast = compToast.gameObject; 
        compToast.transform.SetParent(parent != null ? parent : transform);
        compToast.transform.SetAsLastSibling();
        compToast.type = Image.Type.Sliced;
        compToast.rectTransform.sizeDelta = new Vector2(400, 80);
        compToast.rectTransform.localScale = Vector3.one;
        compToast.transform.localPosition = new Vector2(0, -Screen.height / 4);


        var label = Utility.CreateLabel(message, 30);
        label.rectTransform.SetParent(compToast.rectTransform);
        label.rectTransform.localScale = Vector3.one;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.font = fontLabelToast;

        if (label.preferredWidth > compToast.rectTransform.sizeDelta.x)
        {
            compToast.rectTransform.sizeDelta = new Vector2(label.preferredWidth + 100, compToast.rectTransform.sizeDelta.y);
        }

        label.rectTransform.sizeDelta = new Vector2(390, 50);
        label.transform.localPosition = new Vector2(0, 5);

        if (gameView != null)
        {
            compToast.transform.eulerAngles = gameView.transform.eulerAngles;
            if (gameView.transform.eulerAngles.z == 0)
            {
                compToast.rectTransform.anchoredPosition = new Vector3(0, -150);
            }
            else
                compToast.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
        }


        compToast.rectTransform.localScale = Vector3.zero;
        DOTween.Sequence().Append(compToast.rectTransform.DOScale(1, .5f).SetEase(Ease.OutBack)).Append(compToast.rectTransform.DOScale(0, .5f).SetEase(Ease.InBack).SetDelay(timeShow)).AppendCallback(() =>
        {
            Destroy(compToast.gameObject);
            currentToast = null;
        }).SetAutoKill(true);
    }

    public void OpenSelectTableView()
    {
        ShowProgressing();
        selectTableView = Instantiate(LoadPrefabLobby("SelectTableView"), parentGames).GetComponent<SelectTableView>();
        selectTableView.transform.localScale = Vector3.one;
    }

    public void OpenCreateTableView()
    {
        if (User.userProfile.VipLevel >= 2)
        {
            CreateTableView createTableView = Instantiate(LoadPrefabPopup("PopupCreateTable"), parentPopups).GetComponent<CreateTableView>();
            createTableView.transform.localScale = Vector3.one;
        }
        else
        {
            ShowAlertDialog("You are not eligible to use this feature.");
        }
    }

    public void OpenEnterPasswordView(out EnterPasswordView enterPasswordView)
    {
        enterPasswordView = Instantiate(LoadPrefabPopup("PopupEnterPassword"), parentPopups).GetComponent<EnterPasswordView>();
        enterPasswordView.transform.localScale = Vector3.one;
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

    public void OpenLuckyNumber()
    {
        LuckyNumberView leaderBoardView = Instantiate(LoadPrefabLobby("LuckyNumberView"), parentLobby).GetComponent<LuckyNumberView>();
        leaderBoardView.transform.localScale = Vector3.one;
    }

    public void OpenChatWorld()
    {
        ChatWorldView chatWorldView = Instantiate(LoadPrefabLobby("ChatWorldView"), parentLobby).GetComponent<ChatWorldView>();
        chatWorldView.transform.localScale = Vector3.one;
    }

    public void OpenGiftCode()
    {
        GiftCodeView giftCodeView = Instantiate(LoadPrefabPopup("PopupGiftCode"), parentPopups).GetComponent<GiftCodeView>();
        giftCodeView.transform.localScale = Vector3.one;
    }

    public void OpenSupport()
    {
        SupportView supportView = Instantiate(LoadPrefabPopup("PopupSupport"), parentPopups).GetComponent<SupportView>();
        supportView.transform.localScale = Vector3.one;
    }
    public void OpenSendGift()
    {
        SendGiftView sendGiftView = Instantiate(LoadPrefabPopup("PopupSendGift"), parentPopups).GetComponent<SendGiftView>();
        sendGiftView.transform.localScale = Vector3.one;
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

    public void OpenCheckInBonus()
    {
        CheckInBonusView checkInBonusView = Instantiate(LoadPrefabPopup("PopupCheckInBonus"), parentPopups).GetComponent<CheckInBonusView>();
        checkInBonusView.transform.localScale = Vector3.one;
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

    public ChatInGameView OpenChatInGame()
    {
        ChatInGameView chatInGameView = Instantiate(LoadPrefabPopup("PopupChatInGame"), parentPopups).GetComponent<ChatInGameView>();
        chatInGameView.transform.localScale = Vector3.zero;
        return chatInGameView;
    }

    public void OpenGroupMenu()
    {
        GroupMenuView groupMenuView = Instantiate(LoadPrefabPopup("GroupMenu"), parentGames).GetComponent<GroupMenuView>();
        groupMenuView.transform.localScale = Vector3.one;
    }

    public void OpenBanner(TypeInAppMessage type, float delay = 0.3f)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            ListBannerView bannerView = Instantiate(LoadPrefabPopup("ListBannerView"), parentBanners).GetComponent<ListBannerView>();
            bannerView.SetBannerType(type);
            bannerView.transform.localScale = Vector3.zero;
        });
    }

    public void OpenVipFarm()
    {
        VipFarmView vipFarmView = Instantiate(LoadPrefabLobby("VipFarmView"), parentLobby).GetComponent<VipFarmView>();
        vipFarmView.transform.localScale = Vector3.one;
    }

    public void OpenRule()
    {
        gameView.OpenRule();
    }

    public void OpenWebView(string url = "", string title = "")
    {
        // WebViewControl webview = Instantiate(LoadPrefabPopup("WebView"), transform).GetComponent<WebViewControl>();
        // webview.loadUrl(url, title);
        // webview.transform.SetAsLastSibling();
    }
    #endregion

    #region Helpers
    public GameObject LoadPrefab(string path)
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
        return BundleHandler.LoadSkeletonDataAsset("BundlePack/Animations/" + name);

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

    public async UniTask ReloadTableView()
    {
        if (selectTableView != null)
        {
            ShowProgressing();
            await selectTableView.GetListBet();
            OpenBanner(TypeInAppMessage.Banner, 0.6f);
        }
    }
    #endregion
}
