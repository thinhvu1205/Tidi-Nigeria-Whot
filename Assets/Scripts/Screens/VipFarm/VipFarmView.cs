using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VipFarmView : BaseView
{
    public static event Action OnClaimed;
    [SerializeField] private TextMeshProUGUI textCurrentLevel, textNextLevel, textCurrentLevelChip, textNextLevelChip, textProgress, textMoney, textMoneyReceive;
    [SerializeField] private Button buttonClaim;
    [SerializeField] private SkeletonGraphic animationTree1, animationTree2, animationTree3, animationReceive;
    [SerializeField] private GameObject receiveInfo, arrow, currentLevelTree;
    [SerializeField] private Image imageCurrentLevelTree, imageNextLevelTree;
    [SerializeField] private Sprite[] listImageTree;
    private const string TREE_ANIMATION_PATH = "VipFarm/anims/%name/skeleton_SkeletonData";
    private readonly string[] listTreeNameAnimation = new string[]
    {
        "V2_CaTim",
        "V3_Muop",
        "V4_DuaHau",
        "V5_Ngo",
        "V6_Lua",
        "V7_DuDu",
        "V8_Chuoi",
        "V9_Dua",
        "V10_Xoai",
    };
    
    private VipFarmPresenter vipFarmPresenter;
    private UserVipFarmProgress data;
    private double reward;

    protected override void Awake()
    {
        base.Awake();
        vipFarmPresenter = new VipFarmPresenter();
        vipFarmPresenter.Init(this);

        _ = GetVipFarmProgress();
    }

    private async UniTask GetVipFarmProgress()
    {
        data = await vipFarmPresenter.GetVipFarmProgress();
        UpdateUI();
    }

    private void UpdateUI()
    {
        int currentLevel = (int)data.Level;
        double progress = data.Progress;
        reward = data.CurrentReward;
        textCurrentLevel.text = "Lv " + data.Level.ToString();
        textNextLevel.text = "Lv " + (data.Level + 1).ToString();
        textCurrentLevelChip.text = Utility.FormatNumber(data.CurrentReward);
        textMoney.text = Utility.FormatNumber(data.CurrentReward);
        textNextLevelChip.text = Utility.FormatNumber(data.NextReward);
        textProgress.text = (data.Progress * 100).ToString("F2") + "%";
        buttonClaim.interactable = data.Progress >= 1;
        UpdateTree(currentLevel, progress);
    }
    
    private void UpdateTree(int currentLevel, double progress)
    {
        int nextLevel = currentLevel == 10 ? 10 : currentLevel + 1;
        if (currentLevel == 10)
        {
            arrow.SetActive(false);
            currentLevelTree.SetActive(false);
        }
        string suffix;
        if (progress == 1)
        {
            suffix = "4";
        }
        else if (progress > 0.5 && progress <= 0.75)
        {
            suffix = "3";
        }
        else if (progress > 0.25 && progress <= 0.5)
        {
            suffix = "2";
        }
        else
        {
            suffix = "1";
        }
        if (nextLevel - 2 < 0) return;
        string animationPath = TREE_ANIMATION_PATH.Replace("%name", listTreeNameAnimation[nextLevel - 2]);
        string animationName = $"V{nextLevel}_{suffix}";
        Debug.Log("animationName: " + animationName);
        Utility.PlayAnimationByPath(animationTree1, animationPath, animationName, true);
        Utility.PlayAnimationByPath(animationTree2, animationPath, animationName, true);
        Utility.PlayAnimationByPath(animationTree3, animationPath, animationName, true);

        imageNextLevelTree.sprite = listImageTree[nextLevel - 2];
        imageCurrentLevelTree.sprite = listImageTree[currentLevel - 2 < 0 ? 0 : currentLevel - 2];
        
    }

    public void OnClickClaim()
    {
        _ = HandleClickClaim();
    }

    private async UniTask HandleClickClaim()
    {
        await vipFarmPresenter.ClaimVipFarm();
        receiveInfo.SetActive(true);
        Utility.PlayAnimation(animationReceive, "animation", false);
        Utility.TweenNumberToNumber(textMoneyReceive, (int)reward, 0);
        animationReceive.AnimationState.Complete += async delegate
        {
            receiveInfo.SetActive(false);
            await GetVipFarmProgress();
            OnClaimed?.Invoke();
        };

    }
}
