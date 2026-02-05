using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackView : BaseView
{
    [SerializeField] TextMeshProUGUI feedbackText;
    [SerializeField] Button feedbackButton;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        feedbackButton.onClick.RemoveAllListeners();
        feedbackButton.onClick.AddListener(OnClickSendFeedback);
    }

    private async void OnClickSendFeedback()
    {
        try
        {
            string feedbackStr = Regex.Replace(feedbackText.text, @"\p{C}+", "").Trim();
            if (!string.IsNullOrEmpty(feedbackStr))
            {
                UIManager.Instance.ShowProgressing();
                SubmitFeedbackResponse feedbackResponse = await DataSender.SubmitFeedBack(feedbackStr);
                UIManager.Instance.HideProgressing();
                Hide();
                if (feedbackResponse != null)
                { 
                    UIManager.Instance.ShowAlertDialog(feedbackResponse.Message);
                }
            }
            else
            {
                UIManager.Instance.ShowAlertDialog("Please enter your feedback before submitting.");
            }
        }
        catch (Exception e)
        {
        }
        
    }
}
