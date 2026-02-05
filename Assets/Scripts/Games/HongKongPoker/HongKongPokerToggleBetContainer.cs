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
        
        // ✅ Enable toggles when updating (user can select action before turn)
        SetTogglesInteractable(true);
    }
    
    /// <summary>
    /// Enable/disable toggle interactability
    /// </summary>
    private void SetTogglesInteractable(bool interactable)
    {
        if (toggleCheckComponent != null) toggleCheckComponent.interactable = interactable;
        if (toggleCheckFoldComponent != null) toggleCheckFoldComponent.interactable = interactable;
        if (toggleCallAnyComponent != null) toggleCallAnyComponent.interactable = interactable;
        
        // Get Toggle components for other toggles
        if (toggleCall != null)
        {
            var toggleComponent = toggleCall.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.interactable = interactable;
        }
        if (toggleFold != null)
        {
            var toggleComponent = toggleFold.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.interactable = interactable;
        }
        if (toggleAllIn != null)
        {
            var toggleComponent = toggleAllIn.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.interactable = interactable;
        }
    }
    
    // Toggle button handlers
    public void OnToggleCheck(bool isOn)
    {
        if (isOn)
        {
            // Uncheck other toggles
            if (toggleCheckFoldComponent != null) toggleCheckFoldComponent.isOn = false;
            if (toggleCallAnyComponent != null) toggleCallAnyComponent.isOn = false;
            UncheckOtherToggles(toggleCheckComponent);
        }
    }
    
    public void OnToggleCheckFold(bool isOn)
    {
        if (isOn)
        {
            // Uncheck other toggles
            if (toggleCheckComponent != null) toggleCheckComponent.isOn = false;
            if (toggleCallAnyComponent != null) toggleCallAnyComponent.isOn = false;
            UncheckOtherToggles(toggleCheckFoldComponent);
        }
    }
    
    public void OnToggleCallAny(bool isOn)
    {
        if (isOn)
        {
            // Uncheck other toggles
            if (toggleCheckComponent != null) toggleCheckComponent.isOn = false;
            if (toggleCheckFoldComponent != null) toggleCheckFoldComponent.isOn = false;
            UncheckOtherToggles(toggleCallAnyComponent);
        }
    }
    
    public void OnToggleCall(bool isOn)
    {
        if (isOn)
        {
            UncheckOtherToggles(null, toggleCall);
        }
    }
    
    public void OnToggleFold(bool isOn)
    {
        if (isOn)
        {
            UncheckOtherToggles(null, null, toggleFold);
        }
    }
    
    public void OnToggleAllIn(bool isOn)
    {
        if (isOn)
        {
            UncheckOtherToggles(null, null, null, toggleAllIn);
        }
    }
    
    /// <summary>
    /// Uncheck all toggles except specified ones
    /// </summary>
    private void UncheckOtherToggles(Toggle exceptCheck = null, GameObject exceptCall = null, GameObject exceptFold = null, GameObject exceptAllIn = null)
    {
        if (toggleCheckComponent != null && toggleCheckComponent != exceptCheck) toggleCheckComponent.isOn = false;
        if (toggleCheckFoldComponent != null && toggleCheckFoldComponent != exceptCheck) toggleCheckFoldComponent.isOn = false;
        if (toggleCallAnyComponent != null && toggleCallAnyComponent != exceptCheck) toggleCallAnyComponent.isOn = false;
        
        if (toggleCall != null && toggleCall != exceptCall)
        {
            var toggleComponent = toggleCall.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.isOn = false;
        }
        if (toggleFold != null && toggleFold != exceptFold)
        {
            var toggleComponent = toggleFold.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.isOn = false;
        }
        if (toggleAllIn != null && toggleAllIn != exceptAllIn)
        {
            var toggleComponent = toggleAllIn.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.isOn = false;
        }
    }
    
    /// <summary>
    /// Get selected action when turn comes
    /// Returns the action that was pre-selected via toggle (based on OLD available actions)
    /// </summary>
    public HKPokerAction? GetSelectedAction()
    {
        // Check Call Any first (highest priority) - auto call when turn comes
        if (toggleCallAnyComponent != null && toggleCallAnyComponent.isOn)
        {
            return HKPokerAction.HkActionCall; // Will be validated later
        }
        
        // Check Check/Fold
        if (toggleCheckFoldComponent != null && toggleCheckFoldComponent.isOn)
        {
            return HKPokerAction.HkActionCheck; // Will check if possible, otherwise fold (validated later)
        }
        
        // Check Check
        if (toggleCheckComponent != null && toggleCheckComponent.isOn)
        {
            return HKPokerAction.HkActionCheck;
        }
        
        // Check Call
        if (toggleCall != null && toggleCall.activeSelf)
        {
            var toggleComponent = toggleCall.GetComponent<Toggle>();
            if (toggleComponent != null && toggleComponent.isOn)
            {
                return HKPokerAction.HkActionCall;
            }
        }
        
        // Check Fold
        if (toggleFold != null && toggleFold.activeSelf)
        {
            var toggleComponent = toggleFold.GetComponent<Toggle>();
            if (toggleComponent != null && toggleComponent.isOn)
            {
                return HKPokerAction.HkActionFold;
            }
        }
        
        // Check All-In
        if (toggleAllIn != null && toggleAllIn.activeSelf)
        {
            var toggleComponent = toggleAllIn.GetComponent<Toggle>();
            if (toggleComponent != null && toggleComponent.isOn)
            {
                return HKPokerAction.HkActionAllIn;
            }
        }
        
        return null; // No action selected
    }
    
    /// <summary>
    /// Validate selected toggle action with current AvailableActions from server
    /// Returns validated action if still valid, null otherwise
    /// </summary>
    public HKPokerAction? ValidateSelectedAction(HKPlayerAvailableActions currentActions)
    {
        if (currentActions == null) return null;
        
        var selectedAction = GetSelectedAction();
        if (!selectedAction.HasValue) return null;
        
        var action = selectedAction.Value;
        
        // Validate action with current AvailableActions
        switch (action)
        {
            case HKPokerAction.HkActionCheck:
                // Check if Check is still valid
                if (currentActions.CanCheck)
                {
                    return HKPokerAction.HkActionCheck;
                }
                // If Check/Fold toggle was selected but can't check, fallback to fold
                if (toggleCheckFoldComponent != null && toggleCheckFoldComponent.isOn && 
                    currentActions.CanFold)
                {
                    return HKPokerAction.HkActionFold;
                }
                return null; // Can't check or fold
                
            case HKPokerAction.HkActionCall:
                // Check if Call is still valid
                if (currentActions.CanCall)
                {
                    return HKPokerAction.HkActionCall;
                }
                // If Call Any toggle was selected but can't call, fallback to all-in
                if (toggleCallAnyComponent != null && toggleCallAnyComponent.isOn && 
                    currentActions.CanAllIn)
                {
                    return HKPokerAction.HkActionAllIn;
                }
                return null; // Can't call or all-in
                
            case HKPokerAction.HkActionFold:
                if (currentActions.CanFold)
                {
                    return HKPokerAction.HkActionFold;
                }
                return null; // Can't fold
                
            case HKPokerAction.HkActionAllIn:
                if (currentActions.CanAllIn)
                {
                    return HKPokerAction.HkActionAllIn;
                }
                return null; // Can't all-in
                
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Clear all toggle selections
    /// </summary>
    public void ClearSelections()
    {
        if (toggleCheckComponent != null) toggleCheckComponent.isOn = false;
        if (toggleCheckFoldComponent != null) toggleCheckFoldComponent.isOn = false;
        if (toggleCallAnyComponent != null) toggleCallAnyComponent.isOn = false;
        
        if (toggleCall != null)
        {
            var toggleComponent = toggleCall.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.isOn = false;
        }
        if (toggleFold != null)
        {
            var toggleComponent = toggleFold.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.isOn = false;
        }
        if (toggleAllIn != null)
        {
            var toggleComponent = toggleAllIn.GetComponent<Toggle>();
            if (toggleComponent != null) toggleComponent.isOn = false;
        }
    }
    
    /// <summary>
    /// Disable toggles when it's my turn (prevent further changes)
    /// </summary>
    public void DisableToggles()
    {
        SetTogglesInteractable(false);
    }
    
    /// <summary>
    /// Reset all data when new game starts or round changes
    /// ✅ Toggle selections are only valid for ONE round, reset when new round starts
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
        
        // ✅ Clear selections (reset for new round)
        ClearSelections();
        
        // Reset text
        if (textToggleCall != null) textToggleCall.text = "";
        
    }
}

