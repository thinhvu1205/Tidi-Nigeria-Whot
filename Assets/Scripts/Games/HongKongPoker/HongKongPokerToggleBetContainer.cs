using System.Collections;
using System.Collections.Generic;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HongKongPokerToggleBetContainer : MonoBehaviour
{
    [SerializeField] GameObject toggleCheck, toggleCheckFold, toggleCallAny, toggleCall, toggleFold, toggleAllIn;
    
    [SerializeField] TextMeshProUGUI textToggleCall;
    
    [SerializeField] Toggle toggleCheckComponent, toggleCheckFoldComponent, toggleCallAnyComponent;
    
    private HKPlayerAvailableActions currentAvailableActions;
    private HongKongPokerView view; // Reference to view for sending actions
    
    public void SetViewReference(HongKongPokerView view)
    {
        this.view = view;
    }
    
    /// <summary>
    /// Update toggle buttons based on AvailableActions from server for OFF TURN players
    /// </summary>
    public void UpdateTogglesFromAvailableActions(HKPlayerAvailableActions availableActions)
    {
        currentAvailableActions = availableActions;
        
        // B. OFF TURN - No bet yet
        if (availableActions.CallAmount == 0)
        {
            // B.1 No bet: Check, Check/Fold, Call Any
            if (toggleCheck != null)
            {
                toggleCheck.SetActive(availableActions.CanCheck);
            }
            
            if (toggleCheckFold != null)
            {
                // Check/Fold is available if can check
                toggleCheckFold.SetActive(availableActions.CanCheck);
            }
            
            if (toggleCallAny != null)
            {
                toggleCallAny.SetActive(availableActions.CanCallAny);
            }
            
            // Hide other toggles
            if (toggleCall != null) toggleCall.SetActive(false);
            if (toggleFold != null) toggleFold.SetActive(false);
            if (toggleAllIn != null) toggleAllIn.SetActive(false);
        }
        else
        {
            // B.2 Bet exists
            // Hide Check toggles
            if (toggleCheck != null) toggleCheck.SetActive(false);
            if (toggleCheckFold != null) toggleCheckFold.SetActive(false);
            
            // Show Call Any
            if (toggleCallAny != null)
            {
                toggleCallAny.SetActive(availableActions.CanCallAny);
            }
            
            // Show Call X (if visible)
            if (toggleCall != null)
            {
                bool canCall = availableActions.CanCall && availableActions.CallVisible;
                toggleCall.SetActive(canCall);
                
                if (canCall && textToggleCall != null && availableActions.CallAmount > 0)
                {
                    textToggleCall.text = $"CALL {Utility.FormatMoney((int)availableActions.CallAmount)}";
                }
            }
            
            // Show Fold
            if (toggleFold != null)
            {
                toggleFold.SetActive(availableActions.CanFold);
            }
            
            // Show All-In (if not enough chips to call)
            if (toggleAllIn != null)
            {
                // All-In is available when not enough chips to call
                bool showAllIn = availableActions.CanAllIn && 
                                 availableActions.CanCall && 
                                 !availableActions.CallVisible;
                toggleAllIn.SetActive(showAllIn);
            }
        }
    }
    
    // Toggle button handlers
    public void OnToggleCheck(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("[HK Poker] OFF TURN - Toggle CHECK selected");
            // Uncheck other toggles
            if (toggleCheckFoldComponent != null) toggleCheckFoldComponent.isOn = false;
            if (toggleCallAnyComponent != null) toggleCallAnyComponent.isOn = false;
        }
    }
    
    public void OnToggleCheckFold(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("[HK Poker] OFF TURN - Toggle CHECK/FOLD selected");
            // Uncheck other toggles
            if (toggleCheckComponent != null) toggleCheckComponent.isOn = false;
            if (toggleCallAnyComponent != null) toggleCallAnyComponent.isOn = false;
        }
    }
    
    public void OnToggleCallAny(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("[HK Poker] OFF TURN - Toggle CALL ANY selected");
            // Uncheck other toggles
            if (toggleCheckComponent != null) toggleCheckComponent.isOn = false;
            if (toggleCheckFoldComponent != null) toggleCheckFoldComponent.isOn = false;
        }
    }
    
    public void OnToggleCall(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("[HK Poker] OFF TURN - Toggle CALL selected");
        }
    }
    
    public void OnToggleFold(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("[HK Poker] OFF TURN - Toggle FOLD selected");
        }
    }
    
    public void OnToggleAllIn(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("[HK Poker] OFF TURN - Toggle ALL-IN selected");
        }
    }
    
    /// <summary>
    /// Get selected action when turn comes
    /// Returns the action that was pre-selected via toggle
    /// </summary>
    public HKPokerAction? GetSelectedAction()
    {
        if (currentAvailableActions == null) return null;
        
        // Check Call Any first (highest priority) - auto call when turn comes
        if (toggleCallAnyComponent != null && toggleCallAnyComponent.isOn && 
            currentAvailableActions.CanCallAny)
        {
            // Call Any means auto call when turn comes
            if (currentAvailableActions.CanCall)
            {
                return HKPokerAction.HkActionCall;
            }
            // If can't call (not enough chips), fallback to all-in
            else if (currentAvailableActions.CanAllIn)
            {
                return HKPokerAction.HkActionAllIn;
            }
        }
        
        // Check Check/Fold
        if (toggleCheckFoldComponent != null && toggleCheckFoldComponent.isOn &&
            currentAvailableActions != null)
        {
            // Will check if possible, otherwise fold
            if (currentAvailableActions.CanCheck)
            {
                return HKPokerAction.HkActionCheck;
            }
            else
            {
                // Can't check, so fold
                return HKPokerAction.HkActionFold;
            }
        }
        
        // Check Check
        if (toggleCheckComponent != null && toggleCheckComponent.isOn &&
            currentAvailableActions != null && currentAvailableActions.CanCheck)
        {
            return HKPokerAction.HkActionCheck;
        }
        
        // Check Call
        if (toggleCall != null && toggleCall.activeSelf &&
            currentAvailableActions != null && currentAvailableActions.CanCall)
        {
            return HKPokerAction.HkActionCall;
        }
        
        // Check Fold
        if (toggleFold != null && toggleFold.activeSelf &&
            currentAvailableActions != null && currentAvailableActions.CanFold)
        {
            return HKPokerAction.HkActionFold;
        }
        
        // Check All-In
        if (toggleAllIn != null && toggleAllIn.activeSelf &&
            currentAvailableActions != null && currentAvailableActions.CanAllIn)
        {
            return HKPokerAction.HkActionAllIn;
        }
        
        return null; // No action selected
    }
    
    /// <summary>
    /// Clear all toggle selections
    /// </summary>
    public void ClearSelections()
    {
        if (toggleCheckComponent != null) toggleCheckComponent.isOn = false;
        if (toggleCheckFoldComponent != null) toggleCheckFoldComponent.isOn = false;
        if (toggleCallAnyComponent != null) toggleCallAnyComponent.isOn = false;
    }
    
    /// <summary>
    /// Reset all data when new game starts or round changes
    /// </summary>
    public void Reset()
    {
        // Clear available actions
        currentAvailableActions = null;
        
        // Hide all toggles
        if (toggleCheck != null) toggleCheck.SetActive(false);
        if (toggleCheckFold != null) toggleCheckFold.SetActive(false);
        if (toggleCallAny != null) toggleCallAny.SetActive(false);
        if (toggleCall != null) toggleCall.SetActive(false);
        if (toggleFold != null) toggleFold.SetActive(false);
        if (toggleAllIn != null) toggleAllIn.SetActive(false);
        
        // Clear selections
        ClearSelections();
        
        // Reset text
        if (textToggleCall != null) textToggleCall.text = "";
        
        Debug.Log("[HK Poker] ToggleBetContainer reset");
    }
}
