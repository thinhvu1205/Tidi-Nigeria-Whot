using System.Collections;
using System.Collections.Generic;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HongKongPokerButtonBetContainer : MonoBehaviour
{
    [SerializeField] GameObject buttonRaise, buttonCall, buttonAllIn, buttonCheck, buttonFold;

    [SerializeField] TextMeshProUGUI textButtonRaise, textButtonCall, textBet, textMax;

    [SerializeField] Image sliderHandle;

    [SerializeField] Slider slider;
    private float betValue = 0, playerCurrentChipValue = 0, markUnitValue = 0, potValue = 0;
    private bool isBetMode = false; // true = Bet mode, false = Raise mode
    private HongKongPokerView view; // Reference to view for sending actions

    void Start()
    {
        slider.onValueChanged.AddListener((vl) =>
        {
            OnValueChange();
        });
    }
    
    public void SetValues(long playerCurrentChipValue, int markUnitValue, int potValue)
    {
        this.playerCurrentChipValue = playerCurrentChipValue;
        this.markUnitValue = markUnitValue;
        this.potValue = potValue;

        betValue = markUnitValue;
        textBet.text = Utility.FormatMoney((int)markUnitValue);
        // if (playerCurrentChipValue <= 0)
        // {
        //     buttonAllIn.SetActive(true);
        //     buttonRaise.SetActive(false);
        //     buttonCall.SetActive(false);
        // }
        // else
        // {
        //     buttonRaise.SetActive(true);
        //     buttonCall.SetActive(true);
        //     buttonAllIn.SetActive(false);
        // }

    }

    private void OnValueChange()
    {
        float rawValue = slider.value;

        // Convert real money -> 0..1
        float normalized = (rawValue - slider.minValue) / (slider.maxValue - slider.minValue);
        normalized = Mathf.Clamp01(normalized);

        float valueMoney = 0;

        if (normalized <= 0.7f)
        {
            valueMoney = Mathf.FloorToInt((normalized / 0.7f) * (playerCurrentChipValue / 2));
        }
        else
        {
            float t = (normalized - 0.7f) / 0.3f;
            valueMoney = Mathf.FloorToInt(playerCurrentChipValue / 2 + t * (playerCurrentChipValue / 2));
        }

        // clamp
        if (valueMoney < slider.minValue) valueMoney = slider.minValue;
        if (valueMoney > slider.maxValue) valueMoney = slider.maxValue;

        betValue = valueMoney;
        Debug.Log("Bet Value "+betValue);
        textBet.text = Utility.FormatMoney((int)betValue);
    }

    public void OnClickFold()
    {
        if (view != null)
        {
            view.OnClickFold();
        }
    }

    public void OnClickCall()
    {
        if (view != null)
        {
            view.OnClickCall();
        }
    }
    
    public void OnClickCheck()
    {
        if (view != null)
        {
            view.OnClickCheck();
        }
    }

    public void OnClickRaise()
    {
        // Hide raise/bet button and show slider
        if (buttonRaise != null)
        {
            buttonRaise.SetActive(false);
        }
        
        // SoundManager.instance.soundClick();
        // isClickRaise = true;
        if (slider != null && slider.transform.parent != null)
        {
            slider.transform.parent.gameObject.SetActive(true);
        }
        // ResetSlider();
    }

    public void OnClickAllIn()
    {
        if (view != null)
        {
            view.OnClickAllIn();
        }
    }

    public void SetViewReference(HongKongPokerView view)
    {
        this.view = view;
    }
    
    public void OnClickConfirm()
    {
        if (view != null && betValue > 0 )
        {
            if (isBetMode)
            {
                // Send BET action
                view.OnClickConfirmBet((int)betValue);
            }
            else
            {
                // Send RAISE action
                view.OnClickConfirmRaise((int)betValue);
            }
        }
        
        // Hide slider
        if (slider != null && slider.transform.parent != null)
        {
            slider.transform.parent.gameObject.SetActive(false);
        }
        
        // Show raise button again
        if (buttonRaise != null)
        {
            buttonRaise.SetActive(true);
        }
    }

    public void OnClick1In2()
    {
        if (view != null)
        {
            // Calculate 1/2 pot, but not less than minRaise
            int betAmount = Mathf.Max((int)(potValue * 0.5f), (int)markUnitValue);
        
            if (isBetMode)
            {
                view.OnClickConfirmBet(betAmount);
            }
            else
            {
                view.OnClickConfirmRaise(betAmount);
            }
        }
    }

    public void OnClick1In4()
    {
        if (view != null)
        {
            // Calculate 1/4 pot, but not less than minRaise
            int betAmount = Mathf.Max((int)(potValue * 0.25f), (int)markUnitValue);

        
            if (isBetMode)
            {
                view.OnClickConfirmBet(betAmount);
            }
            else
            {
                view.OnClickConfirmRaise(betAmount);
            }
        }
    }

    public void OnClick1In8()
    {
        if (view != null)
        {
            // Calculate 1/8 pot, but not less than minRaise
            int betAmount = Mathf.Max((int)(potValue * 0.125f), (int)markUnitValue);
        
            if (isBetMode)
            {
                view.OnClickConfirmBet(betAmount);
            }
            else
            {
                view.OnClickConfirmRaise(betAmount);
            }
        }
    }

    private void ResetSlider()
    {
        if (slider != null)
        {
            Debug.Log("Reset min value "+ slider.minValue);
            slider.value = slider.minValue;
            betValue = slider.value;
        }

        if (sliderHandle != null)
        {
            sliderHandle.fillAmount = 0f;
        }

        if (textBet != null)
        {
            textBet.text = Utility.FormatMoney((int)markUnitValue);
        }
    }

    
    /// <summary>
    /// Reset all data when new game starts or round changes
    /// </summary>
    public void Reset()
    {
        // Reset values
        betValue = 0;
        playerCurrentChipValue = 0;
        markUnitValue = 0;
        potValue = 0;
        isBetMode = false;
        
        // Hide all buttons
        if (buttonFold != null) buttonFold.SetActive(false);
        if (buttonCheck != null) buttonCheck.SetActive(false);
        if (buttonCall != null) buttonCall.SetActive(false);
        if (buttonRaise != null) buttonRaise.SetActive(false);
        if (buttonAllIn != null) buttonAllIn.SetActive(false);
        
        // Hide slider
        if (slider != null && slider.transform.parent != null)
        {
            slider.transform.parent.gameObject.SetActive(false);
        }
        
        // Reset slider value
        if (slider != null)
        {
            slider.value = 0;
            slider.minValue = 0;
            slider.maxValue = 0;
        }
        
        // Reset text
        if (textBet != null) textBet.text = "";
        if (textButtonRaise != null) textButtonRaise.text = "";
        if (textButtonCall != null) textButtonCall.text = "";
        if (textMax != null) textMax.gameObject.SetActive(false);
        
        // Reset slider handle
        if (sliderHandle != null) sliderHandle.fillAmount = 0;
        
        Debug.Log("[HK Poker] ButtonBetContainer reset");
    }

    /// <summary>
    /// Update buttons based on AvailableActions from server
    /// </summary>
    public void UpdateButtonsFromAvailableActions(
        HKPlayerAvailableActions availableActions,
        int playerStack,
        int minRaise,
        int currentBet, int currentPot)
    {
        playerCurrentChipValue = playerStack;
        markUnitValue = minRaise;
        potValue = currentPot;
        // ===== STEP 1: Tắt tất cả buttons trước (clear previous state) =====
        if (buttonFold != null) buttonFold.SetActive(false);
        if (buttonCheck != null) buttonCheck.SetActive(false);
        if (buttonCall != null) buttonCall.SetActive(false);
        if (buttonRaise != null) buttonRaise.SetActive(false);
        if (buttonAllIn != null) buttonAllIn.SetActive(false);
        
        // Hide slider
        if (slider != null && slider.transform.parent != null)
        {
            slider.transform.parent.gameObject.SetActive(false);
        }
        
        // ===== STEP 2: Chỉ hiện các button được phép (dựa trên AvailableActions) =====
        
        // Update Fold button (always available)
        if (buttonFold != null)
        {
            buttonFold.SetActive(availableActions.CanFold);
        }
        
        // Update Check button
        if (buttonCheck != null)
        {
            buttonCheck.SetActive(availableActions.CanCheck);
        }
        
        // Update Call button
        if (buttonCall != null)
        {
            bool canCall = availableActions.CanCall && availableActions.CallVisible;
            buttonCall.SetActive(canCall);
            
            if (canCall && textButtonCall != null && availableActions.CallAmount > 0)
            {
                textButtonCall.text = $"Call {Utility.FormatMoney((int)availableActions.CallAmount)}";
            }
        }
        
        // Update Raise/Bet button
        // Nếu can_bet = true và can_raise = false → hiển thị "BET"
        // Nếu can_raise = true → hiển thị "RAISE"
        bool canBet = availableActions.CanBet;
        bool canRaise = availableActions.CanRaise && availableActions.RaiseVisible;
        
        if (buttonRaise != null)
        {
            bool showRaiseBetButton = canBet || canRaise;
            buttonRaise.SetActive(showRaiseBetButton);
            
            if (showRaiseBetButton && textButtonRaise != null)
            {
                // Determine if it's Bet or Raise mode
                if (canBet && !canRaise)
                {
                    // Bet mode - no bet yet, can bet
                    isBetMode = true;
                    textButtonRaise.text = "Bet";
                    // Set slider range for bet
                    if (slider != null)
                    {
                        slider.minValue = availableActions.MinBetAmount > 0 ? availableActions.MinBetAmount : minRaise;
                        slider.maxValue = availableActions.MaxBetAmount > 0 ? availableActions.MaxBetAmount : playerStack;
                    }
                    markUnitValue = availableActions.MinBetAmount > 0 ? availableActions.MinBetAmount : minRaise;
                }
                else if (canRaise)
                {
                    // Raise mode - there's a bet, can raise
                    isBetMode = false;
                    textButtonRaise.text = "Raise";
                    // Set slider range for raise
                    if (slider != null)
                    {
                        slider.minValue = availableActions.MinRaiseAmount > 0 ? availableActions.MinRaiseAmount : (currentBet + minRaise);
                        slider.maxValue = availableActions.MaxRaiseAmount > 0 ? availableActions.MaxRaiseAmount : playerStack;
                    }
                    markUnitValue = availableActions.MinRaiseAmount > 0 ? availableActions.MinRaiseAmount : (currentBet + minRaise);
                }
            }
        }
        
        // Update All-In button
        if (buttonAllIn != null)
        {
            buttonAllIn.SetActive(availableActions.CanAllIn);
        }
        
        // Reset slider to minimum value
        if (slider != null && playerCurrentChipValue > 0)
        {
            ResetSlider();
        }
    }
}
