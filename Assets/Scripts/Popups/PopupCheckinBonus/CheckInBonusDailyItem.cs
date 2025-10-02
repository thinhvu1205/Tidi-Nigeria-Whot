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
    private const string REWARD_ANIMATION_PATH = "Lobby/ChipBonus/gift/skeleton_SkeletonData";
    private const string LIGHT_ANIMATION_PATH = "Lobby/ChipBonus/light/skeleton_SkeletonData";
    private const string CLICK_RECEIVE_ANIMATION_NAME = "click_receive";
    private const string NOT_RECEIVE_ANIMATION_NAME = "not_receive";
    private const string RECEIVE_ANIMATION_NAME = "receive";
    private const string RECEIVED_ANIMATION_NAME = "received";

    public void SetInfo(RewardTemplate reward, int vipLevel, CheckInBonusView.RewardState state)
    {
        textChip.text = Utility.FormatMoney(reward.BasicChips[vipLevel], true);
        textTime.text = Utility.ConvertTimeToString((int)reward.OnlineSec);
        switch (state)
        {
            case CheckInBonusView.RewardState.NOT_RECEIVE:
                Utility.PlayAnimationByPath(animationGift, REWARD_ANIMATION_PATH, NOT_RECEIVE_ANIMATION_NAME, false);
                button.interactable = false;
                break;
            case CheckInBonusView.RewardState.RECEIVABLE:
                Utility.PlayAnimationByPath(animationGift, REWARD_ANIMATION_PATH, RECEIVE_ANIMATION_NAME, false);
                button.interactable = true;
                break;
            case CheckInBonusView.RewardState.RECEIVED:
                Utility.PlayAnimationByPath(animationGift, REWARD_ANIMATION_PATH, RECEIVED_ANIMATION_NAME, false);
                button.interactable = false;
                break;
        }
    }

    public void AnimateReceiveReward()
    {
        Utility.PlayAnimationByPath(animationGift, REWARD_ANIMATION_PATH, CLICK_RECEIVE_ANIMATION_NAME, false);
    }

    public void ShowAnimationLight()
    {
        animationLight.gameObject.SetActive(true);
        Utility.PlayAnimationByPath(animationLight, LIGHT_ANIMATION_PATH, "animation", true);
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
