using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class SlotJuicyView : BaseSlotView
{
    [SerializeField] private Transform holdPackageContainer;
    [SerializeField] private TextNumberControl grandJackpotText, majorJackpotText, minorJackpotText, miniJackpotText;
    [SerializeField] private TextMeshProUGUI totalPackageValueText;
    [SerializeField] private SkeletonGraphic popupGetFreeSpinAnimation, popupResultPackageAnimation, jackpotAnimation, totalMoneyPackageAnimation, effectPackageAnimation;
    [SerializeField] private DialogView popupGetFruitRain;
    [SerializeField] private Material grandJackpotMaterial, majorJackpotMaterial, minorJackpotMaterial, miniJackpotMaterial;
    protected override Vector2 RECT_SIZE => new(120f, 120f);
    protected override string BIG_WIN_ANIMATION_PATH => "SlotSpine/JuicyGarden/big_megawinJuicy/skeleton_SkeletonData";
    protected override string MEGA_WIN_ANIMATION_PATH => "GameView/SlotSpine/JuicyGarden/big_megawinJuicy/skeleton_SkeletonData";
    protected override string FREE_SPIN_ANIMATION_PATH => "GameView/SlotSpine/JuicyGarden/AnimBox/skeleton_SkeletonData";
    protected override string BACKGROUND_FREE_SPIN_ANIMATION_PATH => "SlotSpine/JuicyGarden/BgFreeSpin/skeleton_SkeletonData";
    private const string BACKGROUND_MONEY_PACKAGE_ANIMATION_PATH = "SlotSpine/JuicyGarden/EffectPackage/skeleton_SkeletonData";
    private const string RESULT_BONUSGAME_ANIMPATH = "SlotSpine/JuicyGarden/EndGame/skeleton_SkeletonData";
    private const string JACKPOT_ANIMPATH = "SlotSpine/JuicyGarden/Jackpot/skeleton_SkeletonData";
    private int rateJackpotGrand = 0, rateJackpotMajor = 0, rateJackpotMinor = 0, rateJackpotMini = 0;
}
