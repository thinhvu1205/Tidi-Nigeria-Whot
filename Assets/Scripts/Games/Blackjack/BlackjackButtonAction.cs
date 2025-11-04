using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackButtonAction : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image checkbox;
    [SerializeField] private Image checkboxImage;
    public Button button;
    public bool isChecked;

    private void Awake()
    {
        Reset();
    }

    public void ShowCheckBox()
    {
        checkbox.gameObject.SetActive(true);
        checkboxImage.gameObject.SetActive(isChecked);
        text.transform.localPosition = new Vector3(20f, text.transform.localPosition.y, text.transform.localPosition.z);
    }

    public void HideCheckBox()
    {
        checkbox.gameObject.SetActive(false);
        checkboxImage.gameObject.SetActive(false);
        text.transform.localPosition = new Vector3(0f, text.transform.localPosition.y, text.transform.localPosition.z);
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
