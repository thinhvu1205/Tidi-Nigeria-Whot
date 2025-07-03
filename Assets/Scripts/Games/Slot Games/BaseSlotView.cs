using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseSlotView : BaseView
{
    [SerializeField] protected Button maxBetButton, plusBetButton, minusBetButton;
    [SerializeField] protected TextMeshProUGUI betAmountText, betInfoSessionText, betStateText, stateWinText, freeSpinLeftText;
    [SerializeField] protected Image spinBackgroundImage;
    [SerializeField] protected List<Sprite> stateWinSpriteList, itemSpriteList;
    [SerializeField] protected Transform lineContainer, effectContainer, paylineInfoContainer, columnContainer, coinParent;
    [SerializeField] protected GameObject rulePrefab, linePrefab, coinPrefab;
    [SerializeField] protected SkeletonGraphic backgroundFreeSpinAnimation, nearFreeSpinAnimation, buttonSpinAnimation;

    [Header("Constants")]
    protected readonly List<string> colorsList = new List<string>
    {
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE",
        "#920E48", "#F277E2", "#BC8B15", "#AC6456", "#E17512",
        "#E8C500", "#F0E915", "#FD93A1", "#C735D4", "#FF0C04",
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE",
        "#920E48", "#F277E2", "#BC8B15", "#AC6456", "#E17512",
        "#E8C500", "#F0E915", "#FD93A1", "#C735D4", "#FF0C04",
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE"
    };
    protected const float AUTO_SPIN_HOLD_DURATION = 1.3f;

    [Header("Game State")]
    protected SpinType spintype = SpinType.NORMAL;
    protected SlotGameState gameState = SlotGameState.JOIN_GAME;

    [Header(" Object Pools ")]
    protected UnityEngine.Pool.ObjectPool<Image> coinPool;

    [Header("Game Data")]
    protected long playerWallet;
    protected int totalLineWin;
    protected bool isSpinning, isHoldingSpin, isFreeSpin;
    protected float holdingSpinTime = 0;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    protected void Update()
    {
        HandleHoldingSpin();   
    }

    #region Buttons
    public void OnClickSpinButton()
    {

    }

    // giữ nút spin
    public void OnTriggerDownSpinButton()
    {

    }

    // thả nút spin
    public void OnTriggerUpSpinButton()
    {

    }

    public void OnClickPlusBetButton()
    {

    }

    public void OnClickMinusBetButton()
    {

    }

    public void OnClickMaxBetButton()
    {

    }

    public void OnClickShopButton()
    {

    }

    public void OnClickRule()
    {
        
    }
        
    #endregion

    protected void HandleHoldingSpin()
    {
        if (isHoldingSpin)
        {
            holdingSpinTime += Time.deltaTime;
            if (holdingSpinTime > AUTO_SPIN_HOLD_DURATION)
            {
                // if (agPlayer < totalListBetRoom[currentMarkBet])
                // {
                //     lbInfoSession.text = Config.getTextConfig("msg_warrning_send");
                //     return;
                // }
                spintype = SpinType.AUTO;

                OnClickSpinButton();
                isHoldingSpin = false;
            }
        }
    }

    #region Effects
    protected void AnimateCoinsFly(Transform transFrom, Transform transTo, int totalCoins = 5, float timeInterval = 0.1f)
    {

        for (int i = 0; i < totalCoins; i++)
        {
            DOTween.Sequence().AppendInterval(i * timeInterval).AppendCallback(() =>
            {
                Image coin = coinPool.Get();
                coin.gameObject.SetActive(true);
                coin.GetComponent<Animator>().Play("Idle");
                AnimateCoinFly(coin, transFrom, transTo);
            });
        }
    }
    protected void AnimateCoinFly(Image coin, Transform from, Transform to)
    {
        coin.transform.position = from.position;
        coin.transform.DOJump(to.position, 1, 1, 2).SetEase(Ease.InOutCubic);
        Color fadedColor = coin.color;
        fadedColor.a = .2f;
        coin.color = fadedColor;
        coin.DOFade(1, .75f);

        DOTween.Sequence()
            .AppendInterval(.5f)
            .Append(coin.transform.DOScale(2, 0.25f))
            .AppendInterval(0.15f)
            .Append(coin.transform.DOScale(1, 0.25f))//0.85
            .AppendInterval(0.6f)
            .Append(coin.DOFade(0, .25f)).AppendCallback(() =>
            {
                coinPool.Release(coin);
            });
    }
    #endregion

    private void Init()
    {
        paylineInfoContainer.gameObject.SetActive(false);
        coinPool = new UnityEngine.Pool.ObjectPool<Image>(
            createFunc: () =>
            {
                var coin = Instantiate(coinPrefab, coinParent).GetComponent<Image>();
                coin.gameObject.SetActive(false); // bắt đầu ẩn
                return coin;
            },
            actionOnGet: (coin) =>
            {
                coin.gameObject.SetActive(true);
                coin.transform.localScale = Vector3.one;
                coin.color = new Color(coin.color.r, coin.color.g, coin.color.b, 0f);
            },
            actionOnRelease: (coin) =>
            {
                coin.gameObject.SetActive(false);
            },
            actionOnDestroy: (coin) =>
            {
                Destroy(coin.gameObject);
            },
            collectionCheck: false,  // không cần check trùng (cho nhanh)
            defaultCapacity: 10,     // số lượng khởi tạo
            maxSize: 20             // tối đa object trong pool
        );
    }
}
