using System;
using System.Collections.Generic;
using Api;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class SlotSixiangView : BaseSlotSymbolView
{   
    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol._10, 0 },
        { SiXiangSymbol.J, 1 },
        { SiXiangSymbol.Q, 2 },
        { SiXiangSymbol.K, 3 },
        { SiXiangSymbol.A, 4 },
        { SiXiangSymbol.BlueDragon, 5 },
        { SiXiangSymbol.WhiteTiger, 6 },
        { SiXiangSymbol.Warrior, 7 },
        { SiXiangSymbol.VermilionBird, 8 },
        { SiXiangSymbol.Scatter, 9 },
        { SiXiangSymbol.Wild, 10 },

    };
   
}
