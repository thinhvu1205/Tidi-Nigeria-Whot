using System.Collections;
using System.Collections.Generic;
using Proto;
using DG.Tweening;
using Games.Card;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class BasePlayerView : MonoBehaviour
{
    [Tooltip("Player name display")]
    [SerializeField] public TextMeshProUGUI textName, textMoney, textChipWinLise;
    [Tooltip("Player avatar component")]
    [SerializeField] public Avatar avatar;
    [Tooltip("Turn countdown timer")]
    [SerializeField] Image timeCountDown;
    [Tooltip("Host indicator, exit button, dealer icon, background bar")]
    [SerializeField] GameObject hostIcon, exitIcon, dealerIcon, backgroundBar;
    [SerializeField] TMP_FontAsset fontWin, fontLose;


    [Tooltip("Current turn time")]
    private float timeTurn = 0;

    [Tooltip("Total win/lose amounts")]
    public long ChipLose { get; private set; } = 0;
    public long ChipWin { get; private set; } = 0;
    public long CurrentChip { get; private set; } = 0;
    public bool IsCurrentPlayer { get; private set; } = false;


    [Header("=== ANIMATIONS ===")]
    [Tooltip("Spine animation for win/lose/draw effects")]
    [SerializeField] public SkeletonGraphic animationResult;

    [Tooltip("All-in animation effect")]
    [SerializeField] public GameObject aniAllIn, hitpot;


    [Tooltip("Pot indicators list")]
    [SerializeField] public List<GameObject> pots;

    [Header("=== ANIMATION ASSETS ===")]
    [Tooltip("Animation assets: 0-lose, 1-draw, 2-win")]
    [SerializeField] public List<SkeletonDataAsset> listAnimationResult;

    [Header("=== CARDS ===")]
    [Tooltip("Player's cards list")]
    [HideInInspector] public List<CardModel> cards = new List<CardModel>();

    [Header("=== ANIMATION SEQUENCES ===")]
    [Tooltip("Flying text animation sequence")]
    private Sequence seqTextFly;

    [Header("=== VIP SYSTEM ===")]
    [Tooltip("VIP item GameObject")]
    GameObject itemVip;

    private string _id;
    public string id
    {
        get => _id;
        set => _id = value;
    }

    private string _userName;
    public string user_name
    {
        get => _userName;
        set => _userName = value;
    }

    private string _wallet;
    public string wallet
    {
        get => _wallet;
        set => _wallet = value;
    }

    private string _avatarId;
    public string avatar_id
    {
        get => _avatarId;
        set => _avatarId = value;
    }

    private long _vipLevel;
    public long vipLevel
    {
        get => _vipLevel;
        set => _vipLevel = value;
    }

    private string _sid;
    public string sid
    {
        get => _sid;
        set => _sid = value;
    }
    
    private Coroutine countdownCoroutine;

    public void SetData(Player playerData)
    {
        id = playerData.Id;
        user_name = playerData.UserName;
        wallet = playerData.Wallet;
        avatar_id = playerData.AvatarId;
        vipLevel = playerData.VipLevel;
        sid = playerData.Sid.ToString();
        SetCurrentChip(long.Parse(wallet));
        avatar.LoadAvatar(avatar_id, vipLevel);
        // if (string.IsNullOrEmpty(avatar_id))
        // {
        //     // avatar.setSpriteFrame(UIManager.Instance.avatarAtlas.getSpriteFrame(avatar_id));
        // }
        // else
        // {
        //     avatar.SetSpriteFrame(UIManager.Instance.GetAvatarDefault());
        // }

        // if (vipLevel != 0)
        // {
        //     avatar.setVip((int)vipLevel);
        // }
        // else
        // {
        //     avatar.setVip(0);
        // }
        setName(user_name);
    }


    /// <summary>
    /// Sets player name with scrolling effect if too long
    /// </summary>
    /// <param name="namePl">Player name to display</param>
    public void setName(string namePl)
    {
        string nameText = namePl;
        if (namePl.Length > 10)
        {
            nameText = namePl.Substring(0, 7) + "..."; 
        }
        textName.text = nameText;
        // Config.EffectTextRunInMask(txtName);
    }

    /// <summary>
    /// Shows/hides all-in effect
    /// </summary>
    /// <param name="isAllIn">Whether to show all-in effect</param>
    public void setEffectAllIn(bool isAllIn = true)
    {
        aniAllIn.gameObject.SetActive(isAllIn);
    }

    /// <summary>
    /// Sets player avatar with VIP level
    /// </summary>
    /// <param name="avaId">Avatar ID</param>
    /// <param name="fname">Facebook name</param>
    /// <param name="Faid">Facebook ID</param>
    /// <param name="vip">VIP level</param>
    public void setAvatar(int avaId, string fname, string Faid, int vip)
    {
        avatar.loadAvatarAsync(avaId, fname, Faid);
        avatar.setVip(vip);
    }

    /// <summary>
    /// Updates VIP item display based on VIP level and game type
    /// </summary>
    /// <param name="idVip">VIP item ID</param>
    /// <param name="vip">VIP level</param>
    /// <param name="idPosTongits">Position for Tongits game</param>
    bool isOnItemVip = true;
    public void updateItemVip(int idVip, int vip, int idPosTongits = -1)
    {
        // Show VIP item for VIP level 5+
        if (vip >= 5 && idVip != 0)
        {
            // Create VIP item if not exists
            if (itemVip == null && isOnItemVip)
            {
                isOnItemVip = false;
                itemVip = Instantiate(UIManager.Instance.LoadPrefabGame("GameView/Objects/ItemVip"), transform);
            }

            // Calculate position based on game type
            var vecPos = itemVip.transform.localPosition;
            var vecPosThis = transform.localPosition;
            var size = gameObject.GetComponent<RectTransform>().sizeDelta;

            vecPos.x = vecPosThis.x > 0 ? 100 : -100;
            vecPos.y = -60;

            // Position for different games
            // if (Config.currentGameId == (int)GAMEID.TONGITS_OLD || Config.currentGameId == (int)GAMEID.TONGITS_JOKER || Config.currentGameId == (int)GAMEID.TONGITS)
            // {
            //     switch (idPosTongits)
            //     {
            //         case 0: vecPos.x = 55; break;
            //         case 1: vecPos.x = -100; break;
            //         case 2: vecPos.x = 100; break;
            //     }
            // }
            // else if (Config.currentGameId == (int)GAMEID.LUCKY_89)
            // {
            //     vecPos.x = vecPosThis.x < 0 ? 100 : -100;
            //     vecPos.y = -60;
            // }
            // else if (Config.currentGameId == (int)GAMEID.SICBO)
            // {
            //     vecPos.x = vecPosThis.x < 0 ? -100 : 100;
            //     vecPos.y = 0;
            // }
            // else
            // {
            //     vecPos.x = vecPosThis.x > 0 ? 100 : -100;
            //     vecPos.y = -60;
            // }

            // Activate and position VIP item
            itemVip.SetActive(true);
            itemVip.transform.localPosition = vecPos;
            updateItemVipFromSV(idVip);
            itemVip.transform.SetAsLastSibling();

            // Setup button interaction
            Button btn = itemVip.GetComponent<Button>();
            btn.interactable = (vip > 5) && IsCurrentPlayer;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { onClickSelectItemVip(vip); });
        }
        else
        {
            if (itemVip != null) itemVip.SetActive(false);
        }
    }

    /// <summary>
    /// Cleanup when object is disabled
    /// </summary>
    public void OnDisable()
    {
        OnDestroy();
    }

    /// <summary>
    /// Cleanup VIP items when destroyed
    /// </summary>
    public void OnDestroy()
    {
        if (itemVip != null)
        {
            Destroy(itemVip.gameObject);
        }
        itemVip = null;
        isOnItemVip = true;

        if (BkgVip != null)
        {
            Destroy(BkgVip.gameObject);
        }
        BkgVip = null;
    }

    /// <summary>
    /// Updates VIP item animation from server data
    /// </summary>
    /// <param name="idItem">VIP item ID from server</param>
    public void updateItemVipFromSV(int idItem)
    {
        Debug.Log("idItem  " + idItem);
        var itemIdVip = idItem / 10;
        if (itemIdVip > 10) itemIdVip = 10;
        Debug.Log("itemIdVip  " + itemIdVip);

        if (itemVip != null && itemIdVip >= 5)
        {
            GameObject animGO = itemVip.transform.Find("anim").gameObject;
            if (animGO != null)
            {
                SkeletonGraphic animSG = animGO.GetComponent<SkeletonGraphic>();
                if (animSG != null) animSG.AnimationState.SetAnimation(0, itemIdVip.ToString(), true);
            }
        }
    }

    /// <summary>
    /// VIP background for item selection
    /// </summary>
    Transform BkgVip;

    /// <summary>
    /// Handles VIP item selection click
    /// </summary>
    /// <param name="vip">VIP level</param>
    void onClickSelectItemVip(int vip)
    {
        if (itemVip != null && IsCurrentPlayer)
        {
            // Create VIP background if not exists
            if (BkgVip == null)
            {
                BkgVip = Instantiate(UIManager.Instance.LoadPrefabGame("GameView/Objects/BkgItemVip"), UIManager.Instance.gameView.transform).transform;

                // Setup VIP item buttons
                for (var i = 0; i < BkgVip.childCount; i++)
                {
                    var index = i;
                    var item = BkgVip.GetChild(i);
                    if (index + 5 <= vip)
                    {
                        item.gameObject.SetActive(true);
                        item.GetComponent<Button>().onClick.RemoveAllListeners();
                        item.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            onClickItemVip(index + 5);
                        });
                    }
                    else
                    {
                        item.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                BkgVip.gameObject.SetActive(!BkgVip.gameObject.activeSelf);
            }

            // Animate VIP background
            if (BkgVip.gameObject.activeSelf)
            {
                BkgVip.SetAsLastSibling();
                var scale = itemVip.transform.localScale.x;
                BkgVip.localScale = Vector3.zero;

                DOTween.Sequence().AppendInterval(0.2f).AppendCallback(() =>
                {
                    var sizeBkg = BkgVip.GetComponent<RectTransform>().sizeDelta;
                    var vecPosThis = transform.localPosition;
                    var posBkg = BkgVip.localPosition;

                    // Position based on item position
                    if (itemVip.transform.localPosition.x < 0)
                    {
                        posBkg.x = vecPosThis.x - 100;
                    }
                    else
                    {
                        posBkg.x = vecPosThis.x + 100;
                    }
                    posBkg.y = vecPosThis.y - 30;
                    // Special positioning for Sicbo
                    // if (Config.currentGameId == (int)Globals.GAMEID.SICBO)
                    // {
                    //     posBkg.y = vecPosThis.y - sizeBkg.y * scale - 30;
                    // }
                    // else
                    //     posBkg.y = vecPosThis.y - 30;

                    BkgVip.localPosition = posBkg;
                    BkgVip.DOScale(itemVip.transform.localScale, .2f);
                });
            }
        }
    }

    /// <summary>
    /// Handles VIP item selection
    /// </summary>
    /// <param name="vip">Selected VIP level</param>
    void onClickItemVip(int vip)
    {
        // SocketSend.sendUpdateItemVip(vip);
        if (BkgVip != null)
        {
            BkgVip.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets player money with tween animation
    /// </summary>
    /// <param name="ag">New money amount</param>
    public void SetCurrentChip(long ag)
    {
        // Utility.TweenNumberTo(textMoney, ag, CurrentChip, 0.3f, false, false);
        Utility.TweenNumberToNumberScale1(textMoney, ag, (int)CurrentChip, 0.5f, false);

        CurrentChip = ag;
    }

    /// <summary>
    /// Sets avatar click callback
    /// </summary>
    /// <param name="callback">Click callback function</param>
    public void setCallbackClick(System.Action callback)
    {
        var btnCom = avatar.gameObject.GetComponent<Button>();
        if (btnCom == null)
        {
            btnCom = avatar.gameObject.AddComponent<Button>();
        }
        btnCom.onClick.RemoveAllListeners();
        btnCom.onClick.AddListener(() =>
        {
            Debug.Log("Click avatar");
            callback();
        });
    }

    /// <summary>
    /// Sets position for current player's progress bar
    /// </summary>
    public void SetPositionInfoThisPlayer()
    {

        if (Config.currentGameId == Constants.HK_POKER_GAME_ID)
        {
            return;
        }
        backgroundBar.transform.localPosition = new Vector2(120, -12);
        // if (
        //     Globals.Config.currentGameId == (int)Globals.GAMEID.TONGITS_JOKER ||
        //     Globals.Config.currentGameId == (int)Globals.GAMEID.TONGITS ||
        //     Globals.Config.currentGameId == (int)Globals.GAMEID.TONGITS_OLD
        // )
        // {
        //     bkgThanhBar.transform.localPosition = new Vector2(-120, 5);
        //     bkgThanhBar.GetComponent<Image>().enabled = false;
        //     txtMoney.fontSize = 23;
        //     txtName.fontSize = 26;
        //     txtName.gameObject.transform.parent.transform.localPosition = new Vector2(txtName.gameObject.transform.parent.transform.localPosition.x, 15);
        //     return;
        // }
        // else if (Globals.Config.currentGameId == (int)Globals.GAMEID.SICBO)
        // {
        //     bkgThanhBar.transform.localPosition = new Vector2(113, -20);
        // }
        // else
        // {
        //     bkgThanhBar.transform.localPosition = new Vector2(120, -12);
        // }
    }

    public void SetPositionInfoRightPlayer()
    {
        backgroundBar.transform.localPosition = new Vector2(-120, 0);
    }

    /// <summary>
    /// Sets player turn with countdown timer
    /// </summary>
    /// <param name="isTurn">Is player's turn</param>
    /// <param name="_timeTurn">Turn duration</param>
    /// <param name="_isMe">Is current player</param>
    /// <param name="timeVibrate">Vibration time before turn ends</param>
    public virtual void SetCurrentTurn(bool isTurn, float _timeTurn = 0f, float _totalTimeTurn = 0, bool _isMe = false, float timeVibrate = 5f)
    {
        // Tắt hoặc bật countdown UI
        timeCountDown.gameObject.SetActive(isTurn);

        // Nếu có coroutine đang chạy thì dừng lại trước
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        // Nếu là lượt của người chơi → bắt đầu countdown mới
        if (isTurn)
        {
            countdownCoroutine = StartCoroutine(FillAmountToZero());
        }

        // ------------------------------
        // Coroutine fill countdown timer
        IEnumerator FillAmountToZero()
        {
            timeTurn = _timeTurn;
            timeCountDown.fillAmount = timeTurn / _totalTimeTurn;

            // Hiệu ứng scale nhỏ khi bắt đầu lượt
            avatar.transform.DOScale(1.1f * Vector2.one, .1f)
                .OnComplete(() => avatar.transform.DOScale(Vector2.one, .1f));

            float elapsedTime = 0;

            while (timeCountDown.fillAmount > 0)
            {
                yield return new WaitForFixedUpdate();

                timeCountDown.fillAmount -= Time.fixedDeltaTime / _totalTimeTurn;
                elapsedTime += Time.fixedDeltaTime;

                // Nếu countdown bị tắt giữa chừng thì dừng luôn
                if (!timeCountDown.gameObject.activeSelf)
                    yield break;

                // Rung gần hết thời gian nếu là người chơi hiện tại
                if (_isMe && (elapsedTime >= timeTurn - timeVibrate))
                {
                    Config.Vibration();
                    elapsedTime = -99; // để không rung nhiều lần
                }
            }

            // Khi kết thúc countdown, xóa coroutine handle
            countdownCoroutine = null;
        }
    }

    public virtual void HideCountDown()
    {
        timeCountDown.gameObject.SetActive(false);
    }

    /// <summary>
    /// Checks if it's player's turn
    /// </summary>
    /// <returns>True if player's turn</returns>
    public bool IsCurrentTurn()
    {
        return timeCountDown.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Gets avatar sprite
    /// </summary>
    /// <returns>Avatar sprite</returns>
    public Sprite GetAvatarSprite()
    {
        return avatar.imageAvatar.sprite;
    }

    public Vector2 GetAvatarPosition()
    {
        return avatar.transform.localPosition;
    }

    public Transform GetAvatarTransform()
    {
        return avatar.transform;
    }

    /// <summary>
    /// Sets avatar dark mode
    /// </summary>
    /// <param name="isDark">Dark mode enabled</param>
    public void setDark(bool isDark)
    {
        avatar.setDark(isDark);
    }

    /// <summary>
    /// Shows win effect animation
    /// </summary>
    /// <param name="animName">Animation name (default: win)</param>
    /// <param name="isLoop">Loop animation</param>
    public virtual void SetEffectWin(string animName = "win", bool isLoop = true)
    {
        animationResult.gameObject.SetActive(true);
        animationResult.skeletonDataAsset = listAnimationResult[2];
        animationResult.Initialize(true);

        animationResult.AnimationState.SetAnimation(0, animName, isLoop);
        if (!isLoop)
        {
            animationResult.AnimationState.Complete += delegate
            {
                animationResult.gameObject.SetActive(false);
            };
        }
    }

    /// <summary>
    /// Shows lose effect animation
    /// </summary>
    /// <param name="isLoop">Loop animation</param>
    public virtual void SetEffectLose(string animationName = "lose", bool isLoop = true)
    {
        animationResult.TrimRenderers();
        animationResult.gameObject.SetActive(true);
        animationResult.skeletonDataAsset = listAnimationResult[0];
        animationResult.Initialize(true);
        animationResult.AnimationState.SetAnimation(0, animationName, isLoop);

        if (isLoop == false)
        {
            animationResult.AnimationState.Complete += delegate
            {
                animationResult.gameObject.SetActive(false);
            };
        }
    }

    /// <summary>
    /// Shows draw effect animation
    /// </summary>
    /// <param name="isLoop">Loop animation</param>
    public void SetEffectDraw(string animationName = "draw", bool isLoop = true)
    {
        animationResult.gameObject.SetActive(true);
        animationResult.skeletonDataAsset = listAnimationResult[1];
        animationResult.Initialize(true);
        animationResult.AnimationState.SetAnimation(0, animationName, true);

        if (isLoop == false)
        {
            animationResult.AnimationState.Complete += delegate
            {
                animationResult.gameObject.SetActive(false);
            };
        }
    }

    /// <summary>
    /// Sets player ready state
    /// </summary>
    /// <param name="isReady">Ready state</param>
    public void setReady(bool isReady)
    {
        avatar.setDark(!isReady);
    }

    /// <summary>
    /// Shows/hides host indicator
    /// </summary>
    /// <param name="isHost">Is host</param>
    public void setHost(bool isHost)
    {
        hostIcon.SetActive(isHost);
    }

    /// <summary>
    /// Shows/hides exit button
    /// </summary>
    /// <param name="isExit">Show exit</param>
    public void setExit(bool isExit)
    {
        exitIcon.SetActive(isExit);
    }

    /// <summary>
    /// Shows flying money effect with animation
    /// </summary>
    /// <param name="mo">Money amount</param>
    /// <param name="fonzSize">Font size</param>
    public void AnimateFlyMoney(long mo, int fonzSize = 50)
    {
        if (mo == 0) return;

        textChipWinLise.fontSize = fonzSize;
        if (mo < 0)
        {
            textChipWinLise.font = fontLose;
            textChipWinLise.text = Utility.FormatMoney2(mo, true, true);
        }
        else
        {
            textChipWinLise.font = fontWin;
            textChipWinLise.text = "+" + Utility.FormatMoney2(mo, true, true);
        }

        textChipWinLise.transform.localPosition = Vector2.zero;
        int height = 100;

        // Game-specific height adjustments
        // if (Config.currentGameId == (int)Globals.GAMEID.SICBO && transform.localPosition.y > 280)
        // {
        //     height = 50;
        // }
        // if (Config.currentGameId == (int)Globals.GAMEID.PUSOY)
        // {
        //     lbChipWinLose.transform.localPosition = new Vector2(0, -30);
        //     height = 50;
        // }
        if (Config.currentGameId == Constants.BACCARAT_GAME_ID)
        {
            height = 60;
        }
        // if (Config.currentGameId == (int)GAMEID.LUCKY9)
        // {
        //     height = 35;
        // }

        textChipWinLise.gameObject.SetActive(true);
        if (seqTextFly != null)
        {
            seqTextFly.Kill();
        }
        seqTextFly = DOTween.Sequence()
             .Append(textChipWinLise.transform.DOLocalMove(new Vector2(0, height), 2.0f).SetEase(Ease.OutBack))
             .AppendInterval(1.0f)
             .AppendCallback(() =>
             {
                 textChipWinLise.gameObject.SetActive(false);
             });
    }

    /// <summary>
    /// Shows dealer icon with animation
    /// </summary>
    /// <param name="isShow">Show dealer</param>
    /// <param name="isLeft">Position left</param>
    /// <param name="isUp">Position up</param>
    public void showDealer(bool isShow, bool isLeft = false, bool isUp = false)
    {
        dealerIcon.SetActive(isShow);
        if (!isShow) return;

        float posx = isLeft == true ? -60 : 60;
        float posy = isUp == true ? 25 : -25;
        // if (Config.currentGameId == Constants.LUCKY9)
        // {
        //     posx = isLeft == true ? -85 : 85;
        //     posy = isUp == true ? 50 : -50;
        // }

        dealerIcon.transform.DOLocalMove(new Vector2(posx, posy), 0);
        dealerIcon.GetComponent<CanvasGroup>().alpha = 0;
        dealerIcon.transform.eulerAngles = new Vector3(0, 0, 90);
        dealerIcon.transform.localScale = new Vector2(3, 3);
        dealerIcon.transform.DOScale(Vector2.one, 0.6f).SetEase(Ease.OutCubic);
        dealerIcon.transform.DOLocalRotate(Vector3.zero, 0.6f).SetEase(Ease.OutCubic);
        dealerIcon.GetComponent<CanvasGroup>().DOFade(1, 0.6f);
    }

    /// <summary>
    /// Enables/disables hitpot indicator
    /// </summary>
    /// <param name="isShow">Show hitpot</param>
    public void enableHitpot(bool isShow = false)
    {
        hitpot.SetActive(isShow);
    }

    /// <summary>
    /// Sets hitpot number display
    /// </summary>
    /// <param name="num">Number of pots to show</param>
    public void setHitpot(int num)
    {
        for (int i = 0; i < 4; i++)
        {
            pots[i].gameObject.SetActive(i < num);
        }
    }
    

}
