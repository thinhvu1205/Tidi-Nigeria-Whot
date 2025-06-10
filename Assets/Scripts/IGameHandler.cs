using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public interface IGameHandler
{
    void OnMatchFound(IMatchmakerMatched matchmakerMatched);
    void OnMatchJoin(IMatch match);
    void OnMatchPresence(IMatchPresenceEvent presenceEvent);
    void OnMatchLeave();
    void OnUpdateTable(IMatchState matchState);
    void OnUpdateDeal(IMatchState matchState);
    void OnUpdateTurn(IMatchState matchState);
    void OnUpdateCardState(IMatchState matchState);
    void OnUpdateGameState(IMatchState matchState);
    void OnFinish(IMatchState matchState);
}

