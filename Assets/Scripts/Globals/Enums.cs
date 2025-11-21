using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Globals
{
    public enum GameID
    {

    }

    public enum ConnectionStatus
    {
        NONE,
        CONNECTING,
        CONNECTED,
        DISCONNECTED,
    }

    public enum GameState
    {
        WAITING = 0,
        PLAYING = 1,
        VIEWING = 2
    }

    public enum LoginType
    {
        NONE = -1,
        NORMAL = 0,
        PLAYNOW = 1,
    }

    public enum PrefabType
    {
        WhotCard,
        Card,
        ChipPlayerWhot,
        ChipPlayerBaccarat,
        ChipPlayerBlackjack,
        ChipPlayerHkPoker,
        BoxBetPlayerHkPoker,
    }

    public enum SpinType
    {
        NORMAL,
        AUTO,
        FREE_NORMAL,
        FREE_AUTO
    }
    public enum SlotGameState
    {
        PREPARE,
        SPINNING,
        SHOWING_RESULT,
        JOIN_GAME
    }

    public enum MetaBankAction
    {
        Unspecified = 0,
        SendGift = 1,
        RecvGift = 2,
        RevertSendGift = 3,
        PushToSafe = 4,
        Withdraw = 5
    }
}

