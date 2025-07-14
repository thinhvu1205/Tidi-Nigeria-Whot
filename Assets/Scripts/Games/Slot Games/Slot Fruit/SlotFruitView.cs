using System.Collections;
using System.Collections.Generic;
using Api;
using UnityEngine;

public class SlotFruitView : BaseSlotView
{
    protected override Vector2 RECT_SIZE => new(140, 140f);
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
        { SiXiangSymbol.JuicePinapple, 4 },
        { SiXiangSymbol.JuiceMangosteen, 5 },
        { SiXiangSymbol.JuiceWatermelon, 6 },
        { SiXiangSymbol.JuiceStrawberry, 7 },
        { SiXiangSymbol.JuiceStoneDiamond, 8 },
        { SiXiangSymbol.JuiceStoneViolet, 9 },
        { SiXiangSymbol.JuiceStoneGreen, 10 },
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
}
