using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Globals
{
    #region Config

    #endregion

    #region Helpers
    
    #endregion

    #region Enums
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
        FACEBOOK = 2,
        FACEBOOK_INSTANT = 3,
        APPLE_ID = 4,
        REG_ACC = 5
    }

    #endregion
}

