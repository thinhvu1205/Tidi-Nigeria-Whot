using System.Collections;
using System.Collections.Generic;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class SlotTarzanItem : SlotItem
{
    protected override float IconScale => 0.9f;
    protected override string ICON_ANIMATION_PATH => "SlotSpine/Tarzan/SpineIcon/%id/skeleton_SkeletonData";
    private string ICON_WILD_ANIMATION_NAME => "wild";
    protected override Vector2[] ItemPositionList => new Vector2[]
    {
        new(0, 155),
        new(0, 0),
        new(0, -155),
    };
    private int wildAnimationCallCount = 0;
    private List<int> listIdAnimal = new(){ 4, 5, 6, 7 };

    public override void SetFinishView()
    {
        for (int i = 0; i < finishView.Length; i++)
        {
            Image image = slotImageList[i];
            int spriteIndex = finishView[i];
            if (spriteIndex == 14) spriteIndex = 11;
            image.sprite = spriteList[spriteIndex];
            image.SetNativeSize();

            if (finishView[i] < 4)
            {
                slotImageList[i].transform.localScale = new Vector2(0.70f, 0.70f);
            }
            else if (finishView[i] == 12)
            {
                slotImageList[i].transform.localScale = new Vector2(0.45f, 0.45f);
            }
            else
            {
                slotImageList[i].transform.localScale = new Vector2(0.85f, 0.85f);
            }
        }
    }

    private void SetSpreadFinishView()
    {
        for (int i = 0; i < finishView.Length; i++)
        {
            Image image = slotImageList[i];
            int spriteIndex = finishView[i];
            if (spreadFinishView[i] == 11)
            {
                spriteIndex += 11;
            }
            if (spriteIndex == 14) spriteIndex = 11;
            image.sprite = spriteList[spriteIndex];
            image.SetNativeSize();

            if (finishView[i] < 4)
            {
                slotImageList[i].transform.localScale = new Vector2(0.70f, 0.70f);
            }
            else if (finishView[i] == 12)
            {
                slotImageList[i].transform.localScale = new Vector2(0.45f, 0.45f);
            }
            else
            {
                slotImageList[i].transform.localScale = new Vector2(0.85f, 0.85f);
            }
        }
    }


    public override void SetItemAnimation(int index, bool isWild = false)
    {
        int itemIndex = finishView[index];
        Vector2 posSpine = slotImageList[index].gameObject.GetComponent<RectTransform>().localPosition;

        SkeletonGraphic targetSpine;

        if (isWild && wildAnimationCallCount > 0)
        {
            // ✅ Từ lần thứ 2, clone spineItem
            targetSpine = Instantiate(spineItem, spineItem.transform.parent);
        }
        else
        {
            // ✅ Lần đầu vẫn dùng spineItem gốc
            targetSpine = spineItem;
        }

        wildAnimationCallCount++; 

        targetSpine.transform.localScale = itemIndex switch
        {
            0 or 1 or 2 or 3 => (Vector3)new Vector2(0.65f, 0.65f),
            4 or 5 or 6 or 7 or 8 or 9 or 10 or 13 => (Vector3)new Vector2(0.4f, 0.4f),
            12 or 15 or 16 or 17 or 18 => (Vector3)new Vector2(0.45f, 0.45f),
            _ => (Vector3)Vector2.one,
        };

        targetSpine.gameObject.transform.localPosition = posSpine;
        targetSpine.gameObject.SetActive(true);

        string itemAnimationPath = ICON_ANIMATION_PATH.Replace("%id", itemIndex.ToString());
        string animationName = (isWild && listIdAnimal.IndexOf(itemIndex) != -1) ? ICON_WILD_ANIMATION_NAME : ICON_ANIMATION_NAME;
        Utility.PlayAnimationByPath(targetSpine, itemAnimationPath, animationName);
        targetSpine.AnimationState.Complete += delegate
        {
            targetSpine.gameObject.SetActive(false);
        };

    }
    
    public void TransformToWild()
    {
        for (int i = 0; i < finishView.Length; i++)
        {
            if (listIdAnimal.IndexOf(finishView[i]) != -1)
            {
                SetItemAnimation(i, true);
            }
        }
        SetSpreadFinishView();
    }
}
