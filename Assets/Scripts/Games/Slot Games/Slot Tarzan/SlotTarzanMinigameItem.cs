using System.Collections;
using System.Collections.Generic;
using Proto;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class SlotTarzanMinigameItem : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic scoreAnimation;
    [SerializeField] private TextMeshProUGUI valueText;
    private SlotTarzanMiniGameView miniGameView;
    public int Index { get; set; } = 0;
    public bool IsOpen { get; set; } = false;

    public void ShowResult(SiXiangSymbol symbol, long winAmount)
    {
        IsOpen = true;

        scoreAnimation.gameObject.SetActive(true);
        Utility.PlayAnimation(scoreAnimation, "animation", true);
        scoreAnimation.AnimationState.Complete += delegate
        {
            scoreAnimation.gameObject.SetActive(false);
        };
        if (symbol == SiXiangSymbol.TarzanMoreTurnx2) {
            valueText.text = "+2 PICKS";
            // this.effectFlyMoney(leftBonus, 2, 50, -28, 37);
        } else if (symbol == SiXiangSymbol.TarzanMoreTurnx3) {
            valueText.text = "+3 PICKS";
            // this.effectFlyMoney(leftBonus, 3, 50, -28, 37);
        } else {
            valueText.text = Utility.FormatMoney2(winAmount, true);
        }
        valueText.gameObject.SetActive(true);
        valueText.transform.localScale = Vector2.zero;
        valueText.transform
            .DOScale(new Vector2(1.0f, 1.0f), 0.1f).SetEase(Ease.OutBack);
            // .OnComplete(() =>
            // {
            //     if (id == 0 || id == 15 || id == 16)
            //     {
            //         SlotTarzanMiniGameView.instance.addPickTurn(this, value);
            //     }
            //     if (SlotTarzanMiniGameView.instance.pickLeft == 0 && idItem != 0 && idItem != 15 && idItem != 16)
            //     {
            //         DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() =>
            //         {
            //             SlotTarzanMiniGameView.instance.showPopupResult();
            //         });
            //     }
            //     SlotTarzanMiniGameView.instance.canClick = true;
            // });
        
    }
    public void Reset()
    {
        IsOpen = false;
        valueText.text = "?";
        scoreAnimation.gameObject.SetActive(false);
    }
}
