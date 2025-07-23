using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;

public class SlotFruitItem : SlotItem
{
    protected override float IconScale => 0.72f;
    // protected override float PositionResetY => 1000f;
    protected override Vector2[] ItemPositionList => new Vector2[]
    {
        new(0, 120),
        new(0, 0),
        new(0, -120),
    };
    protected override string ICON_ANIMATION_PATH => "SlotSpine/Fruit/SpineIcon/%id/skeleton_SkeletonData";

    public override void SetItemAnimation(int index, bool isWild = false)
    {
        int itemIndex = finishView[index];
        Vector2 posSpine = slotImageList[index].gameObject.GetComponent<RectTransform>().localPosition;
        switch (itemIndex)
        {
            case 8:
            case 9:
            case 10:
                spineItem.transform.localScale = new Vector2(0.8f, 0.8f);
                posSpine = new Vector2(posSpine.x, posSpine.y - 10);
                break;
            case 11:
                spineItem.transform.localScale = new Vector2(0.55f, 0.55f);
                break;
            case 12:
                spineItem.transform.localScale = new Vector2(0.75f, 0.75f);
                break;
            default:
                spineItem.transform.localScale = new Vector2(1, 1);
                break;
        }
        spineItem.gameObject.transform.localPosition = posSpine;
        spineItem.gameObject.SetActive(true);
        string itemAnimationPath = ICON_ANIMATION_PATH.Replace("%id", itemIndex.ToString());
        Utility.PlayAnimationByPath(spineItem, itemAnimationPath, ICON_ANIMATION_NAME);
    }
    
}
