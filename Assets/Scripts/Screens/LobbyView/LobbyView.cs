using System;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using Avatar = Common.Objects.Avatar;
using System.Collections;
using UnityEngine.Pool;
using Nakama;

public class LobbyView : BaseView
{
    [SerializeField] private Avatar avatar;
    [SerializeField] private TextMeshProUGUI displayNameText, userIdText, accountChip, textTimeLeftToClaimReward, textVipFarmPercent;
    [SerializeField] private Image allSlotGamesImage, allGamesImage, imageVipFarmPercent;
    [SerializeField] private Transform bigGameIconParent, miniGameIconParent, slotGameIconParent, allGamesParent, slotGamesParent, textPreviewChatWorldParent;
    [SerializeField] private GameObject gameIconPrefab, textPreviewChatWorldPrefab, videoBackground, redDotChipBonus, redDotFreeChip, redDotMail, vipFarm;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip videoStartSiXiang;
    [Header("Area Buttons")]
    [SerializeField] private Button btnSupport;
    [SerializeField] private Button btnProfile;
    [SerializeField] private Button btnShop;
    [SerializeField] private Button btnNews;
    [SerializeField] private Button btnSetting;
    [SerializeField] private Button btnCheckinOnline;
    [SerializeField] private Button btnFreeChip;
    [SerializeField] private Button btnMail;
    [SerializeField] private Button btnRank;
    [SerializeField] private Button btnGiftCode;
    [SerializeField] private Button btnBank;
    [SerializeField] private Button btnChatWorld;
    [SerializeField] private Button btnChatTable;
    [SerializeField] private Button btnExchange;
    [SerializeField] private Button btnNewsPopup;
    [SerializeField] private Button btnNewBanner;
    [SerializeField] private Button btnQuickPlay;
    [SerializeField] private Button btnVipFarm;

    private List<Game> gameList = new();
    private List<TextMeshProUGUI> listTextPreviewChatWorld = new();
    private LobbyPresenter lobbyPresenter;
    private int timeLeftToClaimReward;
    private bool canClaimCheckinBonus, isDeviceAllowed, hasReachedMaxStreak;
    private ObjectPool<TextMeshProUGUI> textPreviewChatWorldPool;
    private Coroutine claimTimerCoroutine;
    VideoPlayer.EventHandler videoStartedListener;
    VideoPlayer.EventHandler videoEndedListener;

