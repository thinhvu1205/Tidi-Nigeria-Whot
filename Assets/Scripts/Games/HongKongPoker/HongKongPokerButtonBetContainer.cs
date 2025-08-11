using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HongKongPokerButtonBetContainer : MonoBehaviour
{
    [SerializeField] GameObject buttonRaise, buttonCall, buttonAllIn;

    [SerializeField] TextMeshProUGUI textButtonRaise, textButtonCall, textBet, textMax;

    [SerializeField] Image sliderHandle;

    [SerializeField] Slider slider;
    private float betValue = 0, playerCurrentChipValue = 0, markUnitValue = 0, potValue = 0;

    public void SetValues(long playerCurrentChipValue, int markUnitValue, int potValue)
    {
        this.playerCurrentChipValue = playerCurrentChipValue;
        this.markUnitValue = markUnitValue;
        this.potValue = potValue;

        betValue = markUnitValue;
        textBet.text = Utility.FormatMoney((int)markUnitValue);
        if (playerCurrentChipValue <= 0)
        {
            buttonAllIn.SetActive(true);
            buttonRaise.SetActive(false);
            buttonCall.SetActive(false);
        }
        else
        {
            buttonRaise.SetActive(true);
            buttonCall.SetActive(true);
            buttonAllIn.SetActive(false);
        }

    }

    public void OnValueChange()
    {
        float valueMoney = 0;
        float progress = slider.value;

        if (progress <= 0.7f)
        {

            valueMoney = Mathf.FloorToInt((progress * playerCurrentChipValue / 2) * (1 / 0.7f));
            if (progress <= markUnitValue / playerCurrentChipValue / 0.7)
            {
                slider.value = markUnitValue / playerCurrentChipValue / 0.7f;
            }
        }
        else
        {
            valueMoney = Mathf.FloorToInt(playerCurrentChipValue / 2 + ((progress - 0.7f) * playerCurrentChipValue / 2 * (1 / 0.3f)));
            if (progress >= 0.99)
            {
                slider.value = 1;
            }
        }

        textMax.gameObject.SetActive(Mathf.Approximately(slider.value, 1));
        sliderHandle.fillAmount = slider.value;
        if (valueMoney <= markUnitValue)
        {
            valueMoney = markUnitValue;
            textBet.text = Utility.FormatMoney((int)markUnitValue);
        }
        else if (valueMoney < playerCurrentChipValue)
        {
            textBet.text = Utility.FormatMoney((int)valueMoney);
        }
        else
        {
            textBet.text = Utility.FormatMoney((int)Mathf.Floor(playerCurrentChipValue));
            valueMoney = playerCurrentChipValue;
        }
        betValue = valueMoney;

    }

    public void OnClickFold()
    {

    }

    public void OnClickCall()
    {

    }

    public void OnClickRaise()
    {
        buttonRaise.SetActive(false);
        // SoundManager.instance.soundClick();
        // isClickRaise = true;
        slider.transform.parent.gameObject.SetActive(true);
        ResetSlider();
    }

    public void OnClickAllIn()
    {

    }

    public void OnClickConfirm()
    {

    }

    public void OnClick1In2()
    {

    }

    public void OnClick1In4()
    {

    }

    public void OnClick1In8()
    {

    }

    private void ResetSlider()
    {
        slider.value = markUnitValue / playerCurrentChipValue / 0.7f >= 1 ? 1 : markUnitValue / playerCurrentChipValue / 0.7f;
        sliderHandle.fillAmount = slider.value;
        textBet.text = Utility.FormatMoney((int)markUnitValue);
    }
}
