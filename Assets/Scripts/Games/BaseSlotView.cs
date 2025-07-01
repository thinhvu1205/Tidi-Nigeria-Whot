using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] protected Transform lineContainer, effectContainer, paylineInfoContainer, columnContainer;
    [SerializeField] protected GameObject rulePrefab, linePrefab;
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

    #endregion

    private void Init()
    {
        paylineInfoContainer.gameObject.SetActive(false);
    }
}
