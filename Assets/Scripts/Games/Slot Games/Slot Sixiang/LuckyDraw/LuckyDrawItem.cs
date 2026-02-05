using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using Proto;
using Globals;

public class LuckyDrawItem : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private SkeletonGraphic spine;
    [SerializeField] private TextMeshProUGUI textChipWin;
    public SiXiangLuckyDrawView.JackpotType typeItem = SiXiangLuckyDrawView.JackpotType.NORMAL;
    private readonly string[] listAnimName = new string[] { "minor", "major", "mega", "grand" };

    public void Init(SpinSymbol item)
    {
        string animationNormal = "normal_";
        string typeAnim = "normal";
        switch (item.Symbol)
        {
            case SiXiangSymbol.LuckydrawMinor:
                typeAnim = "minor";
                typeItem = SiXiangLuckyDrawView.JackpotType.MINOR;
                break;
            case SiXiangSymbol.LuckydrawMajor:
                typeAnim = "major";
                typeItem = SiXiangLuckyDrawView.JackpotType.MAJOR;
                break;
            case SiXiangSymbol.LuckydrawMega:
                typeAnim = "mega";
                typeItem = SiXiangLuckyDrawView.JackpotType.MEGA;
                break;
            case SiXiangSymbol.LuckydrawGrand:
                typeAnim = "grand";
                typeItem = SiXiangLuckyDrawView.JackpotType.GRAND;
                break;
            default:
                typeAnim = "normal";
                animationNormal = "";
                typeItem = SiXiangLuckyDrawView.JackpotType.NORMAL;
                break;
        }
        string animationName = animationNormal + typeAnim;
        Utility.PlayAnimation(spine, animationName, true);

        if (typeItem == (int)SiXiangLuckyDrawView.JackpotType.NORMAL && item.WinAmount > 0)
        {
            textChipWin.gameObject.SetActive(true);
            textChipWin.text = Utility.FormatMoney(item.WinAmount, true);
        }
    }

    public void SetResult(SpinSymbol item, bool isFinishGame)
    {
        string animationFlip = "quay_";
        string animationNormal = "normal_";
        string typeAnim = "normal";
        long winAmount = item.WinAmount;
        switch (item.Symbol)
        {
            case SiXiangSymbol.LuckydrawMinor:
                typeAnim = "minor";
                typeItem = SiXiangLuckyDrawView.JackpotType.MINOR;
                break;
            case SiXiangSymbol.LuckydrawMajor:
                typeAnim = "major";
                typeItem = SiXiangLuckyDrawView.JackpotType.MAJOR;
                break;
            case SiXiangSymbol.LuckydrawMega:
                typeAnim = "mega";
                typeItem = SiXiangLuckyDrawView.JackpotType.MEGA;
                break;
            case SiXiangSymbol.LuckydrawGrand:
                typeAnim = "grand";
                typeItem = SiXiangLuckyDrawView.JackpotType.GRAND;
                break;
            default:
                typeAnim = "normal";
                animationFlip = "";
                animationNormal = "";
                typeItem = SiXiangLuckyDrawView.JackpotType.NORMAL;
                break;
        }
        string animationName = animationFlip + typeAnim;
        float duration = spine.Skeleton.Data.FindAnimation(animationName).Duration;
        Utility.PlayAnimation(spine, animationName, false);
        if (typeItem == (int)SiXiangLuckyDrawView.JackpotType.NORMAL)
        {
            Vector2 initPos = transform.localPosition;
            DOTween.Sequence()
                .SetId("showNormal")
                .Append(transform.DOScale(new Vector2(0.85f, 0.85f), 0.4f))
                .Join(transform.DOLocalMoveX(initPos.x - 20, 0.1f).OnComplete(() =>
                {
                    transform.DOLocalMoveX(initPos.x, 0.1f).OnComplete(() =>
                    {
                        transform.DOLocalMoveX(initPos.x + 20, 0.1f).OnComplete(() =>
                        {
                            transform.DOLocalMoveX(initPos.x, 0.1f);
                        });
                    });
                }))
                .AppendCallback(() =>
                {
                    SoundManager.Instance.PlayEffectFromPath(SoundSlot.LUCKYDRAW_ITEM_NORMAL);
                    DOTween.Sequence()
                        .Append(
                        transform.DOScale(new Vector2(1.1f, 1.1f), 0.1f).SetEase(Ease.OutBack).OnComplete(() =>
                        {
                            textChipWin.gameObject.SetActive(true);
                            textChipWin.text = Utility.FormatMoney(winAmount, true);
                        }))
                        .Append(transform.DOScale(Vector2.one, 0.1f));
                });
        }
        else
        {
            float timeDelayAnim = spine.Skeleton.Data.FindAnimation(animationFlip + typeAnim).Duration;
            if (isFinishGame)
            {
                animationName = "win_" + typeAnim;
            }
            else
            {
                animationName = animationNormal + typeAnim;
            }

            DOTween.Sequence()
                .AppendInterval(timeDelayAnim / 2)
                .AppendCallback(() =>
                {
                    SoundManager.Instance.PlayEffectFromPath(SoundSlot.LUCKYDRAW_ITEM_JACKPOT);
                })
                .AppendInterval(timeDelayAnim)
                .AppendCallback(() =>
                {
                    spine.AnimationState.SetAnimation(0, animationName, true);
                });
        }


    }

    public void ShowEffectWinJackpot()
    {
        Reset();
        spine.AnimationState.SetAnimation(0, "win_" + listAnimName[(int)typeItem - 1], true);
    }

    public void Reset()
    {
        spine.AnimationState.GetCurrent(0).TimeScale = 0;
        spine.AnimationState.GetCurrent(0).Reset();
        spine.Initialize(true);
        spine.timeScale = 1;
    }

    // Update is called once per frame

}
