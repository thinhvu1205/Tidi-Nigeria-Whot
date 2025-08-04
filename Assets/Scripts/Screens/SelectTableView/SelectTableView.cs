using System.Collections.Generic;
using System.Linq;
using Api;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject tableItemPrefab, betItemPrefab, tabItemPrefab;
    [SerializeField] private Transform tableItemParent, betItemParent, tabItemParent;
    [SerializeField] private TextMeshProUGUI titleText, accountChip;
    [SerializeField] private TMP_InputField findTableInputField;
    [SerializeField] private List<Sprite> buttonSpriteList;
    private List<Bet> betItemList = new();
    private List<Match> matchList = new();
    private int currentMarkUnitTab = 0;
    private SelectTableTab currentSelectTableTab;

    protected override void Awake()
    {
        base.Awake();
        UpdateVisuals();
        OnClickSelectBet();
        UpdateTitle();
        GetListBet().Forget();
    }
    protected override void Start()
    {
        base.Start();

    }
    #region API Handlers
    private async UniTask GetListBet()
    {
        Bets bets = await DataSender.GetListBet(Config.currentGameId);
        betItemList = bets.Bets_.ToList();
        Debug.Log("List bet game whot : " + bets.ToString());
        LoadListBetItem();
        LoadListTableTab();
    }

    private async UniTask GetListTableByMarkUnit(int markUnit)
    {
        matchList.Clear();
        RpcFindMatchResponse response = await DataSender.FindMatch(Config.currentGameId, markUnit, false);
        if (response == null)
        {
            LoadListTableItem();
            return;
        }

        Debug.Log("Find match response: " + response.ToString());
        matchList = response.Matches.ToList();
        LoadListTableItem();
    }
    #endregion
    private void UpdateVisuals()
    {
        accountChip.text = User.userMain.accountChip.ToString();
    }
    private void UpdateTitle()
    {
        switch (Config.currentGameId)
        {
            case Constants.WHOT_GAME_ID:
                titleText.text = "Whot";
                break;
            case Constants.BACCARAT_GAME_ID:
                titleText.text = "Baccarat";
                break;
            default:
                // titleText.text = "Select Table";
                break;
        }
    }
    private void LoadListBetItem()
    {

        for (int i = 0; i < betItemParent.childCount; i++)
        {
            Destroy(betItemParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < betItemList.Count; i++)
        {
            Debug.Log("Bet item: " + betItemList[i].ToString());
            int index = i;
            // Instantiate bet item
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
            // Instantiate table tab item
            if (!bet.Enable) continue;
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
            if (!isTableTabSelected && bet.Enable)
            {
                currentMarkUnitTab = (int)bet.MarkUnit;
                tableTabItem.SetSelected();
                isTableTabSelected = true;
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
            Match match = matchList[i];
            // Instantiate table item
            TableItem tableItem = Instantiate(tableItemPrefab, tableItemParent).GetComponent<TableItem>();
            tableItem.SetData(this, match.Size, match.MaxSize, match.MarkUnit, match.Name, match.TableId, match.Open, match.MatchId);
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

    public void OnClickSelectTable()
    {
        if (currentSelectTableTab == SelectTableTab.TABLE) return;
        scrollRectTable.gameObject.SetActive(true);
        scrollRectBet.gameObject.SetActive(false);
        selectBetButton.GetComponent<Image>().sprite = buttonSpriteList[1];
        selectTableButton.GetComponent<Image>().sprite = buttonSpriteList[0];
        currentSelectTableTab = SelectTableTab.TABLE;
        GetListTableByMarkUnit(currentMarkUnitTab).Forget();
    }

    public void OnClickQuickStart()
    {
        _ = UIManager.Instance.HandleQuickMatch();
    }

    public void OnClickCreateTable()
    {
        UIManager.Instance.OpenCreateTableView();
    }

    public void OnClickReload()
    {
        GetListTableByMarkUnit(currentMarkUnitTab).Forget();
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
