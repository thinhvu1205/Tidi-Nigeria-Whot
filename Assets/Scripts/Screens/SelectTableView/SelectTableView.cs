using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yuujins.Match.V1;
using Bet = Yuujins.Cfg.Bet.V1.Bet;

public class SelectTableView : BaseView
{
    private enum SelectTableTab
    {
        TABLE,
        BET,
    }
    [SerializeField] private Button selectBetButton, selectTableButton, quickStartButton, createTableButton,
    nextButton, prevButton, refreshButton, findTableButton;
    [SerializeField] private ScrollRect scrollRectTable, scrollRectBet;
    [SerializeField] private GameObject tableItemPrefab, betItemPrefab, tabItemPrefab, jackpot;
    [SerializeField] private Transform tableItemParent, betItemParent, tabItemParent;
    [SerializeField] private TextMeshProUGUI titleText, accountChip;
    [SerializeField] private TMP_InputField findTableInputField;
    [SerializeField] private List<Sprite> buttonSpriteList;
    private List<Yuujins.Cfg.Bet.V1.Bet> betItemList = new();
    private List<Yuujins.Match.V1.MatchInfo> matchList = new();
    private int currentMarkUnitTab = 0;
    private SelectTableTab currentSelectTableTab;
    private SelectTablePresenter selectTablePresenter;

    protected override void Awake()
    {
        base.Awake();
        selectTablePresenter = new SelectTablePresenter();
        selectTablePresenter.Init(this);
        UpdateVisuals();
        OnClickSelectBet();
        UpdateTitle();
        GetListBet().Forget();
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        User.OnProfileUpdated += UpdateVisuals;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        User.OnProfileUpdated -= UpdateVisuals;
    }
    
    #region API Handlers
    public async UniTask GetListBet()
    {
        ListBetLevelsResponse bets = await selectTablePresenter.GetListBet(Config.currentGameId);
        UIManager.Instance.HideProgressing();
        betItemList = bets.Items.ToList();
        LoadListBetItem();
    }

    private async UniTask GetListTableByMarkUnit(int markUnit)
    {
        matchList.Clear();
        var response = await selectTablePresenter.GetListTableByMarkUnit(Config.currentGameId, markUnit);
        UIManager.Instance.HideProgressing();
        if (response?.Items == null)
        {
            LoadListTableItem();
            return;
        }
        matchList = response.Items.ToList();
        LoadListTableItem();
    }

