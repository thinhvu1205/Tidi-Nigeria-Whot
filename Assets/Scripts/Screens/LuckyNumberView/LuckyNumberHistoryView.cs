using System.Collections;
using System.Collections.Generic;
using Proto;
using UnityEngine;

public class LuckyNumberHistoryView : BaseView
{
    [SerializeField] private Transform historyItemParent;
    [SerializeField] private LuckyNumberHistoryItem historyItemPrefab;
    private LuckyNumberView luckyNumberView;

    public void Init(LuckyNumberView luckyNumberView)
    {
        this.luckyNumberView = luckyNumberView;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _ = luckyNumberView.GetLotteryHistory();
    }

    public void InitHistoryItems(List<LotteryTicket> listLotteryTicket)
    {
        foreach (Transform child in historyItemParent)
        {
            Destroy(child.gameObject);
        }

        foreach (LotteryTicket ticket in listLotteryTicket)
        {
            LuckyNumberHistoryItem item = Instantiate(historyItemPrefab, historyItemParent);
            item.Init(ticket);
        }
    }
    
    public override void OnClickCloseButton()
    {
        Hide(false);
    }
}
