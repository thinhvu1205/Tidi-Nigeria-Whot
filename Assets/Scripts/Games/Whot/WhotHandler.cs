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
        whotView.HandleMatchFound(matchmakerMatched);

    }

    public void OnMatchJoin(IMatch match)
    {
        whotView.HandleJoinMatch(match);
        // parse match.Label hoặc chờ MatchState 
    }

    public void OnMatchState(IMatchState state)
    {
        whotView.HandleMatchState(state);
        Debug.Log("WHOT Match State: " + state.OpCode);
        // parse JSON và xử lý tùy theo OpCode
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
        Debug.Log("OpCodeUpdate.Table " + updateTable.ToString());
    }

    public void OnUpdateTurn(IMatchState matchState)
    {
        var updateTurn = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log("OpCodeUpdate.Turn " + updateTurn.ToString());
    }

    public void OnUpdateDeal(IMatchState matchState)
    {
        var deal = UpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("OpCodeUpdate.Deal " + deal.ToString());
    }
    
    public void OnUpdateCardState(IMatchState matchState) {
        var updateCardState = UpdateCardState.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnUpdateCardState: " + updateCardState.ToString());
    }

    public void OnUpdateGameState(IMatchState matchState)
    {
        var updateGameState = UpdateGameState.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnUpdateGameState: " + updateGameState.ToString());
    }

    public void OnFinish(IMatchState matchState)
    {
        var updateFinish = UpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnFinish: " + updateFinish.ToString());
    }
}

