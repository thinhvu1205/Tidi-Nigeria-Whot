using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckyNumberView : BaseView
{
    [SerializeField] private LuckyNumberHistoryView historyView;
    [SerializeField] private LuckyNumberRuleView ruleView;
    [SerializeField] private LuckyNumberSelectView selectView;
    private LuckyNumberPresenter luckyNumberPresenter;

    protected override void Awake()
    {
        base.Awake();
        luckyNumberPresenter = new LuckyNumberPresenter();
        luckyNumberPresenter.Init(this);
    }

}