    private async UniTask FindTable(string tableId)
    {
        matchList.Clear();
        var response = await selectTablePresenter.FindTable(Config.currentGameId, tableId);
        UIManager.Instance.HideProgressing();
        if (response?.Items == null)
        {
            LoadListTableItem();
            return;
        }
        matchList = response.Items.ToList();
        LoadListTableItem();
    }
    #endregion
    private void UpdateVisuals()
    {
        accountChip.text = Utility.FormatNumber(User.UserAccount.Profile.Balance);
    }
    private void UpdateTitle()
    {
        switch (Config.currentGameName)
        {
            case Constants.WHOT_GAME_ID:
                titleText.text = "Whot";
                break;
            case Constants.BACCARAT_GAME_ID:
                titleText.text = "Baccarat";
                break;
            case Constants.HK_POKER_GAME_ID:
                titleText.text = "HongKong Poker";
                break;
            case Constants.BLACKJACK_GAME_ID:
                titleText.text = "Blackjack";
                break;
            default:
                // titleText.text = "Select Table";
                break;
        }
        jackpot.SetActive(Constants.JACKPOT_GAMES_ID.Contains(Config.currentGameName));
    }
    private void LoadListBetItem()
    {

        for (int i = 0; i < betItemParent.childCount; i++)
        {
            Destroy(betItemParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < betItemList.Count; i++)
        {
            int index = i;
            // Instantiate bet item
            // if (betItemList[i].BetDisableType == BetDisableType.AboveMaxVip || betItemList[i].BetDisableType == BetDisableType.BelowMinVip
            // )
            // {
            //     continue;
            // }
            BetItem betItem = Instantiate(betItemPrefab, betItemParent).GetComponent<BetItem>();
            betItem.SetData(betItemList[index], index);
        }
        // tabItemParent.GetChild(currentTab).GetComponent<TableTabItem>().SetSelected();
    }

    private void LoadListTableTab()
    {
        foreach (Transform transform in tabItemParent)
        {
            Destroy(transform.gameObject);
        }
        bool isTableTabSelected = false;

        foreach (Bet bet in betItemList)
        {
            // Instantiate table tab item (cfg bet không có CountPlaying → vẫn hiển thị tab)
            // if (bet.CountPlaying == 0) continue;
            TableTabItem tableTabItem = Instantiate(tabItemPrefab, tabItemParent).GetComponent<TableTabItem>();
            tableTabItem.SetData(bet.MarkUnit, true);
            tableTabItem.GetComponent<Button>().onClick.AddListener(async () =>
            {
                foreach (Transform child in tabItemParent)
                {
                    child.GetComponent<TableTabItem>().SetUnselected();
                }
                tableTabItem.SetSelected();
                currentMarkUnitTab = (int)bet.MarkUnit;
                await GetListTableByMarkUnit(currentMarkUnitTab);
            });
 
            bool betEnable = User.UserAccount.Profile.Vip >= bet.MinVip && User.UserAccount.Profile.Vip <= bet.MaxVip;
            if (!isTableTabSelected && betEnable)
            {
                currentMarkUnitTab = (int)bet.MarkUnit;
                tableTabItem.SetSelected();
                isTableTabSelected = true;
                _ = GetListTableByMarkUnit(currentMarkUnitTab);
            }
            else
            {
                tableTabItem.SetUnselected();
            }
            
        }
    }

    private void LoadListTableItem()
    {
        foreach (Transform transform in tableItemParent)
        {
            Destroy(transform.gameObject);
        }
        for (int i = 0; i < matchList.Count; i++)
        {
            var matchItem = matchList[i];
            TableItem tableItem = Instantiate(tableItemPrefab, tableItemParent).GetComponent<TableItem>();
            tableItem.SetData(matchItem);
        }
    }

    #region Button


    public void OnClickSelectBet()
    {
        if (currentSelectTableTab == SelectTableTab.BET) return;
        scrollRectTable.gameObject.SetActive(false);
        scrollRectBet.gameObject.SetActive(true);
        selectBetButton.GetComponent<Image>().sprite = buttonSpriteList[0];
        selectTableButton.GetComponent<Image>().sprite = buttonSpriteList[1];
        currentSelectTableTab = SelectTableTab.BET;
        // scrollRectBet.DOVerticalNormalizedPos(0f, 0.2f).SetEase(Ease.OutSine);

    }

    public void OnClickSelectTable(bool isFetchListItem = true)
    {
        if (currentSelectTableTab == SelectTableTab.TABLE) return;
        scrollRectTable.gameObject.SetActive(true);
        scrollRectBet.gameObject.SetActive(false);
        selectBetButton.GetComponent<Image>().sprite = buttonSpriteList[1];
        selectTableButton.GetComponent<Image>().sprite = buttonSpriteList[0];
        currentSelectTableTab = SelectTableTab.TABLE;
        LoadListTableTab();
        if (isFetchListItem)
        {
            GetListTableByMarkUnit(currentMarkUnitTab).Forget();
            LoadListTableItem();
        }
    }

    public void OnClickQuickStart()
    {
        _ = UIManager.Instance.HandleQuickMatch();
    }

    public void OnClickCreateTable()
    {
        if (!betItemList.Any((bet) => User.UserAccount.Profile.Vip >= bet.MinVip && User.UserAccount.Profile.Vip <= bet.MaxVip))
        {
            UIManager.Instance.ShowConfirmDialog("You do not have enough chips to create table!", () => UIManager.Instance.OpenShop(), null, "Get More Chips");
            return;
        }
        UIManager.Instance.OpenCreateTableView();
    }

    public void OnClickReload()
    {
        _ = GetListTableByMarkUnit(currentMarkUnitTab);
    }

    public void OnClickFindTable()
    {
        string tableId = findTableInputField.text.Trim();
        if (string.IsNullOrEmpty(tableId)) return;
        _ = FindTable(tableId);
        OnClickSelectTable(false);
    }

    public void OnClickNext()
    {
        scrollRectBet.DOHorizontalNormalizedPos(1.0f, 0.2f).SetEase(Ease.OutSine);
        // if (isHideBtnScroll) return;
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(true);
    }

    public void OnClickPrev()
    {
        scrollRectBet.DOHorizontalNormalizedPos(0.0f, 0.2f).SetEase(Ease.OutSine);
        // if (isHideBtnScroll) return;
        nextButton.gameObject.SetActive(true);
        prevButton.gameObject.SetActive(false);
    }

    public void OnScrollListBet()
    {
        //Globals.Logging.Log(scrBet.horizontalNormalizedPosition);
        float posX = scrollRectBet.horizontalNormalizedPosition;
        float viewportWidth = scrollRectBet.viewport.GetComponent<RectTransform>().rect.width;
        float contentWidth = scrollRectBet.content.GetComponent<RectTransform>().rect.width;
        prevButton.gameObject.SetActive(viewportWidth < contentWidth && posX > 0.25f);
        nextButton.gameObject.SetActive(viewportWidth < contentWidth && posX < 0.75f);
    }
    #endregion
}
