using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailItem : MonoBehaviour
{
    public event EventHandler<OnCheckboxClickedEventArgs> OnCheckboxClicked;

    public class OnCheckboxClickedEventArgs : EventArgs
    {
        public bool isChecked;
    }
    [SerializeField] private Image checkboxImage;
    [SerializeField] private TextMeshProUGUI senderNameText, contentText, dateText, timeText;
    private bool isChecked = false;

    private void Start()
    {
        checkboxImage.gameObject.SetActive(isChecked);
    }

    public void SetData(string senderName, string content, string date, string time)
    {
        senderNameText.text = senderName;
        contentText.text = content;
        dateText.text = date;
        timeText.text = time;
    }

    public void OnClickCheckbox()
    {
        isChecked = !isChecked;
        checkboxImage.gameObject.SetActive(isChecked);
        OnCheckboxClicked?.Invoke(this, new OnCheckboxClickedEventArgs { isChecked = isChecked });
    }

    public void TickCheckbox()
    {
        isChecked = true;
        checkboxImage.gameObject.SetActive(isChecked);
    }

    public void UntickCheckbox()
    {
        isChecked = false;
        checkboxImage.gameObject.SetActive(isChecked);
    }
}
