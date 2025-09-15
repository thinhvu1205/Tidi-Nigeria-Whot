using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;
using UnityEngine.UI;

public class CheckInBonusView : BaseView
{
    [SerializeField] private CheckInBonusDailyItem dailyItemPrefab, lastDailyItem;
    [SerializeField] private CheckInBonusWeeklyItem weeklyItemPrefab;
    [SerializeField] private GameObject dailyItemTab, weeklyItemTab;
    [SerializeField] private GameObject selectedDailyTab, selectedWeeklyTab;
    [SerializeField] private Transform dailyItemParent;
    private CheckInBonusPresenter checkInBonusPresenter;

    protected override void Awake()
    {
        base.Awake();
        InitItems();
        checkInBonusPresenter = new CheckInBonusPresenter();
        checkInBonusPresenter.Init(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnClickDailyTab();
    }

    private void InitItems()
    {
        for (int i = 0; i < 6; i++)
        {
            CheckInBonusDailyItem dailyItem = Instantiate(dailyItemPrefab, dailyItemParent.transform);
        }



    }

    private async UniTask GetDailyReward()
    {
        DailyRewardTemplate dailyReward = await checkInBonusPresenter.GetDailyReward();
        Debug.Log("DAILY REWARD: " + dailyReward.ToString());
        
    }

    public void OnClickDailyTab()
    {
        selectedDailyTab.SetActive(true);
        selectedWeeklyTab.SetActive(false);
        dailyItemTab.SetActive(true);
        weeklyItemTab.SetActive(false);
        _ = GetDailyReward();
    }

    public void OnClickWeeklyTab()
    {
        selectedDailyTab.SetActive(false);
        selectedWeeklyTab.SetActive(true);
        dailyItemTab.SetActive(false);
        weeklyItemTab.SetActive(true);
    }
}
