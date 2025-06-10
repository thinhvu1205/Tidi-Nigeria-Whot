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

    public void OnMatchState(IMatchState state) {
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
}

