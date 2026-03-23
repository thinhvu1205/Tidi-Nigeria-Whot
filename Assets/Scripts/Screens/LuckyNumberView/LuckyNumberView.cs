using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckyNumberView : BaseView
{
    [Header("Prefabs")]
    [SerializeField] private LuckyNumberHistoryView historyView;
    [SerializeField] private LuckyNumberRuleView ruleView;
    [SerializeField] private LuckyNumberSelectView selectView;
    [SerializeField] private LuckyNumberItem luckyNumberItemPrefab;
    [SerializeField] private LuckyNumberItemDraw luckyNumberItemDrawPrefab;
    [SerializeField] private LuckyNumberWinnerItem luckyNumberWinnerItemPrefab;

    [Header("Transforms")]
    [SerializeField] private Transform luckyNumberItemParent;
    [SerializeField] private Transform winnerItemParent;
    [SerializeField] private Transform drawItemParent;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI[] listTextWinningNumber;
    [SerializeField] private TextMeshProUGUI textAccountChip;
    [SerializeField] private TextMeshProUGUI textUpdateTime;
    [SerializeField] private TextMeshProUGUI textPrizePool;
    [SerializeField] private TextMeshProUGUI textNextDrawTime;

    [Header("Buttons")]
    [SerializeField] private Button buttonConfirmNumber;
    [SerializeField] private Button buttonConfirmDraw;
    [SerializeField] private Button buttonClear;

    private LuckyNumberPresenter luckyNumberPresenter;
    private List<LotteryDraw> listLoterryDraw = new();
    private List<LuckyNumberItem> listLuckyNumberItem = new();
    private List<LuckyNumberItemDraw> listLuckyNumberItemDraw = new();
    private List<int> listSelectedNumbers = new();
    private long selectedDrawId;
    private long price;

    protected override void Awake()
    {
        base.Awake();
        User.OnProfileUpdated += UpdateAccountChip;
        luckyNumberPresenter = new LuckyNumberPresenter();
        luckyNumberPresenter.Init(this);
        selectView.Init(this);
        historyView.Init(this);
        InitNumberItems();
        _ = GetLatestDrawResult();
        UpdateAccountChip();
        buttonConfirmNumber.interactable = false;
        buttonClear.interactable = false;
    }

    override protected void OnDestroy()
    {
        base.OnDestroy();
        User.OnProfileUpdated -= UpdateAccountChip;
    }

    #region API Handlers
    private async UniTask GetAvailableDraws()
    {
        GetAvailableDrawsResponse response = await luckyNumberPresenter.GetAvailableDraws();
        if (response != null)
        {
            listLoterryDraw = response.Draws.ToList();
            InitDrawItems();
        }
        // if (listLoterryDraw.Count > 0)
        // {
        //     long nextDrawTime = listLoterryDraw[0].DrawTimeUnix;
        //     textNextDrawTime.text = "Next Draw: " + Utility.ConvertUnixTimeToHHMMDDMMYYYY(nextDrawTime);
        // }
    }

    private async UniTask GetLatestDrawResult()
    {
        GetLatestDrawResultResponse response = await luckyNumberPresenter.GetLatestDrawResult();
        if (response != null)
        {
            var winningNumbers = response.Draw.WinningNumbers.ToArray();
            for (int i = 0; i < listTextWinningNumber.Length; i++)
            {
                listTextWinningNumber[i].text = winningNumbers[i].ToString();
            }
            textUpdateTime.text = "Updated at: " + Utility.ConvertUnixTimeToHHMMDDMMYYYY(response.Draw.DrawTimeUnix);
            textPrizePool.text = "Prize Pool: " + Utility.FormatNumber(response.Draw.PrizePool);
            var sortedTopWinners = response.TopWinners
                .OrderByDescending(w => w.TotalReward)
                .ToList();
            int count = sortedTopWinners.Count;
            
            for (int i = 0; i < count; i++)
            {
                var winner = sortedTopWinners[i];

                var item = Instantiate(luckyNumberWinnerItemPrefab, winnerItemParent);
                item.SetData(
                    (i + 1).ToString(),
                    winner.Profile.UserName,
                    winner.TotalReward,
                    winner.Profile.AvatarId,
                    winner.Profile.VipLevel
                );
            }

        }
    }

    public async UniTask GetLotteryHistory()
    {
        GetLotteryHistoryResponse response = await luckyNumberPresenter.GetLotteryHistory();
        if (response != null)
        {
            var listLotteryTicket = response.Tickets.ToList();
            historyView.InitHistoryItems(listLotteryTicket);
        }
    }

    #endregion


    #region UI
    private void InitNumberItems()
    {
        for (int i = 1; i <= 49; i++)
        {
            LuckyNumberItem item = Instantiate(luckyNumberItemPrefab, luckyNumberItemParent);
            item.SetInfo(i);
            item.GetComponent<Button>().onClick.AddListener(() => OnChooseNumber(item));
            listLuckyNumberItem.Add(item);
        }
    }

    private void InitDrawItems()
    {
        foreach (Transform child in drawItemParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < listLoterryDraw.Count; i++)
        {
            LotteryDraw draw = listLoterryDraw[i];
            LuckyNumberItemDraw item = Instantiate(luckyNumberItemDrawPrefab, drawItemParent);
            item.SetInfo(draw, i);
            item.GetComponent<Button>().onClick.AddListener(() => OnChooseDraw(item));
            listLuckyNumberItemDraw.Add(item);
            price = draw.TicketPrice;
        }
    }

    private void UpdateAccountChip()
    {
        textAccountChip.text = Utility.FormatNumber(User.Profile.Chips);
    }

    #endregion


    #region Button Events
    public void OnChooseNumber(LuckyNumberItem item)
    {
        if (item.isSelected)
        {
            listSelectedNumbers.Remove(item.number);
            buttonConfirmNumber.interactable = false;
            if (listSelectedNumbers.Count == 0)
            {
                buttonClear.interactable = false;
            }
        }
        else
        {
            if (listSelectedNumbers.Count >= 6)
            {
                UIManager.Instance.ShowToast("You can only select up to 6 numbers.", 2, transform);
                return;
            }
            if (listSelectedNumbers.Count == 5)
            {
                buttonConfirmNumber.interactable = true;
            }
            buttonClear.interactable = true;
            listSelectedNumbers.Add(item.number);
        }
        item.ToggleSelected();
    }

    public void OnChooseDraw(LuckyNumberItemDraw item)
    {
        if (!item.isInteractable) return;
        if (item.isSelected)
        {
            selectedDrawId = 0;
            buttonConfirmDraw.interactable = false;
            ResetDraw();
        }
        else
        {
            selectedDrawId = item.id;
            buttonConfirmDraw.interactable = true;
            foreach(LuckyNumberItemDraw otherItem in listLuckyNumberItemDraw)
            {
                if (otherItem == item) continue;
                otherItem.SetUnselected();
            }
        }        
        item.ToggleSelected();
    }

    private void ResetDraw()
    {
        foreach(LuckyNumberItemDraw otherItem in listLuckyNumberItemDraw)
        {
            otherItem.isInteractable = true;
        }
    }

    public void OnClickQuickPick()
    {
        OnClickClear();
        _ = HandleQuickPickAsync();
    }

    private async UniTask HandleQuickPickAsync()
    {
        QuickPickResponse quickPickResponse = await luckyNumberPresenter.QuickPick();
        if (quickPickResponse != null)
        {
            var numbers = quickPickResponse.NumberSets.ToList()[0].Numbers.ToArray();
            foreach (int number in numbers)
            {
                LuckyNumberItem item = listLuckyNumberItem[number - 1];
                if (!item.isSelected)
                {
                    listSelectedNumbers.Add(item.number);
                    item.ToggleSelected();
                }
            }
            buttonConfirmNumber.interactable = true;
            buttonClear.interactable = true;
        }
    }

    public void OnClickClear()
    {
        foreach (Transform child in luckyNumberItemParent)
        {
            LuckyNumberItem item = child.GetComponent<LuckyNumberItem>();
            if (item.isSelected)
            {
                item.ToggleSelected();
            }
        }
        listSelectedNumbers.Clear();
        buttonConfirmNumber.interactable = false;
        buttonClear.interactable = false;
    }

    public async void OnClickConfirmNumber()
    {
        try
        {
            if (listSelectedNumbers.Count < 6)
            {
                UIManager.Instance.ShowToast("Please select exactly 6 numbers.", 2, transform);
                return;
            }

            await GetAvailableDraws();
           
            selectView.Show();
            selectedDrawId = 0;
            buttonConfirmDraw.interactable = false; 
            foreach (LuckyNumberItemDraw item in listLuckyNumberItemDraw)
            {
                if (item.isSelected)
                {
                    item.ToggleSelected();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OnClickRule()
    {
        ruleView.Show();
    }

    public void OnClickHistory()
    {
        historyView.Show();
    }

    public void OnClickShop()
    {
        UIManager.Instance.OpenShop();
    }

    public void OnClickConfirmDraw()
    {
        _ = HandleBuyLotteryTicketAsync();
    }
    
    private async UniTask HandleBuyLotteryTicketAsync()
    {
        if (selectedDrawId == 0)
        {
            UIManager.Instance.ShowToast("Please select a draw.", 2, transform);
            return;
        }
        if (User.Profile.Chips < price)
        {
            UIManager.Instance.ShowConfirmDialog("You do not have enough chips!", () => UIManager.Instance.OpenShop(), null, "Get More Chips");
            selectedDrawId = 0;
            ResetDraw();
            selectView.OnClickCloseButton();
            return;
        }
        BuyLotteryTicketResponse buyLotteryTicketResponse = await luckyNumberPresenter.BuyLotteryTicket(listSelectedNumbers, selectedDrawId);
        OnClickClear();
        selectView.OnClickCloseButton();
        OnClickHistory();
    }
    #endregion

}
