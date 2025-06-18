using Api;
using Nakama;
using UnityEngine;

public class WhotHandler : IGameHandler
{
    private readonly WhotView whotGame;

    public WhotHandler(WhotView game)
    {
        whotGame = game;
    }
    public void OnMatchFound(IMatchmakerMatched matchmakerMatched)
    {

    }

    public void OnMatchJoin(IMatch match)
    {
        whotGame.HandleJoinMatch(match);
                Debug.Log("HandleJoinMatch called with match ID: " + match.ToString());

        // parse match.Label hoặc chờ MatchState 
    }

    public void OnMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        // xử lý khi người chơi vào/ra
        // whotGame.HandleMatchPresence(presenceEvent);
    }

    public void OnMatchLeave()
    {
        // cleanup
        // whotGame.HandleMatchLeave();
    }
    
    public void OnUpdateTable(IMatchState matchState)
    {
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        whotGame.HandleUpdateTable(updateTable);
        Debug.Log("OpCodeUpdate.Table " + updateTable.ToString());
    }

    public void OnUpdateTurn(IMatchState matchState)
    {
        var updateTurn = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log("OpCodeUpdate.Turn " + updateTurn.ToString());
        whotGame.HandleUpdateTurn(updateTurn);
    }

    public void OnUpdateDeal(IMatchState matchState)
    {
        var deal = UpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("OpCodeUpdate.Deal " + deal.ToString());
        whotGame.HandleUpdateDeal(deal);
    }

    public void OnUpdateCardState(IMatchState matchState)
    {
        var updateCardState = UpdateCardState.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnUpdateCardState: " + updateCardState.ToString());
        whotGame.HandleUpdateCardState(updateCardState);
    }

    public void OnUpdateGameState(IMatchState matchState)
    {
        var updateGameState = UpdateGameState.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnUpdateGameState: " + updateGameState.ToString());
        whotGame.HandleUpdateGameState(updateGameState);

    }

    public void OnFinish(IMatchState matchState)
    {
        var updateFinish = UpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("WHOT OnFinish: " + updateFinish.ToString());
    }
}

