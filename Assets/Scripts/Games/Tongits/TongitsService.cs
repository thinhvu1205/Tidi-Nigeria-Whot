using System;
using System.Collections.Generic;
using Google.Protobuf;
using Nakama;
using UnityEngine;
using Yuujins.Api.V1;

/// <summary>
/// Lớp client tập trung gửi request Tongits (opcode + protobuf) và nhận response từ Nakama Match State.
/// Khớp server: <c>Server/yuujins/games/tongits</c> + <c>common/proto/api/v1/tongits.proto</c>.
/// Proto C#: <c>YuujinsGenerated/Tongits.cs</c>, <c>Card.cs</c> (namespace <see cref="Yuujins.Api.V1"/>).
/// </summary>
public static class TongitsService
{
    #region Events (server → client, đã parse protobuf)

    /// <summary>Opcode 1 — <see cref="TongitsGameStateUpdate"/>.</summary>
    public static event Action<TongitsGameStateUpdate> OnGameStateUpdate;

    /// <summary>Opcode 2 — <see cref="TongitsDealUpdate"/>.</summary>
    public static event Action<TongitsDealUpdate> OnDealUpdate;

    /// <summary>Opcode 3 — <see cref="TongitsTurnActionUpdate"/>.</summary>
    public static event Action<TongitsTurnActionUpdate> OnTurnActionUpdate;

    /// <summary>Opcode 4 — <see cref="TongitsFightUpdate"/>.</summary>
    public static event Action<TongitsFightUpdate> OnFightStateUpdate;

    /// <summary>Opcode 5 — <see cref="TongitsFinishUpdate"/>.</summary>
    public static event Action<TongitsFinishUpdate> OnFinishUpdate;

    /// <summary>Opcode 6 — <see cref="TongitsReject"/>.</summary>
    public static event Action<TongitsReject> OnReject;

    /// <summary>Lỗi parse hoặc payload rỗng (kèm opcode gốc).</summary>
    public static event Action<long, Exception> OnDispatchError;

    #endregion

    /// <summary>
    /// Các opcode update Tongits (1–6). Dùng để lọc trước khi gọi <see cref="DispatchMatchState"/>.
    /// </summary>
    public static bool IsTongitsUpdateOpCode(long opCode) =>
        opCode >= (long)TongitsOpCodeUpdate.TongitsUpdateGameState &&
        opCode <= (long)TongitsOpCodeUpdate.TongitsUpdateReject;

    /// <summary>
    /// Xử lý <see cref="IMatchState"/> từ Nakama: parse protobuf theo opcode và raise event tương ứng.
    /// Trả về <c>true</c> nếu opcode thuộc Tongits update (1–6) và đã xử lý (kể cả khi parse lỗi).
    /// </summary>
    public static bool DispatchMatchState(IMatchState matchState)
    {
        if (matchState == null) return false;
        var op = matchState.OpCode;
        if (!IsTongitsUpdateOpCode(op)) return false;

        byte[] data = matchState.State;
        if (data == null || data.Length == 0)
        {
            OnDispatchError?.Invoke(op, new InvalidOperationException("Tongits match state payload is empty."));
            return true;
        }
        
        Debug.Log("Tongits match state "+ matchState);

        try
        {
            switch ((TongitsOpCodeUpdate)op)
            {
                case TongitsOpCodeUpdate.TongitsUpdateGameState:
                    OnGameStateUpdate?.Invoke(TongitsGameStateUpdate.Parser.ParseFrom(data));
                    break;
                case TongitsOpCodeUpdate.TongitsUpdateDeal:
                    OnDealUpdate?.Invoke(TongitsDealUpdate.Parser.ParseFrom(data));
                    break;
                case TongitsOpCodeUpdate.TongitsUpdateTurnAction:
                    OnTurnActionUpdate?.Invoke(TongitsTurnActionUpdate.Parser.ParseFrom(data));
                    break;
                case TongitsOpCodeUpdate.TongitsUpdateFightState:
                    OnFightStateUpdate?.Invoke(TongitsFightUpdate.Parser.ParseFrom(data));
                    break;
                case TongitsOpCodeUpdate.TongitsUpdateFinish:
                    OnFinishUpdate?.Invoke(TongitsFinishUpdate.Parser.ParseFrom(data));
                    break;
                case TongitsOpCodeUpdate.TongitsUpdateReject:
                    OnReject?.Invoke(TongitsReject.Parser.ParseFrom(data));
                    break;
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TongitsService] Parse failed op={op}: {ex.Message}");
            OnDispatchError?.Invoke(op, ex);
        }

        return true;
    }

