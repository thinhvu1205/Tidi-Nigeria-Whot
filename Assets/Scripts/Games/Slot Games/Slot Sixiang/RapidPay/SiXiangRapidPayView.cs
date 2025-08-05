using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using TMPro;
using Proto;


public class SiXiangRapidPayView : MonoBehaviour
{
    [SerializeField] List<RapidPayRow> listRows;

    [SerializeField] private SkeletonGraphic animationLight, animationBackgroundRow, animationResult;
    [SerializeField] private TextMeshProUGUI textTotalBonus;
    [SerializeField] private TextNumberControl textWinResult, textWinAmount;
    [SerializeField] private Button buttonCollect;

    private SlotSixiangView gameView;
    private RapidPayRow currentRow;
    private int indexRow = 0, multiplierBonus = 1;
    private long winAmount = 0;

    private void OnDisable()
    {
        gameView.OnUpdateTable -= SixiangView_OnUpdateTable;
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
        textTotalBonus.text = "x" + multiplierBonus;
        textWinAmount.SetValue(gameView.GetCurrentBetLevel() / 2, true, 0.5f);

    }

    private void SixiangView_OnUpdateTable(SlotSixiangView.OnUpdateTableEventArgs e)
    {
        SlotDesk data = e.data;
        SpinSymbol item = data.SpinSymbols[0];

        currentRow.SetResult(data);
        winAmount = data.GameReward.TotalChipsWinByGame;
        int indexPick = item.Index;


        if (indexPick >= 0 && item.Symbol != SiXiangSymbol.RapidpayEnd)
        {
            animationLight.gameObject.SetActive(true);
            Utility.PlayAnimation(animationLight, GetAnimationLightName(indexPick), false);
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
            // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.RAPID_CHIP_FLY);
        }
        DOTween.Sequence()
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                textTotalBonus.text = "x" + multiplierBonus;
            })
            .AppendInterval(1.4f)
            .AppendCallback(() =>
            {
                animationLight.gameObject.SetActive(false);
                if (!data.IsFinishGame) NextRow();
                else ShowResult();
            });
    }

    private string GetAnimationLightName(int index)
    {
        string name = "";
        name = index switch
        {
            0 => "17",
            1 => "18",
            5 => "14",
            6 => "15",
            7 => "16",
            10 => "10",
            11 => "11",
            12 => "12",
            13 => "13",
            15 => "6",
            16 => "7",
            17 => "8",
            18 => "9",
            20 => "1",
            21 => "2",
            22 => "3",
            23 => "4",
            24 => "5",
            _ => name
        };
        return name;
    }

    private void NextRow()
    {
        textWinAmount.SetValue(winAmount, true, 0.5f);
        animationBackgroundRow.transform
            .DOLocalMoveY(animationBackgroundRow.transform.localPosition.y + 123 - indexRow * 3.5f, 0.3f)
            .SetEase(Ease.InSine);
        indexRow++;
        currentRow = listRows[indexRow];
        currentRow.ActiveAllButtons();
    }

    private void ShowResult()
    {
        animationResult.transform.parent.gameObject.SetActive(true);
        Utility.PlayAnimation(animationResult, "eng", false); buttonCollect.gameObject.SetActive(false);
        // AudioSource soundMoney = SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_START);
        float timeRun = 2f;
        textWinResult.SetValue(winAmount, true, timeRun, "", () =>
        {
            buttonCollect.gameObject.SetActive(true);
            // soundMoney.Stop();
            // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_END);

        });
    }

    public void OnClickCollect()
    {
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
