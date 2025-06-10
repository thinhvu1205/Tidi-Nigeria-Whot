using Nakama;
using UnityEngine;

namespace Games.Whot
{
    public class WhotHandler : IGameHandler
    {
        public void OnMatchJoin(IApiMatch match) {
            Debug.Log("WHOT Join Match: " + match.MatchId);
            // parse match.Label hoặc chờ MatchState 
        }

        public void OnMatchState(IMatchState state) {
            Debug.Log("WHOT Match State: " + state.OpCode);
            // parse JSON và xử lý tùy theo OpCode
        }

        public void OnMatchPresence(IMatchPresenceEvent presenceEvent) {
            // xử lý khi người chơi vào/ra
        }

        public void OnMatchLeave() {
            // cleanup
        }
    }
}
