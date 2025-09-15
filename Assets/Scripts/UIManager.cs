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
  
    public LobbyView lobbyView;
    private Transform parentPopups, parentGames, parentBanners, parentLobby, parentLoading;
    [HideInInspector] public BaseGameView gameView;
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


    
    public void OpenLoginScene()
    {
        ShowProgressing();
        // Global.GameView = null;

        // Preload Scene (in Unity, use LoadSceneAsync)
        StartCoroutine(PreloadAndLoadScene(Config.LOGIN_SCENE));
    }

    private IEnumerator PreloadAndLoadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;

            // Wait until the scene is loaded
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            // Hide progress UI before activation (simulate preload complete)
            HideProgressing();

            // Activate the scene
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
        RpcFindMatchResponse response = await DataSender.QuickMatch(Config.currentGameId);
        if (response == null) return;
        Debug.Log("Quick match response: " + response.ToString());
        var labelMatch = await DataSender.JoinMatch(response.Matches[0].MatchId);
        if (labelMatch != null)
        {
            HandleOpenGame(labelMatch);
        }
    }
    
    public async UniTask HandleCreateMatch(string passWord, int markUnit, string customData)
    {
        RpcCreateMatchResponse response = await DataSender.CreateMatch(Config.currentGameId ,passWord, markUnit, customData);
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
        if (gameView != null)
        {
            Destroy(gameView.gameObject);
        }
        Debug.Log("CURRENT GAME: " + Config.currentGameId);
        switch (Config.currentGameId)
        {
            case Constants.WHOT_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("WhotView"), parentGames).GetComponent<WhotView>();
                break;
            case Constants.BACCARAT_GAME_ID:
                gameView = Instantiate(LoadPrefabGame("BaccaratView"), parentGames).GetComponent<BaccaratView>();
                break;
            case Constants.CHINESE_POKER_GAME_ID:
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

    public void HandleLeaveGame()
    {
        if (gameView != null)
        {
            if (new GameState[] { GameState.Idle, GameState.Matching, GameState.Finish }.Contains(gameView.GameState))
            {
                NetworkManager.INSTANCE.LeaveMatch();
                Destroy(gameView.gameObject);
            }
        }
    }

   
    
    #endregion

    #region Popups
    public void ShowConfirmDialog(string content, Action confirmCallback = null, Action cancelCallback = null)
    {
        DialogView dialogView = Instantiate(LoadPrefabPopup("Dialog"), parentPopups).GetComponent<DialogView>();
        dialogView.transform.localScale = Vector3.one;
        dialogView.SetContent(content);
        dialogView.ConfigConfirmButton(true, "OK", confirmCallback);
        dialogView.ConfigCancelButton(true, "Cancel", cancelCallback);
    }

    public void ShowAlertDialog(string content, Action confirmCallback = null)
    {
        DialogView dialogView = Instantiate(LoadPrefabPopup("Dialog"), parentPopups).GetComponent<DialogView>();
        dialogView.transform.localScale = Vector3.one;
        dialogView.SetContent(content);
        dialogView.ConfigConfirmButton(true, "OK", confirmCallback);
        dialogView.ConfigCancelButton(false, "Cancel", null);
    }

    public void ShowToast(string message, float timeShow = 2, Transform parent = null)
    {
        var compToast = Utility.CreateSprite(spriteToast);
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
        }).SetAutoKill(true);
    }

    public void OpenSelectTableView()
    {
        SelectTableView selectTableView = Instantiate(LoadPrefabLobby("SelectTableView"), parentGames).GetComponent<SelectTableView>();
        selectTableView.transform.localScale = Vector3.one;
    }

    public void OpenCreateTableView()
    {
        CreateTableView createTableView = Instantiate(LoadPrefabPopup("PopupCreateTable"), parentGames).GetComponent<CreateTableView>();
        createTableView.transform.localScale = Vector3.one;
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
            bannerView.transform.localScale = Vector3.one;
        });
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
    #endregion
}
