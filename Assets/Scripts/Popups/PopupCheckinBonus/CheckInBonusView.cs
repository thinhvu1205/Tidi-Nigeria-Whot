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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CheckInBonusView : BaseView
{
    public static event Action OnRewardClaimed;
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
    [SerializeField] private GameObject selectedDailyTab, selectedWeeklyTab, redDotDaily, redDotWeekly;
    [SerializeField] private Transform weeklyItemParent, dailyItemParent;
    [SerializeField] private Image imageProgress, imageChipDaily, imageBoxChip;
    [SerializeField] private TextMeshProUGUI textClaimableDailyChip;
    [SerializeField] private Sprite[] listSpriteClaimableDailyChip;
    [SerializeField] private Button buttonClaimDailyChip;
    private CheckInBonusPresenter checkInBonusPresenter;
    private List<RewardTemplate> listDailyRewardTemplate = new();
    private List<WeeklyRewardTemplate> listWeeklyRewardTemplate = new();
    private Reward nextReward;
    private CheckInBonusDailyItem nextClaimableDailyItem;
    private Tween claimRewardTween;
    private int VipLevel => (int)User.userProfile.VipLevel;
    private int dailyStreak = 0;
    private int weeklyStreak = 0;
    private bool canClaimDailyReward = false, canClaimWeeklyReward = false;
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
        CheckInBonusWeeklyItem.OnButtonClicked += OnClickReceiveWeeklyReward;
        _ = GetClaimableDailyReward(true);
        _ = GetClaimableWeeklyReward();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        claimRewardTween?.Kill();
    }

    private async UniTask GetDailyReward(bool isFill)
    {
        UIManager.Instance.ShowProgressing();
        DailyRewardTemplate dailyReward = await checkInBonusPresenter.GetDailyReward();
        UIManager.Instance.HideProgressing();
        listDailyRewardTemplate = dailyReward.RewardTemplates.ToList();
        InitDailyItems(isFill);
    }

    private async UniTask GetClaimableDailyReward(bool isFill)
    {
        UIManager.Instance.ShowProgressing();
        Reward reward = await checkInBonusPresenter.GetClaimableDailyReward();
        UIManager.Instance.HideProgressing();
        nextReward = reward;
        dailyStreak = (int)reward.Streak;
        if (!gameObject.activeInHierarchy) return;
        if (reward.CanClaim && reward.DeviceAllowed)
        {
            redDotDaily.SetActive(true);
            canClaimDailyReward = true;
        }
        else
        {
            redDotDaily.SetActive(false);
            canClaimDailyReward = false;
        }
        _ = GetDailyReward(isFill);
    }

    private async UniTask GetWeeklyReward()
    {
        UIManager.Instance.ShowProgressing();
        WeeklyBonusTemplate weeklyReward = await checkInBonusPresenter.GetWeeklyReward();
        UIManager.Instance.HideProgressing();
        listWeeklyRewardTemplate = weeklyReward.RewardTemplates.ToList();
        InitWeeklyItems();
      
    }

    private async UniTask GetClaimableWeeklyReward()
    {
        UIManager.Instance.ShowProgressing();
        Reward reward = await checkInBonusPresenter.GetClaimableWeeklyReward();
        UIManager.Instance.HideProgressing();
        canClaimWeeklyReward = reward.CanClaim;
        weeklyStreak = (int)reward.Streak;
        if (reward.CanClaim && reward.DeviceAllowed)
        {
            redDotWeekly.SetActive(true);
        }
        else
        {
            redDotWeekly.SetActive(false);
        }
        _ = GetWeeklyReward();
    }

    public void OnClickDailyTab()
    {
        selectedDailyTab.SetActive(true);
        selectedWeeklyTab.SetActive(false);
        dailyItemTab.SetActive(true);
        weeklyItemTab.SetActive(false);
    }

    public void OnClickWeekly()
    {
        selectedDailyTab.SetActive(false);
        selectedWeeklyTab.SetActive(true);
        dailyItemTab.SetActive(false);
        weeklyItemTab.SetActive(true);
        // _ = GetClaimableWeeklyReward();
    }

    public void OnClickReceiveDailyReward()
    {
        if (!canClaimDailyReward) return;
        UIManager.Instance.ShowProgressing();
        _ = checkInBonusPresenter.ClaimDailyReward();
    }

    public void OnClickReceiveWeeklyReward()
    {
        if (!canClaimWeeklyReward) return;
        UIManager.Instance.ShowProgressing();
        _ = checkInBonusPresenter.ClaimWeeklyReward();
    }

    public void ReceiveDailyReward()
    {
        buttonClaimDailyChip.gameObject.SetActive(false);
        nextClaimableDailyItem.AnimateReceiveReward();
        claimRewardTween?.Kill();
        OnRewardClaimed?.Invoke();
        claimRewardTween = DOVirtual.DelayedCall(2f, async () =>
        {
            await UIManager.Instance.LoadProfileUser();
            await GetClaimableDailyReward(false);
        }).SetTarget(this);
    }

    public void ReceiveWeeklyReward()
    {
        OnRewardClaimed?.Invoke();
        DOVirtual.DelayedCall(0.5f, async () =>
        {
            await UIManager.Instance.LoadProfileUser();
            await GetClaimableWeeklyReward();
        });
    }

    private void InitDailyItems(bool isFill)
    {
        foreach (Transform child in dailyItemParent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < listDailyRewardTemplate.Count; i++)
        {
            CheckInBonusDailyItem dailyItem = Instantiate(dailyItemPrefab, dailyItemParent);
            RewardTemplate reward = listDailyRewardTemplate[i];
            RewardState state = RewardState.NOT_RECEIVE;
            dailyItem.HideAnimationLight();
            if (dailyStreak == (int)reward.Streak)
            {
                textClaimableDailyChip.text = Utility.FormatNumber(reward.BasicChips[VipLevel]);
                imageChipDaily.sprite = listSpriteClaimableDailyChip[dailyStreak - 1];
                nextClaimableDailyItem = dailyItem;
                nextClaimSec = reward.OnlineSec;
                currentClaimSec = reward.OnlineSec - nextReward.NextClaimSec;
                if (isFill)
                {
                    StartCoroutine(AnimateFill((dailyStreak - 1 + currentClaimSec / nextClaimSec) / (float)listDailyRewardTemplate.Count));
                }
                if (nextReward.CanClaim&& nextReward.DeviceAllowed)
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
            if (dailyStreak > i + 1)
            {
                state = RewardState.RECEIVED;
            }
            dailyItem.SetInfo(reward, VipLevel, state);
        }
        if (nextReward.ReachMaxStreak)
        {
            imageProgress.fillAmount = 1;
            buttonClaimDailyChip.gameObject.SetActive(false);
            imageBoxChip.gameObject.SetActive(false);
        }
        else
        {
            imageBoxChip.gameObject.SetActive(true);
            StartCoroutine(ClaimTimer());
        }
    }

    private void InitWeeklyItems()
    {
        foreach (Transform child in weeklyItemParent)
        {
            Destroy(child.gameObject);
            lastWeeklyItem.gameObject.SetActive(false);
        }
        for (int i = 0; i < 7; i++)
        {
            CheckInBonusWeeklyItem weeklyItem;
            if (i == 6)
            {
                weeklyItem = lastWeeklyItem;
                weeklyItem.gameObject.SetActive(true);
            }
            else
            {
                weeklyItem = Instantiate(weeklyItemPrefab, weeklyItemParent);
            }
            WeeklyRewardTemplate reward = listWeeklyRewardTemplate[VipLevel];
            RewardState state;
            if (weeklyStreak > i + 1)
            {
                state = RewardState.RECEIVED;
            }
            else if (weeklyStreak < i + 1)
            {
                state = RewardState.NOT_RECEIVE;
            }
            else
            {
                if (canClaimWeeklyReward)
                {
                    state = RewardState.RECEIVABLE;
                }
                else
                {
                    state = RewardState.NOT_RECEIVE;
                }
            }
            weeklyItem.SetInfo(i + 1, reward.Rewards[i], state);
        }
    }

    private IEnumerator AnimateFill(float amount)
    {
        Debug.Log("Amount: " + amount);
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
            imageProgress.fillAmount = (float)(dailyStreak - 1 + currentClaimSec / nextClaimSec) / 6;
            if (currentClaimSec >= nextClaimSec && nextReward.CanClaim && nextReward.DeviceAllowed)
            {
                nextClaimableDailyItem.ShowAnimationLight();
                buttonClaimDailyChip.gameObject.SetActive(true);
            }
        }
    }
}
