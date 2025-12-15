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
    
    // Quick bet amounts from server: [Base, 1/8, 1/4, 1/2, All-in]
    private List<long> quickBetAmounts = new List<long>();

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
    }

    private void OnValueChange()
    {
        if (slider == null) return;

        long rawValue = (long)slider.value;

        long minBet = (long)slider.minValue;
        long maxBet = (long)slider.maxValue;

        long step = GetStepUnit();
        if(step == 0) return;

        // SNAP theo đơn vị
        long snappedValue =
            ((rawValue - minBet) / step) * step + minBet;

        // Clamp an toàn
        snappedValue = (long) Mathf.Clamp(snappedValue, minBet, maxBet);

        // Update slider về đúng mốc (tránh loop)
        slider.SetValueWithoutNotify(snappedValue);

        betValue = snappedValue;

        if (textBet != null)
        {
            textBet.text = Utility.FormatMoney((int)betValue);
        }

        Debug.Log($"[HK Poker] Slider raw={rawValue}, snapped={snappedValue}, step={step}");
    }
    
    private long CalculateEValue()
    {
        if (quickBetAmounts == null || quickBetAmounts.Count < 5)
            return 0;

        long baseAmount  = quickBetAmounts[0]; // B + C + D
        long allInAmount = quickBetAmounts[4]; // A

        return (long)Mathf.Max(0, allInAmount - baseAmount);
    }

    private long GetStepUnit()
    {
        long eValue = CalculateEValue();

        if (eValue <= 0)
            return (long) markUnitValue;

        if (eValue < 16 * markUnitValue)
            return (long) Mathf.Max(1, eValue / 16);

        return (long) markUnitValue;
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
        
        if (slider != null && slider.transform.parent != null)
        {
            slider.transform.parent.gameObject.SetActive(true);
        }
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

    public void OnClick1In8()
    {
        if (view != null && quickBetAmounts != null && quickBetAmounts.Count >= 5)
        {
            // Use 1/8 amount from server (index 1)
            int betAmount = (int)quickBetAmounts[1];
        
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
        if (view != null && quickBetAmounts != null && quickBetAmounts.Count >= 5)
        {
            // Use 1/4 amount from server (index 2)
            int betAmount = (int)quickBetAmounts[2];

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

    public void OnClick1In2()
    {
        if (view != null && quickBetAmounts != null && quickBetAmounts.Count >= 5)
        {
            // Use 1/2 amount from server (index 3)
            int betAmount = (int)quickBetAmounts[3];
        
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
            betValue = slider.minValue;
        }

        if (sliderHandle != null)
        {
            sliderHandle.fillAmount = 0f;
        }

        // Set bet value to base amount if available, otherwise use markUnitValue
        if (quickBetAmounts != null && quickBetAmounts.Count >= 5)
        {
            betValue = quickBetAmounts[0]; // Base amount
            if (textBet != null)
            {
                textBet.text = Utility.FormatMoney((int)betValue);
            }
        }
        else if (textBet != null)
        {
            betValue = markUnitValue;
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
        quickBetAmounts.Clear();
        
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
        int bigBlind,
        int currentBet, int currentPot)
    {
        playerCurrentChipValue = playerStack;
        markUnitValue = bigBlind;
        potValue = currentPot;
        
        // Store quick bet amounts from server: [Base, 1/8, 1/4, 1/2, All-in]
        quickBetAmounts.Clear();
        if (availableActions.QuickBetAmounts != null && availableActions.QuickBetAmounts.Count >= 5)
        {
            quickBetAmounts.AddRange(availableActions.QuickBetAmounts);
            Debug.Log($"[HK Poker] Quick bet amounts: Base={quickBetAmounts[0]}, 1/8={quickBetAmounts[1]}, 1/4={quickBetAmounts[2]}, 1/2={quickBetAmounts[3]}, All-in={quickBetAmounts[4]}");
        }
        
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
                    
                    // Set slider range from quick bet amounts if available
                    if (slider != null && quickBetAmounts != null && quickBetAmounts.Count >= 5)
                    {
                        slider.minValue = quickBetAmounts[0]; // Base
                        slider.maxValue = quickBetAmounts[4];  // All-in
                    }
                    else
                    {
                        // Fallback to old values
                        slider.minValue = availableActions.MinBetAmount > 0 ? availableActions.MinBetAmount : bigBlind;
                        slider.maxValue = availableActions.MaxBetAmount > 0 ? availableActions.MaxBetAmount : playerStack;
                    }
                    // markUnitValue = availableActions.MinBetAmount > 0 ? availableActions.MinBetAmount : bigBlind;
                }
                else if (canRaise)
                {
                    // Raise mode - there's a bet, can raise
                    isBetMode = false;
                    textButtonRaise.text = "Raise";
                    
                    // Set slider range from quick bet amounts if available
                    if (slider != null && quickBetAmounts != null && quickBetAmounts.Count >= 5)
                    {
                        slider.minValue = quickBetAmounts[0]; // Base
                        slider.maxValue = quickBetAmounts[4];  // All-in
                    }
                    else
                    {
                        // Fallback to old values
                        slider.minValue = availableActions.MinRaiseAmount > 0 ? availableActions.MinRaiseAmount : (currentBet + bigBlind);
                        slider.maxValue = availableActions.MaxRaiseAmount > 0 ? availableActions.MaxRaiseAmount : playerStack;
                    }
                    // markUnitValue = availableActions.MinRaiseAmount > 0 ? availableActions.MinRaiseAmount : (currentBet + bigBlind);
                }
            }
        }
        
        // Update All-In button
        if (buttonAllIn != null)
        {
            buttonAllIn.SetActive(availableActions.CanAllIn);
        }
        
        // Update MAX text if available
        if (textMax != null && quickBetAmounts != null && quickBetAmounts.Count >= 5)
        {
            textMax.text = Utility.FormatMoney((int)quickBetAmounts[4]); // All-in amount
            textMax.gameObject.SetActive(true);
        }
        
        // Reset slider to minimum value (Base)
        if (slider != null && playerCurrentChipValue > 0)
        {
            ResetSlider();
        }
    }
}

