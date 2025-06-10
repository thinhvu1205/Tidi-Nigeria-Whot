using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public interface IGameHandler
{
    void OnMatchJoin(IApiMatch match);
    void OnMatchState(IMatchState state);
    void OnMatchPresence(IMatchPresenceEvent presenceEvent);
    void OnMatchLeave();
}

