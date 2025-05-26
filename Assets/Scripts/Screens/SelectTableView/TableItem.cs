using System.Collections;
using System.Collections.Generic;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI betAmountText, userNameText, roomIDText;
    [SerializeField] List<Image> playerSlotImageList;
    [SerializeField] List<Sprite> slotIconList;
    [SerializeField] Button joinButton;
    [SerializeField] GameObject fullObject;

    public void SetData(JObject dataItem, int index)
    {
        Debug.Log("SetData: " + dataItem.ToString());
        int sizeTable = (int)dataItem["size"];
        for (var i = 0; i < playerSlotImageList.Count; i++)
        {
            playerSlotImageList[i].sprite = i <= (int)dataItem["player"] - 1 ? slotIconList[1] : slotIconList[0];
            playerSlotImageList[i].gameObject.SetActive(!(i >= sizeTable));
            playerSlotImageList[i].SetNativeSize();
        }

        // fullObject.SetActive((int)dataItem["player"] == (int)dataItem["size"]);
        // joinButton.gameObject.SetActive(!fullObject.activeSelf);
        betAmountText.text = Utility.FormatMoney((long)dataItem["mark"], true);
        //txtName.text = (string)dataItem["N"];
        JArray nameArray = (JArray)dataItem["ArrName"];
        List<string> arrName = nameArray.ToObject<List<string>>();
        string tableName = "";
        foreach (string name in arrName)
        {
            string tbName = name;
            if (name.Length > 10)
            {
                tbName = name[..7] + "..., ";
            }
            tableName += tbName;
        }
        userNameText.text = tableName;
        roomIDText.text = (int)dataItem["id"] + "";
    }
}
