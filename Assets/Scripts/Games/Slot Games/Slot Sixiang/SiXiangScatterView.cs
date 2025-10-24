using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using Globals;
using Proto;
using Google.Protobuf;

public class SiXiangScatterView : MonoBehaviour
{
    [SerializeField] private GameObject reel;
    [SerializeField] private Transform spinContainer, resultContainer;

    [SerializeField] private TextNumberControl textChipWin;
    [SerializeField] TextMeshProUGUI[] listGoldValue;

    [SerializeField] private Button buttonCollect, buttonSpin;
    [SerializeField]
    SkeletonGraphic animationBackgroundWin, animationLight, animationBackground, animationButtonSpin, animationResultSpin;

    private bool isPrepareStop = false;
    private int typeResult = 5;
    private long winAmount = 0, currentBetLevel = 0;
    private bool isWaitForAutoSpin = true;
    private SpinSymbol spinResult;
    SlotSixiangView gameView;


    private void OnEnable()
    {
        winAmount = currentBetLevel = 0;
        typeResult = 5;
        isWaitForAutoSpin = true;
        buttonSpin.interactable = true;
        Utility.PlayAnimation(animationButtonSpin, "spin_anim", true);
        Utility.PlayAnimation(animationLight, "light run", true);

        DOTween.Sequence().AppendInterval(10).AppendCallback(() =>
        {
            if (isWaitForAutoSpin)
            {
                OnClickSpin();
            }
        });
    }
    
    private void OnDisable()
    {
        gameView.OnUpdateTable -= SixiangView_OnUpdateTable;
    }

    public void SetInfo(SlotSixiangView slotSixiangView, long betValue)
    {
        gameView = slotSixiangView;
        currentBetLevel = betValue;
        gameView.OnUpdateTable += SixiangView_OnUpdateTable;

        int[] listRateGold = new int[] { 3, 10, 6, 15 };
        for (int i = 0; i < listGoldValue.Length; i++)
        {
            Debug.Log("SET GOLD : " + betValue);
            listGoldValue[i].text = Utility.FormatMoney2(listRateGold[i] * betValue, true);
        }
    }

    public void SixiangView_OnUpdateTable(SlotSixiangView.OnUpdateTableEventArgs e)
    {
        SlotDesk data = e.data;
        if (data.SpinSymbols.Count == 0) return;
        spinResult = data.SpinSymbols[0];
        winAmount = data.GameReward.TotalChipsWinByGame;
        switch (spinResult.Symbol)
        {
            case SiXiangSymbol.BonusDragonball:
                typeResult = 1;
                break; // dragon pearl
            case SiXiangSymbol.BonusLuckydraw:
                typeResult = 3;
                break; // lucky draw
            case SiXiangSymbol.BonusGoldpick:
                typeResult = 7;
                break; // gold pick - (1 + 8)
            case SiXiangSymbol.BonusRapidpay:
                typeResult = 5;
                break; // rapid pay - (3 + 8)
            case SiXiangSymbol.BonusGoldx10:
                typeResult = 4; // x3
                break;
            case SiXiangSymbol.BonusGoldx20:
                typeResult = 6; // x6 - (2 + 8)
                break;
            case SiXiangSymbol.BonusGoldx30:
                typeResult = 0; // x10 - (0 + 8)
                break;
            case SiXiangSymbol.BonusGoldx50:
                typeResult = 2; // x15
                break;
        }
        StartSpin();
    }

    public void OnClickSpin()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        isWaitForAutoSpin = false;
        buttonSpin.interactable = false;
        

