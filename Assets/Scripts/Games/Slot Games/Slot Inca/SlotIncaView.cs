using System.Collections;
using System.Collections.Generic;
using Proto;
using Globals;
using UnityEngine;

public class SlotIncaView : BaseSlotView
{
    protected override Vector2 RECT_SIZE => new(163, 125f);
    protected override string BACKGROUND_FREE_SPIN_ANIMATION_PATH => "SlotSpine/InCa/bgFreeSpin/skeleton_SkeletonData";
    protected override string BIG_WIN_ANIMATION_PATH => "SlotSpine/Common/Bigwin/skeleton_SkeletonData";
    protected override string MEGA_WIN_ANIMATION_PATH => "SlotSpine/Common/Bigwin/skeleton_SkeletonData";
    protected override string BIG_WIN_ANIMATION_NAME => "bigwin";
    protected override string MEGA_WIN_ANIMATION_NAME => "megawin";

    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol.K, 0 },
        { SiXiangSymbol.J, 1 },
        { SiXiangSymbol.A, 2 },
        { SiXiangSymbol.Q, 3 },
        { SiXiangSymbol.SuitHearts, 4 },
        { SiXiangSymbol.SuitDiamonds, 5 },
        { SiXiangSymbol.SuitSpades, 6 },
        { SiXiangSymbol.SuitClubs, 7 },
        { SiXiangSymbol.Sun, 8 },
        { SiXiangSymbol.EagleGaruda, 9 },
        { SiXiangSymbol.Antique, 10 },
        { SiXiangSymbol.Wild, 11 },
        { SiXiangSymbol.Scatter, 12 }
    };
    protected override List<int[]> PaylineIdList => new List<int[]>
    {
        new int[] {1, 1, 1, 1, 1},
        new int[] {0, 0, 0, 0, 0},
        new int[] {2, 2, 2, 2, 2},
        new int[] {0, 1, 2, 1, 0},
        new int[] {2, 1, 0, 1, 2},
        new int[] {0, 0, 1, 2, 2},
        new int[] {2, 2, 1, 0, 0},
        new int[] {1, 0, 1, 2, 1},
        new int[] {1, 2, 1, 0, 1},
        new int[] {1, 0, 0, 1, 0},
        new int[] {1, 2, 2, 1, 2},
        new int[] {0, 1, 0, 0, 1},
        new int[] {2, 1, 2, 2, 1},
        new int[] {0, 2, 0, 2, 0},
        new int[] {2, 0, 2, 0, 2},
        new int[] {1, 0, 2, 0, 1},
        new int[] {1, 2, 0, 2, 1},
        new int[] {0, 1, 1, 1, 0},
        new int[] {2, 1, 1, 1, 2},
        new int[] {0, 2, 2, 2, 0},
    };

    protected override void SetSpinAnimation(SpinType type)
    {
        buttonSpinAnimation.startingAnimation = type switch
        {
            SpinType.NORMAL => "spinHoldforAuto",
            SpinType.FREE_NORMAL or SpinType.FREE_AUTO => "freespin",
            SpinType.AUTO => "stop",
            _ => "autospin"
        };
    }
}
