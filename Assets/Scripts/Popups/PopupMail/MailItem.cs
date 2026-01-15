using System;
using System.Collections;
using System.Collections.Generic;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailItem : MonoBehaviour
{
    public event EventHandler<OnCheckboxClickedEventArgs> OnCheckboxClicked;

    public class OnCheckboxClickedEventArgs : EventArgs
    {
        public bool isChecked;
        public Notification notification;
    }
    [SerializeField] private Image checkboxImage;
    [SerializeField] private TextMeshProUGUI textTitle, textContent, textDate, textTime;
    private bool isChecked = false;
    private Notification notification;

    private void Start()
    {
        checkboxImage.gameObject.SetActive(isChecked);
    }

    public void SetData(Notification notification)
    {
        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(notification.CreateTimeUnix).LocalDateTime;
        this.notification = notification;
        string datePart = dateTime.ToString("dd/MM");
        string timePart = dateTime.ToString("HH:mm");
        textTitle.text = notification.Title;
        textContent.text = notification.Content;
        textDate.text = datePart;
        textTime.text = timePart;
    }

    public void OnClickDetail()
    {
        UIManager.Instance.OpenMailDetail(notification);
    }

    public void OnClickCheckbox()
    {
        isChecked = !isChecked;
        checkboxImage.gameObject.SetActive(isChecked);
        OnCheckboxClicked?.Invoke(this, new OnCheckboxClickedEventArgs { isChecked = isChecked, notification = notification });
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

    public long GetNotificationId() => this.notification.Id;
}
