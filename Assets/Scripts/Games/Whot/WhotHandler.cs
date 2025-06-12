using Api;
using Nakama;
using UnityEngine;

public class WhotHandler : IGameHandler
{
    private readonly WhotView whotView;

    public WhotHandler(WhotView whotView)
    {
        this.whotView = whotView;
    }
    public void OnMatchFound(IMatchmakerMatched matchmakerMatched)
    {

    }

    public void OnMatchJoin(IMatch match)
    {
        whotView.HandleJoinMatch(match);
        // parse match.Label hoặc chờ MatchState 
    }

    public void OnMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        // xử lý khi người chơi vào/ra
        whotView.HandleMatchPresence(presenceEvent);
    }

    public void OnMatchLeave()
    {
        // cleanup
        whotView.HandleMatchLeave();
    }
    
    public void OnUpdateTable(IMatchState matchState)
    {
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        whotView.HandleUpdateTable(updateTable);
        Debug.Log("OpCodeUpdate.Table " + updateTable.ToString());
    }

    public void OnUpdateTurn(IMatchState matchState)
    {
        var updateTurn = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log("OpCodeUpdate.Turn " + updateTurn.ToString());
        whotView.HandleUpdateTurn(updateTurn);
    }

    public void OnUpdateDeal(IMatchState matchState)
    {
        var deal = UpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("OpCodeUpdate.Deal " + deal.ToString());
        whotView.HandleUpdateDeal(deal);
    }

    public void OnUpdateCardState(IMatchState matchState)
    {
        var updateCardState = UpdateCardState.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnUpdateCardState: " + updateCardState.ToString());
        whotView.HandleUpdateCardState(updateCardState);
    }

    public void OnUpdateGameState(IMatchState matchState)
    {
        var updateGameState = UpdateGameState.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnUpdateGameState: " + updateGameState.ToString());
        whotView.HandleUpdateGameState(updateGameState);

    }

    public void OnFinish(IMatchState matchState)
    {
        var updateFinish = UpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnFinish: " + updateFinish.ToString());
    }
}

