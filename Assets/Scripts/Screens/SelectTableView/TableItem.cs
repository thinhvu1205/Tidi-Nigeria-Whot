using System.Collections;
using System.Collections.Generic;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI markUnitText, tableNameText, tableIDText;
    [SerializeField] List<Image> playerSlotImageList;
    [SerializeField] List<Sprite> slotIconList;
    [SerializeField] Button joinButton;
    [SerializeField] GameObject fullObject;

    public void SetData(int countPlaying, int maxSize, float markUnit, string roomName, string roomId, bool isOpen)
    {
        for (var i = 0; i < playerSlotImageList.Count; i++)
        {
            playerSlotImageList[i].sprite = i <= countPlaying - 1 ? slotIconList[1] : slotIconList[0];
            playerSlotImageList[i].gameObject.SetActive(!(i >= maxSize));
            playerSlotImageList[i].SetNativeSize();
        }

        // fullObject.SetActive(!isOpen);
        joinButton.gameObject.SetActive(isOpen);
        markUnitText.text = Utility.FormatMoney((int)markUnit, true);
        tableNameText.text = roomName;
        tableIDText.text = roomId; 
    }
}
