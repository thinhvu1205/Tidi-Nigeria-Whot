using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;

public class SlotIncaColumn : SlotItem
{
    protected override Vector2[] ItemPositionList => new Vector2[]
    {
        new(0, 160),
        new(0, 0),
        new(0, -160),
    };
    protected override string ICON_ANIMATION_PATH => "SlotSpine/InCa/SpineIcon/%id/skeleton_SkeletonData";
    protected override float IconScale => 0.82f;

    public override void SetItemAnimation(int index, bool isWild = false)
    {
        int itemIndex = finishView[index];
        string animPath = "";
        string animName = "animation";
        switch (itemIndex)
        {
            case 0:
                animName = "J";
                animPath = "AJQK";
                break;
            case 1:
                animName = "Q";
                animPath = "AJQK";
                break;
            case 2:
                animName = "K";
                animPath = "AJQK";
                break;
            case 3:
                animName = "A";
                animPath = "AJQK";
                //itemSpine.transform.localScale = new Vector2(0.65f, 0.65f);
                break;
            case 4:
                animName = "bich";
                animPath = "bichcorotep";
                break;
            case 5:
                animName = "co";
                animPath = "bichcorotep";
                break;
            case 6:
                animName = "zo";
                animPath = "bichcorotep";
                break;
            case 7:
                animName = "tep";
                animPath = "bichcorotep";
                break;
            case 8:
                animPath = "binh";
                break;
            case 9:
                animPath = "bird";
                break;
            case 10:
                animPath = "sun";
                break;
            case 11:
                animPath = "wild";
                spineItem.transform.localScale = new Vector2(0.8f, 0.8f);
                break;
            case 12:
                animPath = "scatter";
                spineItem.transform.localScale = new Vector2(0.8f, 0.8f);
                break;
        }
        spineItem.gameObject.transform.localPosition = slotImageList[index].gameObject.GetComponent<RectTransform>().localPosition;
        spineItem.gameObject.SetActive(true);
        string itemAnimationPath = ICON_ANIMATION_PATH.Replace("%id", animPath.ToString());
        Utility.PlayAnimationByPath(spineItem, itemAnimationPath, animName);
    }
}
