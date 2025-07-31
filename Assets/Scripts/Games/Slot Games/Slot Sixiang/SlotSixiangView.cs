using System;
using System.Collections.Generic;
using Proto;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class SlotSixiangView : BaseSlotSymbolView
{
    [SerializeField] private Transform backgroundGoldPick;
    [SerializeField] private SkeletonGraphic animationAnimal, animationCutScene;
    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol._10, 0 },
        { SiXiangSymbol.J, 1 },
        { SiXiangSymbol.Q, 2 },
        { SiXiangSymbol.K, 3 },
        { SiXiangSymbol.A, 4 },
        { SiXiangSymbol.BlueDragon, 5 },
        { SiXiangSymbol.WhiteTiger, 6 },
        { SiXiangSymbol.Warrior, 7 },
        { SiXiangSymbol.VermilionBird, 8 },
        { SiXiangSymbol.Scatter, 9 },
        { SiXiangSymbol.Wild, 10 },

    };

    protected override Dictionary<SiXiangGame, int> GemDictionary => new()
    {
        { SiXiangGame.DragonPearl, 0 },
        { SiXiangGame.Goldpick, 1 },
        { SiXiangGame.Rapidpay, 2 },
        { SiXiangGame.Luckdraw, 3 },
    };

    private const string DRAGON_PEARL_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Dragon/skeleton_SkeletonData";
    private const string GOLD_PICK_PREFAB_PATH = "BundlePack/Animations/Sixiang/Prefab/GoldPickView";
    private const string GOLD_PICK_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Tiger/skeleton_SkeletonData";
    private const string GOLD_PICK_ANIMAL_ANIMATION_NAME = "3";
    private const string RAPID_PAY_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Phoenix/skeleton_SkeletonData";
    private const string LUCKY_DRAW_BACKGROUND_ANIMATION_PATH = "SiXiang/Spine/LuckyDraw/BgGame/skeleton_SkeletonData";
    private const string LUCKY_DRAW_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Turle/skeleton_SkeletonData";
    private const string PATH_ANIM_WINRESULT_DP = "SiXiang/Spine/BigWinGoldPick/skeleton_SkeletonData";
    private SiXiangScatterView scatterView;
    private SiXiangRapidPayView rapidPayView;
    private SiXiangGoldPickView goldPickView;
    private SiXiangLuckyDrawView luckyDrawView;
    
    public void OnSelectBonusGame(int index)
    {
        effectContainer.gameObject.SetActive(true);
        animationAnimal.transform.parent.gameObject.SetActive(true);
        animationAnimal.gameObject.SetActive(true);
        string animationPath = "", animationName = "";
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.SHOW_ANIMAL);
        tweenQueue.Enqueue(() => ShowAnimationCutScene());

        switch (index)
        {
            case 0:
                animationPath = DRAGON_PEARL_ANIMAL_ANIMATION_PATH;
                animationName = "animation";
                // tweenQueue.Enqueue(() => ShowAnimationCutScene());
                break;
            case 1:
                animationPath = GOLD_PICK_ANIMAL_ANIMATION_PATH;
                animationName = GOLD_PICK_ANIMAL_ANIMATION_NAME;
                tweenQueue.Enqueue(() => ShowGoldPickView());
                break;
            case 2:
                animationPath = RAPID_PAY_ANIMAL_ANIMATION_PATH;
                animationName = "animation";
                // tweenQueue.Enqueue(() => ShowAnimationCutScene());
                break;
            case 3:
                animationPath = LUCKY_DRAW_ANIMAL_ANIMATION_PATH;
                animationName = "animation";
                // tweenQueue.Enqueue(() => ShowAnimationCutScene());
                break;
        }
        Debug.Log("ANIMATION PATH: " + animationPath + " ANIMATION NAME: " + animationName);
        Utility.PlayAnimationByPath(animationAnimal, animationPath, animationName, false);

        animationAnimal.AnimationState.Complete += delegate
        {
            animationAnimal.transform.parent.gameObject.SetActive(false);
            animationAnimal.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            // Show animation Cut Scene
            NextTween();
        };
    }

    public void ShowAnimationCutScene()
    {
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.CUT_SCENE);
        animationCutScene.gameObject.SetActive(true);
        Utility.PlayAnimation(animationCutScene, "animation", false);
        DOTween.Sequence().AppendInterval(1.5f).AppendCallback(() =>
        {
            // Show BonusGame view
            NextTween();
        }
        );
        animationCutScene.AnimationState.Complete += delegate
        {
            animationAnimal.transform.parent.gameObject.SetActive(false);
            animationAnimal.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
        };
    }

    private void ShowGoldPickView()
    {
        SiXiangGoldPickView goldPickView = Instantiate(UIManager.Instance.LoadPrefab(GOLD_PICK_PREFAB_PATH), transform).GetComponent<SiXiangGoldPickView>();
        goldPickView.transform.SetSiblingIndex(animationCutScene.transform.GetSiblingIndex() - 3);
        goldPickView.SetInfo(this);
        backgroundGoldPick.gameObject.SetActive(true);
    }

    public void HideBackgroundGoldPick()
    {
        backgroundGoldPick.gameObject.SetActive(false);
    }

    private void ShowLuckyDrawView()
    {
        // SiXiangLuckyDrawView luckyDrawView = UIManager.Instance.GetView<SiXiangLuckyGoldView>();
    }
}