    protected override void Awake()
    {
        base.Awake();
        lobbyPresenter = new LobbyPresenter();
        lobbyPresenter.Init(this);
        InitPool();
        _ = GetVipFarmProgress();
        _ = LoadGames();
        OnClickAllGamesTab();
        UIManager.Instance.lobbyView = this;
        _ = CheckUserInGame();
        _ =  NetworkManager.INSTANCE.JoinWorldChat();
        _ = GetClaimableReward();
        _ = GetFreeChip();
        _ = GetHotNews();
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause && UIManager.Instance.gameView == null)
        {
            _ = GetClaimableReward();
        }
    }

    protected override void Start()
    {
        base.Start();
        UpdateProfileData();
        // User.OnProfileUpdated += UpdateProfileData;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        User.OnProfileUpdated += UpdateProfileData;
        NetworkManager.INSTANCE.OnMessageWorldReceived += NetworkManager_OnMessageReceived;
        CheckInBonusView.OnRewardClaimed += CheckInBonusView_OnRewardClaimed;
        FreeChipView.OnClaimed += FreeChipView_OnClaimed;
        VipFarmView.OnClaimed += VipFarmView_OnClaimed;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        User.OnProfileUpdated -= UpdateProfileData;
        NetworkManager.INSTANCE.OnMessageWorldReceived -= NetworkManager_OnMessageReceived;
        CheckInBonusView.OnRewardClaimed -= CheckInBonusView_OnRewardClaimed;
        FreeChipView.OnClaimed -= FreeChipView_OnClaimed;
        VipFarmView.OnClaimed -= VipFarmView_OnClaimed;
    }

    private async UniTask CheckUserInGame()
    {
        if (User.userProfile.PlayingMatch.MatchId != "")
        {
            Debug.Log($"Joining match with ID: {User.userProfile.PlayingMatch.MatchId}");
            var labelMatch = await DataSender.JoinMatch(User.userProfile.PlayingMatch.MatchId);
            if (labelMatch != null)
            {
                Config.currentGameId = labelMatch.Name;
                UIManager.Instance.HandleOpenGame(labelMatch);
            }
        }
        else
        {
            UIManager.Instance.OpenBanner(TypeInAppMessage.Banner, 0.6f);
        }
    }

    private void CheckInBonusView_OnRewardClaimed()
    {
        _ = GetClaimableReward();
    }
    
    private void FreeChipView_OnClaimed()
    {
        _ = GetFreeChip();
    }


    private void VipFarmView_OnClaimed()
    {
        _ = GetVipFarmProgress();
    }

    private void NetworkManager_OnMessageReceived(IApiChannelMessage message)
    {
        ChatPayload chatPayload = ConvertToChatPayload(message);
        if (listTextPreviewChatWorld.Count >= 5)
        {
            textPreviewChatWorldPool.Release(listTextPreviewChatWorld[0]);
            listTextPreviewChatWorld.RemoveAt(0);
        }
        if (!string.IsNullOrEmpty(chatPayload.Content))
        {
            string name = message.Username;
            if (name.Length > 10)
            {
                name = name[..7] + "...";
            }
            TextMeshProUGUI textPreview = textPreviewChatWorldPool.Get();
            textPreview.text = name + ": " + chatPayload.Content;
            listTextPreviewChatWorld.Add(textPreview);
        }
    }


    private async UniTask LoadGames()
    {
        try
        {
            GameListResponse gameListResponse = await lobbyPresenter.GetListGame();
            UIManager.Instance.HideProgressing();
            gameList = gameListResponse.Games.ToList();
            DOVirtual.DelayedCall(0f, () => UpdateUIListGame());
            Debug.Log("GAME LIST: " + gameListResponse.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log("err load list game : " + ex.Message);
            // throw;
        }
    }

    private async UniTask GetClaimableReward()
    {
        (Reward reward, bool canClaim, bool isDeviceAllowed, bool hasReachedMaxStreak) = await lobbyPresenter.GetClaimableReward();
        UIManager.Instance.HideProgressing();
        if (reward == null) return;
        timeLeftToClaimReward = (int)reward.NextClaimSec;
        canClaimCheckinBonus = canClaim;
        this.isDeviceAllowed = isDeviceAllowed;
        this.hasReachedMaxStreak = hasReachedMaxStreak;

        redDotChipBonus.SetActive(canClaim);
        Utility.AnimateRedDot(redDotChipBonus);

        if (isDeviceAllowed && !hasReachedMaxStreak)
        {
            StartClaimTimer();
        }
    }

    private void StartClaimTimer()
    {
        // Nếu đang chạy → stop trước
        if (claimTimerCoroutine != null)
        {
            StopCoroutine(claimTimerCoroutine);
            claimTimerCoroutine = null;
        }

        // Start coroutine mới
        claimTimerCoroutine = StartCoroutine(ClaimTimer());
    }

    private async UniTask GetFreeChip()
    {
        bool hasFreeChip = await lobbyPresenter.GetFreeChipList();
        redDotFreeChip.SetActive(hasFreeChip);
        Utility.AnimateRedDot(redDotFreeChip);
    }

    private async UniTask GetHotNews()
    {
        ListInAppMessage listInAppMessage = await lobbyPresenter.GetHotNews();
        Debug.Log("GetHotNews");
        foreach(InAppMessage inAppMessage in listInAppMessage.InAppMessages.ToList())
        {
        Debug.Log("LIST HOT NEW: " + inAppMessage);
            
        }
    }

    private async UniTask GetVipFarmProgress()
    {
        if (User.userProfile.VipLevel < 2)
        {
            vipFarm.SetActive(false);
            return;
        }
        else
        {
            vipFarm.SetActive(true);
            double progress = await lobbyPresenter.GetVipFarmProgress();
            textVipFarmPercent.text = (progress * 100).ToString("F2") + "%";
            imageVipFarmPercent.fillAmount = (float)progress;
        }
    }

    private void UpdateUIListGame()
    {
        // foreach (Transform child in miniGameIconParent)
        // {
        //     Destroy(child.gameObject);
        // }
        // foreach (Transform child in bigGameIconParent)
        // {
        //     Destroy(child.gameObject);
        // }
        foreach (Game game in gameList)
        {
            ItemGame itemGame = Instantiate(gameIconPrefab).GetComponent<ItemGame>();
            if (game.Code == Constants.WHOT_GAME_ID)
            {
                itemGame.gameObject.transform.SetParent(bigGameIconParent);
                itemGame.gameObject.transform.SetAsFirstSibling();
                itemGame.SetInfo(game.Code, true);
            }
            else
            {
                itemGame.gameObject.transform.SetParent(miniGameIconParent);
                itemGame.SetInfo(game.Code, false);
            }

            if (Constants.SLOT_GAMES_ID.Contains(game.Code))
            {
                ItemGame slotItemGame = Instantiate(gameIconPrefab, slotGameIconParent).GetComponent<ItemGame>();
                slotItemGame.SetInfo(game.Code, true);
            }

        }
    }

    public void UpdateProfileData()
    {
        if (User.userProfile != null)
        {
            displayNameText.text = User.userProfile.DisplayName;
            userIdText.text = "ID: " + User.userProfile.UserSid;
            accountChip.text = Utility.FormatNumber(User.userProfile.AccountChip);
            avatar.LoadAvatar(User.userProfile.AvatarId, User.userProfile.VipLevel);
        }
        UpdateFeatureButtons();
        _ = GetVipFarmProgress();
    }

    /// <summary>
    /// Cập nhật trạng thái hiển thị của các buttons dựa trên allowedFeatures từ server
    /// Gọi method này khi Profile được cập nhật hoặc khi cần refresh UI
    /// </summary>
    private void UpdateFeatureButtons()
    {
        if (btnSupport != null)
            btnSupport.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureSupport));
        
        if (btnProfile != null)
            btnProfile.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureProfile));
        
        if (btnShop != null)
            btnShop.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureShop));
        
        if (btnNews != null)
            btnNews.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureNews));
        
        if (btnSetting != null)
            btnSetting.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureSetting));
        
        if (btnCheckinOnline != null)
            btnCheckinOnline.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureCheckInOnline));
        
        if (btnFreeChip != null)
            btnFreeChip.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureFreeChip));
        
        if (btnMail != null)
            btnMail.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureMail));
        
        if (btnRank != null)
            btnRank.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureRank));
        
        if (btnGiftCode != null)
            btnGiftCode.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureGiftCode));
        
        if (btnBank != null)
            btnBank.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureBank));
        
        if (btnChatWorld != null)
            btnChatWorld.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureChatWorld));
        
        if (btnChatTable != null)
            btnChatTable.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureChatTable));
        
        if (btnExchange != null)
            btnExchange.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureExchange));
        
        if (btnNewsPopup != null)
            btnNewsPopup.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureTinNongPopup));
        
        if (btnNewBanner != null)
            btnNewBanner.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureNewBanner));
        
        if (btnQuickPlay != null)
            btnQuickPlay.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureQuickPlay));
        
        if (btnVipFarm != null)
            btnVipFarm.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureVipFarm));
        
    }

    private IEnumerator ClaimTimer()
    {
        while (timeLeftToClaimReward >= 0)
        {
            yield return new WaitForSeconds(1f);
            textTimeLeftToClaimReward.text = Utility.ConvertTimeToString(timeLeftToClaimReward);
            timeLeftToClaimReward -= 1;
            if (!canClaimCheckinBonus && isDeviceAllowed && !hasReachedMaxStreak)
            {
                if (timeLeftToClaimReward < 0)
                {
                    redDotChipBonus.SetActive(true);
                }
                else
                {
                    redDotChipBonus.SetActive(false);
                }
            }

        }
    }

    #region Buttons
    public void OnClickAllGamesTab()
    {
        allGamesImage.gameObject.SetActive(true);
        allSlotGamesImage.gameObject.SetActive(false);
        allGamesParent.gameObject.SetActive(true);
        slotGamesParent.gameObject.SetActive(false);
    }

    public void OnClickAllSlotsTab()
    {
        allGamesImage.gameObject.SetActive(false);
        allSlotGamesImage.gameObject.SetActive(true);
        allGamesParent.gameObject.SetActive(false);
        slotGamesParent.gameObject.SetActive(true);
    }
    public void OnClickProfile() => UIManager.Instance.OpenProfile();
    public void OnClickLuckyNumber() => UIManager.Instance.OpenLuckyNumber();
    public void OnClickLeaderboard() => UIManager.Instance.OpenLeaderboard();
    public void OnClickFreeChips() => UIManager.Instance.OpenFreeChips();
    public void OnClickShop() => UIManager.Instance.OpenShop();
    public void OnClickMail() => UIManager.Instance.OpenMail();
    public void OnClickCheckInBonus() => UIManager.Instance.OpenCheckInBonus();
    public void OnClickFriend() => UIManager.Instance.OpenFriend();
    public void OnClickChatWorld() => UIManager.Instance.OpenChatWorld();
    public void OnClickSetting() => UIManager.Instance.OpenSetting();
    public void OnClickGiftCode() => UIManager.Instance.OpenGiftCode();
    public void OnClickBanner() => UIManager.Instance.OpenBanner(TypeInAppMessage.Event);
    public void OnClickSendGift() => UIManager.Instance.OpenSendGift();
    public void OnClickSupport() => UIManager.Instance.OpenSupport();
    public void OnClickVipFarm() => UIManager.Instance.OpenVipFarm();
    public void OnClickTimeScale()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 4;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
    #endregion

    public void PlayVideoSiXiang(Match labelMatch)
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.clip = videoStartSiXiang;
            videoBackground.SetActive(false);
            videoBackground.GetComponent<RawImage>().color = new Color32(255, 255, 225, 0);
            videoPlayer.gameObject.SetActive(true);

            videoPlayer.Play();
            videoStartedListener = delegate
            {
                videoBackground.SetActive(true);
                videoBackground.GetComponent<RawImage>().color = new Color32(255, 255, 225, 255);
                videoPlayer.started -= videoStartedListener;
            };
            videoPlayer.started += videoStartedListener;

            DOTween.Sequence().AppendInterval(1.5f).AppendCallback(() =>
            {
                UIManager.Instance.HandleOpenGame(labelMatch);
            }).AppendInterval(1.1f).AppendCallback(() =>
            {
                videoBackground.SetActive(false);
                videoPlayer.gameObject.SetActive(false);
                videoPlayer.loopPointReached -= videoEndedListener;
            });
        }

    }

    private ChatPayload ConvertToChatPayload(IApiChannelMessage message)
    {
        ContentData data = JsonUtility.FromJson<ContentData>(message.Content);
        Debug.Log("SENDER AVATAR:" + data.sender_profile.avt);
        ChatPayload chatPayload = new()
        {
            ID = message.SenderId,
            Name = message.Username,
            Time = Utility.ConvertISOToHHMMDDMMYYYY(message.CreateTime),
            Content = data.content,
            Avatar = data.sender_profile.avt,
            Vip = data.sender_profile.vip_level
        };
        return chatPayload;
    }
    

    private void InitPool()
    {
        textPreviewChatWorldPool = new ObjectPool<TextMeshProUGUI>(
            createFunc: () =>
            {
                var text = Instantiate(textPreviewChatWorldPrefab, textPreviewChatWorldParent).GetComponent<TextMeshProUGUI>();
                text.gameObject.SetActive(false); // bắt đầu ẩn
                return text;
            },
            actionOnGet: (text) =>
            {
                text.gameObject.SetActive(true);
                text.transform.localScale = Vector3.one;
            },
            actionOnRelease: (text) =>
            {
                text.gameObject.SetActive(false);
            },
            actionOnDestroy: (text) =>
            {
                Destroy(text.gameObject);
            },
            collectionCheck: false,  // không cần check trùng (cho nhanh)
            defaultCapacity: 5,     // số lượng khởi tạo
            maxSize: 10             // tối đa object trong pool
        );
    }
}
