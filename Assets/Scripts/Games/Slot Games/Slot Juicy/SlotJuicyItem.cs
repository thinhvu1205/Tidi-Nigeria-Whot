using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;

public class SlotJuicyItem : SlotItem
{
    [SerializeField] private TextMeshProUGUI[] valuePackageTextList;
    public long[] ValuePackageList { get; set; } = new long[3];
    protected override float PositionResetY => base.PositionResetY - 5f;
    protected override float IconScale => 1f;
    protected override string ICON_ANIMATION_PATH => "SlotSpine/JuicyGarden/SpineIcon/%id/skeleton_SkeletonData";
    protected override Vector2[] ItemPositionList => new Vector2[]
    {
        new(0, 160),
        new(0, 0),
        new(0, -160),
    };
    public override void SetItemAnimation(int index)
    {
        if (index < 13)
        {
            int itemIndex = finishView[index];
            Vector2 posSpine = slotImageList[index].gameObject.GetComponent<RectTransform>().localPosition;
            switch (itemIndex)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    spineItem.transform.localScale = new Vector2(1.2f, 1.2f);
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                    {
                        spineItem.transform.localScale = new Vector2(0.9f, 0.9f);
                        break;
                    }
                case 8:
                case 9:
                case 10:
                    {
                        spineItem.transform.localScale = new Vector2(0.75f, 0.75f);
                        posSpine = new Vector2(posSpine.x, posSpine.y - 20);
                        break;
                    }
                case 12:
                case 11:
                    spineItem.transform.localScale = new Vector2(0.65f, 0.65f);
                    break;
                default:
                    spineItem.transform.localScale = Vector2.one;
                    break;
            }
            spineItem.gameObject.transform.localPosition = posSpine;
            spineItem.gameObject.SetActive(true);
            string itemAnimationPath = ICON_ANIMATION_PATH.Replace("%id", itemIndex.ToString());
            Utility.PlayAnimationByPath(spineItem, itemAnimationPath, ICON_ANIMATION_NAME);
        }
        // else
        // {
        //     setIconData(index, id);

        // }
    }

    public override void SetItemValuePackage()
    {
        for (int i = 0; i < valuePackageTextList.Length; i++)
        {
            long value = ValuePackageList[i];
            TextMeshProUGUI textElement = valuePackageTextList[i];

            if (value == 0)
            {
                textElement.gameObject.SetActive(false);
            }
            else
            {
                textElement.gameObject.SetActive(true);
                textElement.text = Utility.FormatMoney3(value); // hoặc FormatMoney nếu cần
            }
        }
    }

    public void ClearValuePackage()
    {
        foreach(TextMeshProUGUI text in valuePackageTextList)
        {
            text.gameObject.SetActive(false);
        }
    }
}
