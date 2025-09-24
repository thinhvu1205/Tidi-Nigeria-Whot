using System;
using Globals;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckInBonusDailyItem : MonoBehaviour
{
    public static event Action OnButtonClicked;
    [SerializeField] private SkeletonGraphic animationLight, animationGift;
    [SerializeField] private TextMeshProUGUI textTime, textChip;
    [SerializeField] private Button button;
    private const string CLICK_RECEIVE_ANIMATION_NAME = "click_receive";
    private const string NOT_RECEIVE_ANIMATION_NAME = "not_receive";
    private const string RECEIVE_ANIMATION_NAME = "receive";
    private const string RECEIVED_ANIMATION_NAME = "received";

    public void SetInfo(RewardTemplate reward, int vipLevel, CheckInBonusView.RewardState state)
    {
        textChip.text = Utility.FormatMoney(reward.BasicChips[vipLevel], true);
        textTime.text = Utility.ConvertTimeToString((int)reward.OnlineSec);
        Debug.Log("STATE: " + state);
        switch (state)
        {
            case CheckInBonusView.RewardState.NOT_RECEIVE:
                Utility.PlayAnimation(animationGift, NOT_RECEIVE_ANIMATION_NAME, false);
                button.interactable = false;
                break;
            case CheckInBonusView.RewardState.RECEIVABLE:
                Utility.PlayAnimation(animationGift, RECEIVE_ANIMATION_NAME, false);
                button.interactable = true;
                break;
            case CheckInBonusView.RewardState.RECEIVED:
                Utility.PlayAnimation(animationGift, RECEIVED_ANIMATION_NAME, false);
                button.interactable = false;
                break;
        }
    }

    public void AnimateReceiveReward()
    {
        Utility.PlayAnimation(animationGift, CLICK_RECEIVE_ANIMATION_NAME, false);
    }

    public void ShowAnimationLight()
    {
        animationLight.gameObject.SetActive(true);
    }

    public void HideAnimationLight()
    {
        animationLight.gameObject.SetActive(false);
    }

    public void OnClickReward()
    {
        OnButtonClicked?.Invoke();
    }

}
