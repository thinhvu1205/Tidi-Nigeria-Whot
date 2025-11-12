using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Globals;
using TMPro;
using UnityEngine;

public class SlotJuicyItem : SlotItem
{
    [SerializeField] private TextMeshProUGUI[] valuePackageTextList;
    public long[] ValuePackageList { get; set; } = new long[3];
    protected override float PositionResetY => base.PositionResetY - 5f;
    protected override float IconScale => 1.01f;
    protected override string ICON_ANIMATION_PATH => "SlotSpine/JuicyGarden/SpineIcon/%id/skeleton_SkeletonData";
    protected override Vector2[] ItemPositionList => new Vector2[]
    {
        new(0, 160),
        new(0, 0),
        new(0, -160),
    };
    public override void SetItemAnimation(int index, bool isWild = false)
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
            bool isWinJackpotBasket = (new int[] { 14, 15, 16 }).Contains(finishView[i]);
            if (value == 0 || isWinJackpotBasket)
            {
                textElement.gameObject.SetActive(false);
                if (isWinJackpotBasket)
                {
                    SetDark(false, i);
                }
            }
            else
            {
                textElement.gameObject.SetActive(true);
                textElement.text = Utility.FormatMoney3(value);
                SetDark(false, i);
            }
        }
    }

    public void ClearValuePackage()
    {
        foreach (TextMeshProUGUI text in valuePackageTextList)
        {
            text.gameObject.SetActive(false);
        }
    }
    
    public override void SetDark(bool isDark, int index = -1)
    {
        if (isDark) spineItem.gameObject.SetActive(false);
        Color colorState = isDark ? Color.gray : Color.white;
        if (index >= 0 && index < slotImageList.Length)
        {
            slotImageList[index].color = colorState;
            valuePackageTextList[index].color = colorState;
        }
        else
        {
            for (int i = 0; i < slotImageList.Length; i++)
            {
                slotImageList[i].color = colorState;
                slotImageList[i].gameObject.SetActive(true);
                valuePackageTextList[i].color = colorState;
                spineItem.gameObject.SetActive(false);
                // Nếu cần clear animation thì xử lý ở đây
                // listSpineItem[i].gameObject.SetActive(false);
                // CollumSpinCtrl.gameView.removeAnimIcon(listSpineItem[i].gameObject);
            }

            // Clear animation list nếu cần
            // listSpineItem.Clear();
        }
    }
}
