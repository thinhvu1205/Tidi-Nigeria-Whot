using System.Collections;
using System.Collections.Generic;
using Api;
using Globals;
using UnityEngine;

public class SlotSixiangView : BaseSlotView
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
    protected override List<int[]> PaylineIdList => new List<int[]>
    {
        new int[] {1, 1, 1, 1, 1},
        new int[] {0, 0, 0, 0, 0},
        new int[] {2, 2, 2, 2, 2},
        new int[] {2, 1, 0, 1, 2},
        new int[] {0, 1, 2, 1, 0},
        new int[] {1, 2, 1, 0, 1},
        new int[] {1, 2, 1, 2, 1},
        new int[] {0, 1, 0, 1, 2},
        new int[] {2, 1, 2, 1, 0},
        new int[] {2, 1, 0, 1, 0},
        new int[] {0, 1, 2, 1, 2},
        new int[] {1, 0, 1, 2, 1},
        new int[] {2, 1, 2, 1, 2},
        new int[] {1, 0, 1, 0, 1},
        new int[] {0, 1, 0, 1, 0},
        new int[] {1, 1, 2, 1, 1},
        new int[] {0, 0, 1, 0, 0},
        new int[] {2, 2, 1, 2, 2},
        new int[] {1, 1, 0, 1, 1},
        new int[] {0, 0, 2, 0, 0},
        new int[] {2, 2, 0, 2, 2},
        new int[] {1, 1, 1, 2, 1},
        new int[] {0, 0, 0, 1, 2},
        new int[] {2, 2, 2, 1, 0},
        new int[] {2, 1, 1, 1, 0}
    };
    
    protected override void SetSpinAnimation(SpinType type)
    {
        buttonSpinAnimation.startingAnimation = type switch
        {
            SpinType.NORMAL => "spin",
            SpinType.FREE_NORMAL or SpinType.FREE_AUTO => "freespin",
            SpinType.AUTO => "stop",
            _ => "autospin"
        };
    }
}
