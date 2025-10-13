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

public class LobbyView : BaseView
{
    [SerializeField] private Avatar avatar;
    [SerializeField] private TextMeshProUGUI displayNameText, userIdText, accountChip, textTimeLeftToClaimReward;
    [SerializeField] private Image allSlotGamesImage, allGamesImage;
    [SerializeField] private Transform bigGameIconParent, miniGameIconParent, slotGameIconParent, allGamesParent, slotGamesParent, textPreviewChatWorldParent;
    [SerializeField] private GameObject gameIconPrefab, textPreviewChatWorldPrefab, videoBackground, redDotChipBonus, redDotFreeChip, redDotMail;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip videoStartSiXiang;
    private List<Game> gameList = new();
    private List<TextMeshProUGUI> listTextPreviewChatWorld = new();
    private LobbyPresenter lobbyPresenter;
    private int timeLeftToClaimReward;
    private bool canClaimCheckinBonus, isDeviceAllowed, hasReachedMaxStreak;
    private ObjectPool<TextMeshProUGUI> textPreviewChatWorldPool;
    VideoPlayer.EventHandler videoStartedListener;
    VideoPlayer.EventHandler videoEndedListener;

    protected override void Awake()
    {
        base.Awake();
        lobbyPresenter = new LobbyPresenter();
        lobbyPresenter.Init(this);
        InitPool();

        _ = LoadGames();
        _ = GetClaimableReward();
        OnClickAllGamesTab();
        UIManager.Instance.lobbyView = this;

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
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        User.OnProfileUpdated -= UpdateProfileData;
        NetworkManager.INSTANCE.OnMessageWorldReceived -= NetworkManager_OnMessageReceived;
        CheckInBonusView.OnRewardClaimed -= CheckInBonusView_OnRewardClaimed;
    }


    private void CheckInBonusView_OnRewardClaimed()
    {
        _ = GetClaimableReward();
    }


    private void NetworkManager_OnMessageReceived(Nakama.IApiChannelMessage message)
    {
        var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
        if (listTextPreviewChatWorld.Count >= 5)
        {
            textPreviewChatWorldPool.Release(listTextPreviewChatWorld[0]);
            listTextPreviewChatWorld.RemoveAt(0);
        }
        if (!string.IsNullOrEmpty(payload.content))
        {
            TextMeshProUGUI textPreview = textPreviewChatWorldPool.Get();
            textPreview.text = message.Username + ": " + payload.content;
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

        if (canClaim)
        {
            redDotChipBonus.SetActive(true);
        }
        else
        {
            redDotChipBonus.SetActive(false);
        }
        if (isDeviceAllowed && !hasReachedMaxStreak)
        {
            StartCoroutine(ClaimTimer());
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
            avatar.LoadAvatar(User.userProfile.AvatarId);
        }
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
    public void OnClickBanner() => UIManager.Instance.OpenBanner(TypeInAppMessage.HotNews);
    public void OnClickSendGift() => UIManager.Instance.OpenSendGift();
    public void OnClickSupport() => UIManager.Instance.OpenExchange();
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
