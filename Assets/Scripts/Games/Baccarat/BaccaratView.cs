using System.Collections;
using System.Collections.Generic;
using Proto;
using Nakama;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaccaratView : BaseGameView
{
    [SerializeField] private GameObject chipPrefab, playerPrefab, popupHistoryPrefab;
    [SerializeField]
    private TextMeshProUGUI textCountDown, textTime, textWaiting, textMaxbet, textNotEnoughGold,
    textBankerValue, textPlayerValue, textTieValue, textPlayerScore, textBankerScore;
    [SerializeField] private SkeletonGraphic animationVictory;
    [SerializeField] private Button buttonRebet, buttonDouble, buttonEmoji, buttonMenu;
    [SerializeField] private Button[] listButtonBet;
    [SerializeField] private TextMeshProUGUI[] listBetValue, listBetPair;
    [SerializeField] private GameObject[] listBoxBet, listCardBanker, listCardPlayer;
    [SerializeField] private Transform[] listPlayerPosition, listInvitePosition;
    private UnityEngine.Pool.ObjectPool<GameObject> chipPool, cardPool;

    public override void HandleMatchLeave()
    {
        base.HandleMatchLeave();
        Debug.Log("HandleMatchLeave");
    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        var data  = BaccaratUpdateDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateTable " + data);
    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var data = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateUserInTable " + data);
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
        var data = BaccaratUpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateDeal " + data);
    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
    }

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        base.HandleUpdateKickOffTheTable(matchState);
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
    }
}
