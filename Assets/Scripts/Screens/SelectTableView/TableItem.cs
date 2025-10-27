using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Newtonsoft.Json.Linq;
using Proto;
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

    public void SetData(Match match)
    {
        for (var i = 0; i < playerSlotImageList.Count; i++)
        {
            playerSlotImageList[i].sprite = i <= match.Size - 1 ? slotIconList[1] : slotIconList[0];
            playerSlotImageList[i].gameObject.SetActive(!(i >= match.MaxSize));
            playerSlotImageList[i].SetNativeSize();
        }

        // fullObject.SetActive(!isOpen);
        joinButton.gameObject.SetActive(match.Open);
        joinButton.gameObject.SetActive(true);
        markUnitText.text = Utility.FormatMoney((int)match.MarkUnit, true);
        string listName = "";
        foreach (SimpleProfile simpleProfile in match.Profiles)
        {
            name = simpleProfile.UserName;
            if (name.Length > 10)
            {
                name = name.Substring(0, 7) + "...";
            }
            if (simpleProfile != match.Profiles.ToList().Last())
            {
                name += ","; 
            }
            listName += name;
        }
        tableNameText.text = listName;
        tableIDText.text = match.TableId;
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => _ = OnClickButtonJoin(match.MatchId, match.Open));
    }

    private async UniTask OnClickButtonJoin(string matchId, bool isOpen, string password = "")
    {
        if (isOpen)
        {
            await JoinMatch(matchId);
        }
        else
        {
            UIManager.Instance.OpenEnterPasswordView(out EnterPasswordView enterPasswordView);
            enterPasswordView.SetPassword(password);
            enterPasswordView.SetOnClickListener(() => JoinMatch(matchId));
        }
    }

    private async UniTask JoinMatch(string matchId)
    {
        Debug.Log("TRY JOIN MATCH: " + matchId);
        var labelMatch = await DataSender.JoinMatch(matchId);
        if (labelMatch != null)
        {
            Config.currentGameId = labelMatch.Name;
            UIManager.Instance.HandleOpenGame(labelMatch);
        }
    }
}