        InfoBet infoBet = new()
        {
            Chips = currentBetLevel,
        };
        DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
    }
    public void StartSpin()
    {
        Utility.PlayAnimation(animationButtonSpin, "spin normal", true);
        Utility.PlayAnimation(animationBackground, "spin", true);
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.SCATTER_SPIN);
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.SPIN_REEL);
        float startAngle = 0;
        int deltaAngle = typeResult * 45;
        int totalAngle = 4320 + deltaAngle;
        DOTween
            .To(() => startAngle, x => startAngle = x, totalAngle, 5.0f)
            .OnUpdate(() =>
            {
                reel.transform.localEulerAngles = new Vector3(0, 0, startAngle);
                if (startAngle > 3000 && !isPrepareStop)
                {
                    PrepareStop();
                }
            })
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                Utility.PlayAnimation(animationBackgroundWin, "khung eat", true);
                Utility.PlayAnimation(animationBackground, "normal", true);
                animationLight.gameObject.SetActive(false);

                SoundManager.Instance.PlayEffectFromPath(SoundSlot.SCATTER_SYMBOL);
                PreShowResult();
            });
    }

    private void PrepareStop()
    {
        isPrepareStop = true;
        spinContainer.transform.DOLocalMoveY(-331, 1.0f).SetEase(Ease.InSine);
        spinContainer.transform.DOScale(new Vector3(1.5f, 1.5f, 1), 1.0f).SetEase(Ease.InSine);

    }
    private void PreShowResult()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                spinContainer.transform.DOLocalMoveY(-39, 1.0f).SetEase(Ease.OutSine);
                spinContainer.transform.DOScale(new Vector3(1.0f, 1.0f, 1), 1.0f).SetEase(Ease.OutSine);
            })
            .AppendInterval(1f)
            .OnComplete(() =>
            {
                ShowResultAnim();
            });
    }
    private void ShowResultAnim()
    {
        resultContainer.gameObject.SetActive(true);
        buttonCollect.gameObject.SetActive(false);
        string animationPath = "";
        string animationName = "";
        bool isBonusGame = typeResult % 2 != 0;
        if (isBonusGame)
        {
            SoundManager.Instance.PlayEffectFromPath(SoundSlot.SHOW_ANIMAL);
            switch (spinResult.Symbol)
            {
                case SiXiangSymbol.BonusDragonball:
                    {
                        animationPath = "SiXiang/Spine/Animal/Dragon/skeleton_SkeletonData";
                        animationName = "animation";
                        gameView.TweenQueue.Enqueue(() => gameView.ShowDragonPearlView());
                        break;
                    }
                case SiXiangSymbol.BonusGoldpick:
                    {
                        animationPath = "SiXiang/Spine/Animal/Tiger/skeleton_SkeletonData";
                        animationName = "3";
                        gameView.TweenQueue.Enqueue(() => gameView.ShowGoldPickView());

                        break;
                    }
                case SiXiangSymbol.BonusRapidpay:
                    {
                        animationName = "animation";
                        animationPath = "SiXiang/Spine/Animal/Phoenix/skeleton_SkeletonData";
                        gameView.TweenQueue.Enqueue(() => gameView.ShowRapidPayView());

                        break;
                    }
                case SiXiangSymbol.BonusLuckydraw:
                    {
                        animationName = "animation";
                        animationPath = "SiXiang/Spine/Animal/Turle/skeleton_SkeletonData";
                        gameView.TweenQueue.Enqueue(() => gameView.ShowLuckyDrawView());
                        break;
                    }
            }
            textChipWin.gameObject.SetActive(false);

        }
        else
        {
            animationName = "eng";
            animationPath = "SiXiang/Spine/WinResult/skeleton_SkeletonData";
            textChipWin.gameObject.SetActive(true);
            //Globals.Config.tweenNumberToNumber(lbChipWins, winAmount, 0, 2.0f);
            AudioSource soundMoney = SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_START);
            textChipWin.SetValue(winAmount, true, 2.0f, "", () =>
            {
                soundMoney.Stop();
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_END);
            });

        }
        Utility.PlayAnimationByPath(animationResultSpin, animationPath, animationName, false);
        animationResultSpin.AnimationState.Complete += delegate
        {
            if (isBonusGame)
            {
                EndView();
            }
            else
            {
                buttonCollect.gameObject.SetActive(true);
                if (gameView.GetSpinType() == SpinType.AUTO)
                {
                    DOTween.Sequence()
                        .AppendInterval(5.0f)
                        .AppendCallback(() =>
                        {
                            EndView();
                        })
                        .SetId("autoEnd");
                }
            }

        };
    }
    public void OnClickCollect()
    {
        DOTween.Kill("autoEnd");
        EndView();
    }
    private void EndView()
    {
        resultContainer.gameObject.SetActive(false);
        gameView.ShowAnimationCutScene(true);
    }
}
