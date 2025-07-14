using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotFruitItem : SlotItem
{
    protected override float IconScale => 0.68f;
    protected override float PositionResetY => 900f;
    protected override Vector2[] ItemPositionList => new Vector2[]
    {
        new(0, 120),
        new(0, 0),
        new(0, -120),
    };
    protected override string ICON_ANIMATION_PATH => "SlotSpine/Fruit/SpineIcon/%id/skeleton_SkeletonData";
    
}
