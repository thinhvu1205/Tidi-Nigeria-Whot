using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
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
    [SerializeField] GameObject doubleDecking;
    private List<Bet> betItemList = new();

    private int currentBetValue = 0;
    private bool isDoubleDecking = false;

    protected override void Awake()
    {
        base.Awake();
        _ = GetListBet();
        doubleDecking.SetActive(Config.currentGameName == Constants.WHOT_GAME_ID);
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
                if (User.UserAccount.Profile.Balance >= (int)betItem.MarkUnit)
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
        string customData = "";
        if (isDoubleDecking)
        {
            customData = "{\"is_double_decking\": true}";
        }
        _ = UIManager.Instance.HandleCreateMatch(passwordInputField.text, currentBetValue, customData);
        Hide();
    }

    private async UniTask GetListBet()
    {
        UIManager.Instance.ShowProgressing();
        Bets bets = await DataSender.GetListBet(Config.currentGameName);
        UIManager.Instance.HideProgressing();
        betItemList = bets.Bets_ .Where(bet => bet.Enable) .ToList();
        if (betItemList.Count > 0)
        {
            slider.minValue = (float)1 / betItemList.Count;
            currentBetValue = (int)betItemList[0].MarkUnit;
            betValueText.text = Utility.FormatMoney(currentBetValue);
        }
    }
}
