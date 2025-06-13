using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectTableView : BaseView
{
    [SerializeField]
    private Button selectBetButton, selectTableButton, quickStartButton, createTableButton,
    nextButton, prevButton, refreshButton, findTableButton;
    [SerializeField] private ScrollRect scrollRectTable, scrollRectBet;
    [SerializeField] private GameObject tableItemPrefab, betItemPrefab, tabItemPrefab;
    [SerializeField] private Transform tableItemParent, betItemParent, tabItemParent;
    [SerializeField] private TextMeshProUGUI titleText, accountChip;
    [SerializeField] private TMP_InputField findTableInputField;
    [SerializeField] private List<Sprite> buttonSpriteList;
    private Bets bets;
    private int currentTab = 0;

    protected override void Awake()
    {
        base.Awake();
        Config.currentGameId = Constants.WhotGameID;
        UpdateVisuals();
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
        Bets bets = await DataSender.GetListBet(Constants.WhotGameID);
        PlayerCountByBetResponse playerCounts = await DataSender.GetPlayerCountByBet(Constants.WhotGameID);
        this.bets = bets;
        Debug.Log("List bet game whot : " + bets.ToString());
        Debug.Log("Player count by bet: " + playerCounts.ToString());
        LoadListBetItem();

    }
    #endregion
    private void UpdateVisuals()
    {
        accountChip.text = User.userMain.accountChip;
    }
    private void UpdateTitle()
    {
        switch (Config.currentGameId)
        {
            case Constants.WhotGameID:
                titleText.text = "Whot";
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
        List<Bet> betList = bets.Bets_.ToList();
        for (int i = 0; i < betList.Count; i++)
        {
            int index = i;
            var betItem = Instantiate(betItemPrefab, betItemParent);
            betItem.GetComponent<BetItem>().SetData(betList[index], index);
            // var tableTabItem = Instantiate(tabItemPrefab, tabItemParent);
            // tableTabItem.GetComponent<TableTabItem>().SetData(betList[index] as JObject);
            // tableTabItem.GetComponent<Button>().onClick.AddListener(() =>
            // {
            //     OnClickTab(tableTabItem.GetComponent<TableTabItem>(), mockArray[index] as JObject, index);
            // });
            // if (index == 0)
            // {
            //     OnClickTab(tableTabItem.GetComponent<TableTabItem>(), mockArray[index] as JObject, index);
            // }
        }
        // tabItemParent.GetChild(currentTab).GetComponent<TableTabItem>().SetSelected();
    }

    private void LoadListTableItem(int mark)
    {
        scrollRectTable.DOVerticalNormalizedPos(1.0f, 0.2f).SetEase(Ease.OutSine);

        // for (int i = 0; i < tableItemParent.childCount; i++)
        // {
        //     Destroy(tableItemParent.GetChild(i).gameObject);
        // }
        // for (int i = 0; i < mockTableArray.Count; i++)
        // {
        //     if ((int)mockTableArray[i]["mark"] != mark) continue;
        //     int index = i;
        //     var tableItem = Instantiate(tableItemPrefab, tableItemParent);
        //     tableItem.GetComponent<TableItem>().SetData(mockTableArray[index] as JObject, index);

        // }
    }

    #region Button
    public void OnClickTab(TableTabItem tableTabItem, JObject dataItem, int index)
    {
        currentTab = index;
        for (int i = 0; i < tabItemParent.childCount; i++)
        {
            var item = tabItemParent.GetChild(i).GetComponent<TableTabItem>();
            if (item != null)
            {
                if (item.gameObject.name == tableTabItem.gameObject.name)
                {
                    item.SetSelected();
                }
                else
                {
                    item.SetUnselected();
                }
            }
        }
        int mark = (int)dataItem["mark"];
        LoadListTableItem(mark);
    }

    public void OnClickSelectBet()
    {
        scrollRectTable.gameObject.SetActive(false);
        scrollRectBet.gameObject.SetActive(true);
        selectBetButton.GetComponent<Image>().sprite = buttonSpriteList[0];
        selectTableButton.GetComponent<Image>().sprite = buttonSpriteList[1];
        // scrollRectBet.DOVerticalNormalizedPos(0f, 0.2f).SetEase(Ease.OutSine);

    }

    public void OnClickSelectTable()
    {
        scrollRectTable.gameObject.SetActive(true);
        scrollRectBet.gameObject.SetActive(false);
        selectBetButton.GetComponent<Image>().sprite = buttonSpriteList[1];
        selectTableButton.GetComponent<Image>().sprite = buttonSpriteList[0];
        LoadListBetItem();
    }

    public void OnClickQuickStart()
    {

    }

    public void OnClickCreateTable()
    {

    }

    public void OnClickReload()
    {

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
