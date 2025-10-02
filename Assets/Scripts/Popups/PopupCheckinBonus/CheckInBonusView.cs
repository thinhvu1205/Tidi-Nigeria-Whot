using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckInBonusView : BaseView
{
    public static event Action OnDailyRewardClaimed;
    public enum RewardState
    {
        NOT_RECEIVE,
        RECEIVED,
        RECEIVABLE
    }
    // [SerializeField] private CheckInBonusDailyItem dailyItemPrefab;
    [SerializeField] private CheckInBonusDailyItem dailyItemPrefab;
    [SerializeField] private CheckInBonusWeeklyItem weeklyItemPrefab, lastWeeklyItem;
    [SerializeField] private GameObject dailyItemTab, weeklyItemTab;
    [SerializeField] private GameObject selectedDailyTab, selectedWeeklyTab;
    [SerializeField] private Transform weeklyItemParent, dailyItemParent;
    [SerializeField] private Image imageProgress, imageChipDaily;
    [SerializeField] private TextMeshProUGUI textClaimableDailyChip;
    [SerializeField] private Sprite[] listSpriteClaimableDailyChip;
    [SerializeField] private Button buttonClaimDailyChip;
    private CheckInBonusPresenter checkInBonusPresenter;
    private List<RewardTemplate> listRewardTemplate = new();
    private Reward nextReward;
    private CheckInBonusDailyItem nextClaimableDailyItem;
    private int VipLevel => (int)User.userProfile.VipLevel;
    private int streak = 0;
    [SerializeField] private float nextClaimSec, currentClaimSec;

    protected override void Awake()
    {
        base.Awake();
        checkInBonusPresenter = new CheckInBonusPresenter();
        checkInBonusPresenter.Init(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnClickDailyTab();
        CheckInBonusDailyItem.OnButtonClicked += OnClickReceiveDailyReward;
    }

    private async UniTask GetDailyReward(bool isFill)
    {
        UIManager.Instance.ShowProgressing();
        DailyRewardTemplate dailyReward = await checkInBonusPresenter.GetDailyReward();
        UIManager.Instance.HideProgressing();
        listRewardTemplate = dailyReward.RewardTemplates.ToList();
        Debug.Log("DAILY REWARD TEMPLATE : " + dailyReward.ToString());
        InitDailyItems(isFill);
    }

    private async UniTask GetClaimableDailyReward(bool isFill)
    {
        UIManager.Instance.ShowProgressing();
        Reward reward = await checkInBonusPresenter.GetClaimableDailyReward();
        UIManager.Instance.HideProgressing();
        nextReward = reward;
        streak = (int)reward.Streak;
        _ = GetDailyReward(isFill);
    }

    private async UniTask GetWeeklyReward()
    {
        UIManager.Instance.ShowProgressing();
        WeeklyBonusTemplate dailyReward = await checkInBonusPresenter.GetWeeklyReward();
        UIManager.Instance.HideProgressing();
        // listRewardTemplate = dailyReward.RewardTemplates.ToList();
        Debug.Log("WEEKLY Bonus TEMPLATE : " + dailyReward.ToString());
        // InitDailyItems();
        await GetClaimableWeeklyReward();
    }

    private async UniTask GetClaimableWeeklyReward()
    {
        UIManager.Instance.ShowProgressing();
        Reward reward = await checkInBonusPresenter.GetClaimableWeeklyReward();
        Debug.Log("WEEKLY Can Claim : " + reward.ToString());
        UIManager.Instance.HideProgressing();
        // nextReward = reward;
        // streak = (int)reward.Streak;
        // _ = GetDailyReward();
    }

    public void OnClickDailyTab()
    {
        selectedDailyTab.SetActive(true);
        selectedWeeklyTab.SetActive(false);
        dailyItemTab.SetActive(true);
        weeklyItemTab.SetActive(false);
        _ = GetClaimableDailyReward(true);
    }

    public void OnClickWeeklyTab()
    {
        imageProgress.fillAmount = 0;
        selectedDailyTab.SetActive(false);
        selectedWeeklyTab.SetActive(true);
        dailyItemTab.SetActive(false);
        weeklyItemTab.SetActive(true);
        _ = GetWeeklyReward();
    }

    public void OnClickReceiveDailyReward()
    {
        UIManager.Instance.ShowProgressing();
        _ = checkInBonusPresenter.ClaimDailyReward();
        buttonClaimDailyChip.gameObject.SetActive(false);
    }

    public void ReceiveDailyReward()
    {
        nextClaimableDailyItem.AnimateReceiveReward();
        DOVirtual.DelayedCall(2f, async () =>
        {
            await UIManager.Instance.LoadProfileUser();
            await GetClaimableDailyReward(false);
            OnDailyRewardClaimed?.Invoke();
        });
    }

    private void InitDailyItems(bool isFill)
    {
        foreach (Transform child in dailyItemParent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < listRewardTemplate.Count; i++)
        {
            CheckInBonusDailyItem dailyItem = Instantiate(dailyItemPrefab, dailyItemParent);
            RewardTemplate reward = listRewardTemplate[i];
            RewardState state = RewardState.NOT_RECEIVE;
            dailyItem.HideAnimationLight();
            if (streak == (int)reward.Streak)
            {
                textClaimableDailyChip.text = Utility.FormatNumber(reward.BasicChips[VipLevel]);
                imageChipDaily.sprite = listSpriteClaimableDailyChip[streak - 1];
                nextClaimableDailyItem = dailyItem;
                nextClaimSec = reward.OnlineSec;
                currentClaimSec = reward.OnlineSec - nextReward.NextClaimSec;
                if (isFill)
                {
                    StartCoroutine(AnimateFill((streak - 1 + currentClaimSec / nextClaimSec) / (float)listRewardTemplate.Count));
                }
                if (nextReward.CanClaim && nextReward.DeviceAllowed)
                {
                    state = RewardState.RECEIVABLE;
                    buttonClaimDailyChip.gameObject.SetActive(true);
                    nextClaimableDailyItem.ShowAnimationLight();
                }
                else
                {
                    buttonClaimDailyChip.gameObject.SetActive(false);
                    nextClaimableDailyItem.HideAnimationLight();
                }
            }
            if (streak > i + 1)
            {
                state = RewardState.RECEIVED;
            }
            dailyItem.SetInfo(reward, VipLevel, state);
        }
        StartCoroutine(ClaimTimer());
    }

    private IEnumerator AnimateFill(float amount)
    {
        Debug.Log("AMOUNT: " + amount);
        float elapsed = 0f;

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            imageProgress.fillAmount = Mathf.Lerp(0f, amount, elapsed / 0.3f);
            yield return null;
        }

        imageProgress.fillAmount = amount;
    }
    
    private IEnumerator ClaimTimer()
    {
        while (currentClaimSec <= nextClaimSec)
        {
            yield return new WaitForSeconds(1f);
            currentClaimSec += 1;
            imageProgress.fillAmount = (float)(streak - 1 + currentClaimSec / nextClaimSec) / 6;
            if (currentClaimSec >= nextClaimSec)
            {
                nextClaimableDailyItem.ShowAnimationLight();
                buttonClaimDailyChip.gameObject.SetActive(true);
            }
        }
    }
}
