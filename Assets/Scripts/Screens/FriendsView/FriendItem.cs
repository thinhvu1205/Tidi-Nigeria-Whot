using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendItem : MonoBehaviour
{
    public event EventHandler<OnCheckboxClickedEventArgs> OnCheckboxClicked;

    public class OnCheckboxClickedEventArgs : EventArgs
    {
        public bool isChecked;
    }
    [SerializeField] private Image checkboxImage, avatarImage;
    [SerializeField] private TextMeshProUGUI nameText;
    private bool isChecked = false;

    private void Start()
    {
        checkboxImage.gameObject.SetActive(isChecked);
    }

    public void SetData(string name, string avatar)
    {

        nameText.text = name;
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
