using System;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

public class ExchangeHistoryItem : MonoBehaviour
{
    public event Action<string> OnClickCancel;

    [SerializeField] private TextMeshProUGUI textDay, textChip, textCurrency, textId, textStatus;
    [SerializeField] private GameObject buttonCancel;
    private string id;

    public void SetInfo(ExchangeInfo exchangeInfo)
    {
        id = exchangeInfo.Id;
        textDay.text = Utility.ConvertUnixTimeToHHMMDDMMYYYY(exchangeInfo.CreateTime);
        textChip.text = exchangeInfo.Chips.ToString();
        textCurrency.text = exchangeInfo.Price.ToString();
        textId.text = exchangeInfo.Id.ToString();
        switch (exchangeInfo.Status)
        {
            case 1: // Waiting
                textStatus.gameObject.SetActive(false);
                // textStatus.text = "Waiting";
                buttonCancel.SetActive(true);
                break;
            case 2: // Canceled by user
                textStatus.gameObject.SetActive(true);
                textStatus.text = "Canceled";
                textStatus.color = Color.red;
                buttonCancel.SetActive(false);
                break;
            case 3: // Pending
                textStatus.gameObject.SetActive(true);
                textStatus.text = "Pending";
                buttonCancel.SetActive(false);
                break;
            case 4: // Done
                textStatus.gameObject.SetActive(true);
                textStatus.text = "Done";
                textStatus.color = Color.green;
                buttonCancel.SetActive(false);
                break;
            case 5: // Rejected
                textStatus.gameObject.SetActive(true);
                textStatus.text = "Rejected";
                textStatus.color = Color.red;
                buttonCancel.SetActive(false);
                break;
        }
    }

    public void OnClickCancelButton()
    {
        OnClickCancel?.Invoke(id);
    }
}
