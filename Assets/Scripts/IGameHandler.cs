using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public interface IGameHandler
{
    void OnMatchFound(IMatchmakerMatched matchmakerMatched);
    void OnMatchJoin(IMatch match);
    void OnMatchState(IMatchState state);
    void OnMatchPresence(IMatchPresenceEvent presenceEvent);
    void OnMatchLeave();
}

