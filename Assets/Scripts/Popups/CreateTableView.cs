using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateTableView : BaseView
{
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] Slider slider;
    [SerializeField] Button createButton, minusButton, plusButton, doubleDeckingCheckbox;
    [SerializeField] TextMeshProUGUI betValueText;
    [SerializeField] Image tickImage;
    private List<Bet> betItemList = new();

    private int currentBetValue = 0;
    private bool isDoubleDecking = false;

    protected override void Awake()
    {
        base.Awake();
        GetListBet().Forget();
    }

    public void OnClickCheckboxDoubleDecking()
    {
        isDoubleDecking = !isDoubleDecking;
        tickImage.gameObject.SetActive(isDoubleDecking);
    }

    public void OnClickPlus()
    {
        if (betItemList.Count == 0) return;
        float step = 1f / betItemList.Count;

        slider.value = Mathf.Clamp(slider.value + step + 0.001f, 0f, 1f); 
        HandleSliderBet();
    }

    public void OnClickMinus()
    {
        if (betItemList.Count == 0) return;
        float step = 1f / betItemList.Count;

        slider.value = Mathf.Clamp(slider.value - step + 0.001f, 0f, 1f); 
        HandleSliderBet();
    }

    public void HandleSliderBet()
    {
        for (int i = 0, l = betItemList.Count; i < l; i++)
        {
            Bet betItem = betItemList[i];
            if (slider.value >= (float)(i + 1.0f) / l)
            {
                if (int.Parse(User.userMain.accountChip) >= (int)betItem.MarkUnit)
                {
                    createButton.interactable = true;
                }
                else
                {
                    createButton.interactable = false;
                }
                currentBetValue = (int)betItem.MarkUnit;
                betValueText.text = Utility.FormatMoney(currentBetValue);

            }

        }
    }

    public void OnClickCreateTable()
    {
        
    }

    private async UniTask GetListBet()
    {
        Bets bets = await DataSender.GetListBet(Config.currentGameId);
        betItemList = bets.Bets_.ToList();
        if (betItemList.Count > 0)
        {
            slider.minValue = (float)1 / betItemList.Count;
            currentBetValue = (int)betItemList[0].MarkUnit;
            betValueText.text = Utility.FormatMoney(currentBetValue);
        }
    }
}
