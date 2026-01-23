using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;
using Globals;
using TMPro;
using Proto;
using System;
using System.Linq;


public class SiXiangRapidPayView : MonoBehaviour
{
    [SerializeField] List<RapidPayRow> listRows;

    [SerializeField] private SkeletonGraphic animationLight, animationBackgroundRow, animationResult;
    [SerializeField] private TextMeshProUGUI textTotalBonus;
    [SerializeField] private TextNumberControl textWinResult, textWinAmount;
    [SerializeField] private Button buttonCollect;
    private const string RESULT_ANIMATION_PATH = "SiXiang/Spine/BigWinRapid/skeleton_SkeletonData";
    private const string LIGHT_ANIMATION_PATH = "SiXiang/Spine/RapidLight/skeleton_SkeletonData";
    private SlotSixiangView gameView;
    private RapidPayRow currentRow;
    private AudioSource soundMoney;
    private int indexRow = 0;
    private long multiplierBonus = 1;
    private long winAmount = 0;
    private bool canSkipAnimation = false;

    private void OnDestroy()
    {
        gameView.OnUpdateTable -= SixiangView_OnUpdateTable;
        DOTween.Kill("autoKillRapidPayResult");
    }

    public void SetInfo(SlotSixiangView slotSixiangView)
    {
        gameView = slotSixiangView;
        gameView.OnUpdateTable += SixiangView_OnUpdateTable;
        SetInitView();
    }

    private void SetInitView()
    {
        currentRow = listRows[indexRow];
        currentRow.ActiveAllButtons();
        animationBackgroundRow.transform
            .DOLocalMoveY(animationBackgroundRow.transform.localPosition.y + 123 * indexRow - indexRow * 3.5f, 0.3f)
            .SetEase(Ease.InSine);
            Debug.Log("IS BUNOS RAPID PAY: " + (gameView.GetCurrentGame() == SiXiangGame.SixangbonusRapidpay));
        if (gameView.GetCurrentGame() == SiXiangGame.SixangbonusRapidpay)
        {
            textWinAmount.SetValue(gameView.GetCurrentBetLevel() * 2, true, 0.5f);
            multiplierBonus = 4;
        }
        else
        {
            textWinAmount.SetValue(gameView.GetCurrentBetLevel() / 2, true, 0.5f);
        }
        textTotalBonus.text = "x" + multiplierBonus;
        foreach(RapidPayRow rapidPayRow in listRows)
        {
            rapidPayRow.Reset();
        }
    }

    private void SixiangView_OnUpdateTable(SlotSixiangView.OnUpdateTableEventArgs e)
    {
        SlotDesk data = e.data;
        if (data.SpinSymbols.Count == 0)
        {
            // Setup data cũ
            indexRow = data.Matrix.SpinLists.Where(spinSymbol => spinSymbol.Symbol != SiXiangSymbol.Unspecified).ToList().Count;
            SetInitView();
            foreach(RapidPayRow row in listRows)
            {
                row.SetupData(data);
            }
            multiplierBonus =  data.GameReward.TotalChipsWinByGame / (gameView.GetCurrentBetLevel() / 2);
            textTotalBonus.text = "x" + multiplierBonus;
            textWinAmount.SetValue(data.GameReward.TotalChipsWinByGame, true, 0.5f);
            return;
        }
        SpinSymbol item = data.SpinSymbols[0];
        Debug.Log("CURRENT ROW: " + indexRow);
        currentRow.SetResult(data);
        winAmount = data.GameReward.TotalChipsWinByGame;
        int indexPick = item.Index;


        if (indexPick >= 0 && item.Symbol != SiXiangSymbol.RapidpayEnd)
        {
            animationLight.gameObject.SetActive(true);
            Utility.PlayAnimationByPath(animationLight, LIGHT_ANIMATION_PATH, GetAnimationLightName(indexPick), false);
            switch (item.Symbol)
            {
                case SiXiangSymbol.RapidpayX2:
                    multiplierBonus *= 2;
                    break;
                case SiXiangSymbol.RapidpayX3:
                    multiplierBonus *= 3;
                    break;
                case SiXiangSymbol.RapidpayX4:
                    multiplierBonus *= 4;
                    break;
            }
            SoundManager.Instance.PlayEffectFromPath(SoundSlot.RAPID_CHIP_FLY);
        }
        DOTween.Sequence()
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                textTotalBonus.text = "x" + multiplierBonus;
                animationLight.gameObject.SetActive(false);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                textWinAmount.SetValue(winAmount, true, 0.5f);
                if (!data.IsFinishGame) NextRow();
                else ShowResult();
            });
    }

    private string GetAnimationLightName(int index)
    {
        Debug.Log("ANIMATION LIGHT INDEX: " + index);
        string name = "";
        name = index switch
        {
            0 => "18",
            1 => "17",
            5 => "16",
            6 => "15",
            7 => "14",
            10 => "13",
            11 => "12",
            12 => "11",
            13 => "10",
            15 => "9",
            16 => "8",
            17 => "7",
            18 => "6",
            20 => "5",
            21 => "4",
            22 => "3",
            23 => "2",
            24 => "1",
            _ => name
        };
        return name;
    }

    private void NextRow()
    {
        Debug.Log("NEXT ROW");
        animationBackgroundRow.transform
            .DOLocalMoveY(animationBackgroundRow.transform.localPosition.y + 123 - indexRow * 3.5f, 0.3f)
            .SetEase(Ease.InSine);
        indexRow++;
        currentRow = listRows[indexRow];
        currentRow.ActiveAllButtons();
    }

    private void ShowResult()
    {
        canSkipAnimation = true;
        animationResult.transform.parent.gameObject.SetActive(true);
        Utility.PlayAnimationByPath(animationResult, RESULT_ANIMATION_PATH, "eng", false); 
        buttonCollect.gameObject.SetActive(false);
        soundMoney = SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_START);
        float timeRun = 2f;
        textWinResult.SetValue(winAmount, true, timeRun, "", () =>
        {
            OnAnimationResultFinish();
        });
    }

    private void OnAnimationResultFinish()
    {
        textWinResult.SetValue(winAmount, false);
        buttonCollect.gameObject.SetActive(true);
        if (soundMoney != null)
        {
            soundMoney.Stop();
        }
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_END);
        DOTween.Sequence().SetId("autoKillRapidPayResult").AppendInterval(10.0f)
            .AppendCallback(() =>
            {
                if (animationResult.gameObject.activeSelf)
                {
                    OnClickCollect();
                }
            });
    }

    public void OnClickCollect()
    {
        canSkipAnimation = false;
        animationResult.transform
            .DOScale(new Vector2(0.8f, 0.8f), 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                animationResult.transform.parent.gameObject.SetActive(false);
                Reset();
                gameView.ShowAnimationCutScene(true);
            });
    }

    private void Reset()
    {
        indexRow = 0;
        multiplierBonus = 1;   
    }
}
