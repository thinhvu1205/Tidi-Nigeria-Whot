using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using TMPro;
using UnityEngine;

public class SlotTarzanMiniGameView : BaseView
{
    [SerializeField] TextMeshProUGUI pickLeftText, totalWinText;
    [SerializeField] Transform itemContainer;
    [SerializeField] private SlotTarzanMinigameItem[] itemList;
    private readonly SiXiangSymbol[] tarzanSymbolList = new SiXiangSymbol[] {
        SiXiangSymbol.TarzanMoreTurnx2,
        SiXiangSymbol.TarzanMoreTurnx3,
        SiXiangSymbol.TarzanRandom1,
        SiXiangSymbol.TarzanRandom2,
        SiXiangSymbol.TarzanRandom3,
        SiXiangSymbol.TarzanRandom4,
        SiXiangSymbol.TarzanRandom5
    };

    private SlotTarzanView tarzanView;
    private SlotTarzanMinigameItem currentItemPick;
    public int PickLeft { get; private set; } = 5;
    private int totalWin = 0;
    public bool CanClick { get; set; } = true;

    protected override void Start()
    {
        base.Start();
        for (int i = 0; i < itemList.Length; i++)
        {
            SlotTarzanMinigameItem item = itemList[i];
            item.Index = i;
        }
        tarzanView = (SlotTarzanView)UIManager.Instance.gameView;
    }

    private void OnDisable()
    {
        ResetUI();
    }

    public void OnClickItem(SlotTarzanMinigameItem item)
    {
        if (PickLeft > 0 && CanClick)
        {
            Debug.Log("CLICK ITEM");
            if (!item.IsOpen)
            {
                PickLeft--;
                pickLeftText.text = PickLeft.ToString();
                InfoBet infoBet = new()
                {
                    Chips = tarzanView.CurrentBetLevel,
                    Id = item.Index,
                };
                DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
            }
        }

    }
    public void SetInfo(SlotDesk data, bool isPicking = false)
    {
        List<SpinSymbol> spinList = data.Matrix.SpinLists.ToList();
        PickLeft = (int)data.NumSpinLeft;
        // if (PickLeft <= 0)
        // {
        //     PickLeft = 5;
        // }
        if (!isPicking)
        {
            pickLeftText.text = PickLeft.ToString();
        }
        UpdateTotalWin((int)data.GameReward.TotalChipsWinByGame); 
        // List<JObject> listData = new List<JObject>();
        // JArray views = (JArray)dataBonus["view"];
        // foreach (JArray dataView in views)
        // {
        //     foreach (JObject data in dataView)
        //     {
        //         listData.Add(data);
        //     }
        // }
        for (int i = 0; i < spinList.Count; i++)
        {
            SpinSymbol spinSymbol = spinList[i];
            SlotTarzanMinigameItem item = itemList[i];
            if (!tarzanSymbolList.Contains(spinSymbol.Symbol))
            {
                item.Reset();
            }
            else
            {
                if (!item.IsOpen)
                {
                    Debug.Log("OPEN ITEM INDEX: " + i);
                    item.ShowResult(spinSymbol.Symbol, spinSymbol.WinAmount);
                }

            }
        }
    }

    public void UpdateTotalWin(int value)
    {
        Utility.TweenNumberFromK(totalWinText, value, totalWin, 0.3f, true);
        totalWin = value;
    }
    public void ShowPopupResult()
    {
        // gameView.ShowPopupRes(totalWin);
    }
    public void ResetUI()
    {
        totalWin = 0;
        PickLeft = 5;
        pickLeftText.text = "5";
        totalWinText.text = "0";
        foreach (SlotTarzanMinigameItem item in itemList)
        {
            item.Reset();
        }
        // itemList.ForEach(item =>
        // {
        //     item.Reset();
        // });
        // onClickClose(false);
    }
}
