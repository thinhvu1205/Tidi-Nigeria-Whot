using System;
using Nakama;
using UnityEngine;
using Yuujins.Api.V1;

/// <summary>
/// Match-layer updates (opcode ≥ 100), không thuộc từng game. Server: <c>core/internal/matchhandler</c> broadcast sau join/leave.
/// Proto: <see cref="MatchRosterUpdate"/> trong <c>Assets/YuujinsGenerated/GameCommon.cs</c> (namespace <c>Yuujins.Api.V1</c>).
/// </summary>
public static class MatchRosterService
{
    /// <summary>Opcode 100 — snapshot/delta: <c>players</c>, <c>joined_players</c> / <c>left_players</c> (đủ profile), <c>revision</c>, <c>is_snapshot</c>.</summary>
    public static event Action<MatchRosterUpdate> OnRosterUpdate;

    /// <summary>Lỗi parse hoặc payload rỗng (kèm opcode).</summary>
    public static event Action<long, Exception> OnDispatchError;

    /// <summary>Gửi yêu cầu full roster (server trả lại opcode 100). Payload rỗng.</summary>
    public static void RequestRosterSnapshot()
    {
        DataSender.SendMatchState((long)MatchOpCodeUpdate.MatchRequestRosterSnapshot, System.Array.Empty<byte>());
    }

    /// <summary>
    /// Trả về <c>true</c> nếu opcode là match-layer (chỉ 100 — payload roster).
    /// </summary>
    public static bool DispatchMatchState(IMatchState matchState)
    {
        if (matchState == null) return false;
        var op = matchState.OpCode;
        if (op != (long)MatchOpCodeUpdate.MatchUpdateRoster) return false;
        byte[] data = matchState.State;
        if (data == null || data.Length == 0)
        {
            OnDispatchError?.Invoke(op, new InvalidOperationException("Match roster payload is empty."));
            return true;
        }

        try
        {
            OnRosterUpdate?.Invoke(MatchRosterUpdate.Parser.ParseFrom(data));
        }
        catch (Exception ex)
        {
            OnDispatchError?.Invoke(op, ex);
        }

        return true;
    }
}
