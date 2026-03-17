using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using Yuujins.Match.V1;
using UnityEngine;
using UnityEngine.UI;
using MatchInfo = Yuujins.Match.V1.MatchInfo;

public class TableItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI markUnitText, tableNameText, tableIDText;
    [SerializeField] List<Image> playerSlotImageList;
    [SerializeField] List<Sprite> slotIconList;
    [SerializeField] Button joinButton;
    [SerializeField] GameObject fullObject, imageDoubleDeck, imageLock;

    public void SetData(MatchInfo match)
    {
        for (var i = 0; i < playerSlotImageList.Count; i++)
        {
            playerSlotImageList[i].sprite = slotIconList[0];
            playerSlotImageList[i].gameObject.SetActive(true);
            playerSlotImageList[i].SetNativeSize();
        }
        joinButton.gameObject.SetActive(true);
        markUnitText.text = match.MatchId;
        tableNameText.text = "";
        tableIDText.text = match.MatchId;
        imageLock.SetActive(false);
        imageDoubleDeck.SetActive(false);
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => _ = OnClickButtonJoin(match.MatchId, true));
    }

    private async UniTask OnClickButtonJoin(string matchId, bool isOpen)
    {
        if (isOpen)
        {
            await JoinMatch(matchId);
        }
        else
        {
            UIManager.Instance.OpenEnterPasswordView(out EnterPasswordView enterPasswordView);
            enterPasswordView.SetOnClickListener(password => JoinMatch(matchId, password));
        }
    }

    private async UniTask JoinMatch(string matchId, string passWord = "")
    {
        var labelMatch = await DataSender.JoinMatch(matchId, passWord);
        if (labelMatch != null)
        {
            Config.currentGameName = labelMatch.Name ?? labelMatch.MatchId;
            UIManager.Instance.HandleOpenGame(labelMatch);
        }
    }
}
