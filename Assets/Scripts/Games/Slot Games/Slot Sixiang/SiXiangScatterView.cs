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
    [SerializeField] private Transform spinContainer;

    [SerializeField] private TextNumberControl textChipWin;
    [SerializeField] TextMeshProUGUI[] listGoldValue;

    [SerializeField] private Button buttonCollect, buttonSpin;
    [SerializeField]
    SkeletonGraphic animationBackgroundWin, animationLight, animationBackground, animationButtonSpin, animationResultSpin;

    private bool isPrepareStop = false;
    private int typeResult = 5;
    private long winAmount = 0, currentBetLevel = 0;
    private bool isWaitForAutoSpin = true;
    private enum RESULT_SPIN
    {

        COIN_1 = 0,
        GOLD_PICK = 7,
        COIN_2 = 6,
        RAPID_PAY = 5,
        COIN_4 = 4,
        LUCKY_DRAW = 3,
        COIN_5 = 2,
        DRAGON_PEARL = 1,
    }
    [HideInInspector]
    SlotSixiangView gameView;


    private void OnEnable()
    {
        isWaitForAutoSpin = true;
        buttonSpin.interactable = true;
        DOTween.Sequence().AppendInterval(10).AppendCallback(() =>
        {
            if (isWaitForAutoSpin)
            {
                OnClickSpin();
            }
        });
    }

    public void SetInfo(SlotSixiangView slotSixiangView, long betValue)
    {
        gameView = slotSixiangView;
        currentBetLevel = betValue;
        int[] listRateGold = new int[] { 3, 6, 10, 15 };
        for (int i = 0; i < listGoldValue.Length; i++)
        {
            Debug.Log("SET GOLD : " + betValue);
            listGoldValue[i].text = Utility.FormatMoney2(listRateGold[i] * betValue, true);  
        }

    }
    
    public void OnClickSpin()
    {
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.CLICK);
        isWaitForAutoSpin = false;
        buttonSpin.interactable = false;
        Utility.PlayAnimation(animationButtonSpin, "spin normal", true);
        Utility.PlayAnimation(animationBackground, "spin", true);
        InfoBet infoBet = new()
        {
            Chips = currentBetLevel,
        };
        DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
    }
    public void startSpin()
    {
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.SCATTER_SPIN);
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.SPIN_REEL);
        float startAngle = 0;
        int deltaAngle = typeResult * 45;
        int totalAngle = 4320 + deltaAngle;
        DOTween.To(() => startAngle, x => startAngle = x, totalAngle, 5.0f).OnUpdate(() =>
        {
            reel.transform.localEulerAngles = new Vector3(0, 0, startAngle);
            if (startAngle > 3000 && isPrepareStop == false)
            {
                prepareStop();
            }
        }).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            animationBackgroundWin.Initialize(true);
            animationBackgroundWin.AnimationState.SetAnimation(0, "khung eat", true);
            animationBackgroundWin.gameObject.SetActive(true);
            animationLight.gameObject.SetActive(false);
            animationBackground.AnimationState.SetAnimation(0, "normal", true);
            // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.SCATTER_SYMBOL);
            preShowResult();
        });
    }
    public async Task handleScatterSpin(JObject data)
    {
        int reward = (int)data["reward"];
        winAmount = (int)data["winAmount"];
        switch (reward)
        {
            case 0:
                typeResult = 1;
                break; // dragon pearl
            case 1:
                typeResult = 3;
                break; // lucky draw
            case 2:
                typeResult = 7;
                break; // gold pick - (1 + 8)
            case 3:
                typeResult = 5;
                break; // rapid pay - (3 + 8)
            case 4:
                typeResult = 4; // x3
                break;
            case 5:
                typeResult = 6; // x6 - (2 + 8)
                break;
            case 6:
                typeResult = 0; // x10 - (0 + 8)
                break;
            case 7:
                typeResult = 2; // x15
                break;
        }
        // await startSpin();
    }
    private void prepareStop()
    {
        isPrepareStop = true;
        spinContainer.transform.DOLocalMoveY(-331, 1.0f).SetEase(Ease.InSine);
        spinContainer.transform.DOScale(new Vector3(1.5f, 1.5f, 1), 1.0f).SetEase(Ease.InSine);

    }
    private async void preShowResult()
    {
        spinContainer.transform.DOLocalMoveY(-39, 1.0f).SetEase(Ease.OutSine);
        spinContainer.transform.DOScale(new Vector3(1.0f, 1.0f, 1), 1.0f).SetEase(Ease.OutSine).SetId("nodeSpin");
        Tween nodeSpinTween = DOTween.TweensById("nodeSpin")[0];
        await nodeSpinTween.AsyncWaitForCompletion();
        await Task.Delay(1000);

        await showResultAnim();
    }
    private async Task showResultAnim()
    {
        buttonCollect.gameObject.SetActive(false);
        string pathSkeData = "";
        string animName = "";
        if (typeResult % 2 != 0)
        {
            // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.SHOW_ANIMAL);
            switch (typeResult)
            {
                case (int)RESULT_SPIN.DRAGON_PEARL:
                    {
                        pathSkeData = "GameView/SiXiang/Spine/Animal/Dragon/skeleton_SkeletonData";
                        animName = "animation";
                        break;
                    }
                case (int)RESULT_SPIN.GOLD_PICK:
                    {
                        pathSkeData = "GameView/SiXiang/Spine/Animal/Tiger/skeleton_SkeletonData";
                        animName = "3";
                        break;
                    }
                case (int)RESULT_SPIN.RAPID_PAY:
                    {
                        animName = "animation";
                        pathSkeData = "GameView/SiXiang/Spine/Animal/Phoenix/skeleton_SkeletonData";
                        break;
                    }
                case (int)RESULT_SPIN.LUCKY_DRAW:
                    {
                        animName = "animation";
                        pathSkeData = "GameView/SiXiang/Spine/Animal/Turle/skeleton_SkeletonData";
                        break;
                    }
            }
            textChipWin.gameObject.SetActive(false);

        }
        else
        {
            animName = "eng";
            pathSkeData = "GameView/SiXiang/Spine/WinResult/skeleton_SkeletonData";
            textChipWin.gameObject.SetActive(true);
            //Globals.Config.tweenNumberToNumber(lbChipWins, winAmount, 0, 2.0f);
            // AudioSource soundMoney = SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_START);
            textChipWin.SetValue(winAmount, true, 2.0f, "", () =>
            {
                // soundMoney.Stop();
                // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_END);
            });

        }
        // animResultSpin.skeletonDataAsset = UIManager.instance.loadSkeletonData(pathSkeData);
        animationResultSpin.Initialize(true);
        animationResultSpin.AnimationState.SetAnimation(0, animName, false);
        animationResultSpin.transform.parent.gameObject.SetActive(true);
        await Task.Delay((int)animationResultSpin.Skeleton.Data.FindAnimation(animName).Duration * 1000);
        if (typeResult % 2 != 0)
        {
            endView();
        }
        else
        {
            buttonCollect.gameObject.SetActive(true);
            // if (gameView.spinType == BaseSlotSymbolView.SPIN_TYPE.AUTO)
            // {
            //     DOTween.Sequence()
            //         .AppendInterval(5.0f)
            //         .AppendCallback(() =>
            //         {
            //             endView();
            //         })
            //         .SetId("autoEnd");
            // }
        }
    }
    public void onClickCollect()
    {
        DOTween.Kill("autoEnd");
        endView();
    }
    private async void endView()
    {
        animationResultSpin.transform.parent.gameObject.SetActive(false);
        // await gameView.showAnimCutScene();

        Destroy(gameObject);
        reel.transform.localEulerAngles = Vector3.zero;
        if (typeResult == (int)RESULT_SPIN.COIN_1 || typeResult == (int)RESULT_SPIN.COIN_2 || typeResult == (int)RESULT_SPIN.COIN_4 || typeResult == (int)RESULT_SPIN.COIN_5)
        {
            JObject dataEnd = new JObject();
            dataEnd["winAmount"] = winAmount;
            // dataEnd["gameType"] = (int)SlotSixiangView.GAME_TYPE.SCATTER;
            dataEnd["isSelectBonusGame"] = false;
            // await SlotSixiangView.Instance.endMinigame(dataEnd);
        }


    }

}
