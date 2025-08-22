using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackButtonAction : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image checkbox;
    [SerializeField] private Image checkboxImage;
    private bool isChecked;

    private void Awake()
    {
        Reset();
    }

    public void ShowCheckBox()
    {
        checkbox.gameObject.SetActive(true);
        checkboxImage.gameObject.SetActive(true);
    }

    public void HideCheckBox()
    {
        checkbox.gameObject.SetActive(false);
        checkboxImage.gameObject.SetActive(false);
    }

    public void OnClickCheckBox()
    {
        checkboxImage.gameObject.SetActive(!isChecked);
        isChecked = !isChecked;
    }

    public void Reset()
    {
        isChecked = false;
        checkboxImage.gameObject.SetActive(false);
    }
}