    /// <summary>Gỡ toàn bộ subscriber (gọi khi thoát scene / destroy view để tránh leak).</summary>
    public static void ClearAllSubscribers()
    {
        OnGameStateUpdate = null;
        OnDealUpdate = null;
        OnTurnActionUpdate = null;
        OnFightStateUpdate = null;
        OnFinishUpdate = null;
        OnReject = null;
        OnDispatchError = null;
    }

    #region Send request (client → server)

    static void Send(long opCode, byte[] payload) =>
        DataSender.SendMatchState(opCode, payload ?? Array.Empty<byte>());

    /// <summary>Opcode 1 — không payload.</summary>
    public static void SendDrawCard() =>
        Send((long)TongitsOpCodeRequest.TongitsRequestDrawCard, Array.Empty<byte>());

    /// <summary>Opcode 2 — <see cref="TongitsDiscardRequest"/> (rank 1–13, suit 0–3).</summary>
    public static void SendDiscard(Card card)
    {
        var req = new TongitsDiscardRequest { Card = card };
        Send((long)TongitsOpCodeRequest.TongitsRequestDiscard, req.ToByteArray());
    }

    /// <summary>Opcode 2 — từ rank/suit proto.</summary>
    public static void SendDiscard(int rank, int suit) =>
        SendDiscard(new Card { Rank = rank, Suit = suit });

    /// <summary>
    /// Opcode 2 — map index bài 0–51 (suit*13 + rankIndex) sang rank/suit proto như server <c>IndexFromRankSuit</c>.
    /// </summary>
    public static void SendDiscardFromDeckIndex(int index0To51)
    {
        if (index0To51 < 0 || index0To51 > 51)
        {
            Debug.LogWarning("[TongitsService] SendDiscardFromDeckIndex: index out of 0..51");
            return;
        }
        int rank = index0To51 % 13 + 1;
        int suit = index0To51 / 13;
        SendDiscard(rank, suit);
    }

    /// <summary>Opcode 3.</summary>
    public static void SendSendCard(TongitsSendCardRequest request) =>
        Send((long)TongitsOpCodeRequest.TongitsRequestSendCard, request.ToByteArray());

    /// <summary>Opcode 4.</summary>
    public static void SendHaPhom(TongitsHaPhomRequest request) =>
        Send((long)TongitsOpCodeRequest.TongitsRequestHaPhom, request.ToByteArray());

    /// <summary>Opcode 5 — không payload.</summary>
    public static void SendFight() =>
        Send((long)TongitsOpCodeRequest.TongitsRequestFight, Array.Empty<byte>());

    /// <summary>Opcode 6 — không payload.</summary>
    public static void SendAcceptFight() =>
        Send((long)TongitsOpCodeRequest.TongitsRequestAcceptFight, Array.Empty<byte>());

    /// <summary>Opcode 7.</summary>
    public static void SendDeclare(TongitsDeclareRequest request) =>
        Send((long)TongitsOpCodeRequest.TongitsRequestDeclare, request.ToByteArray());

    /// <summary>Opcode 7 — tiện ích: nhóm bài từ list rank/suit.</summary>
    public static void SendDeclareGroups(IEnumerable<IEnumerable<Card>> groups)
    {
        var req = new TongitsDeclareRequest();
        foreach (var g in groups)
        {
            var dg = new TongitsDeclareGroup();
            foreach (var c in g)
                dg.Cards.Add(c);
            req.Groups.Add(dg);
        }
        SendDeclare(req);
    }

    #endregion

    #region Helpers

    /// <summary>Tạo <see cref="Card"/> proto từ rank/suit.</summary>
    public static Card MakeCard(int rank, int suit) => new Card { Rank = rank, Suit = suit };

    /// <summary>Chuyển list (rank, suit) thành repeated Card cho request.</summary>
    public static void AddCards(ICollection<Card> target, IEnumerable<(int rank, int suit)> cards)
    {
        foreach (var (r, s) in cards)
            target.Add(MakeCard(r, s));
    }

    #endregion
}
