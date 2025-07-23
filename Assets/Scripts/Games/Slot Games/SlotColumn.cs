using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using Globals;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SlotColumn : MonoBehaviour
{
    [SerializeField] protected SlotItem defaultItem;
    public SlotItem ResultItem { get; set; }
    protected List<SlotItem> itemList = new();
    protected BaseSlotView slotView;
    protected int columnIndex = 0;
    public bool IsLastColumn { get { return columnIndex == 4; } }
    public bool IsSpinning { get; set; } = false;
    public bool IsShowingThirdScatter { get; set; } = false;
    public float ExtraTime { get; set; } = 0f;

    [Header("Constants")]

    protected const float DEFAULT_SPIN_DURATION = 1.5f;
    protected const float SPEED_NORMAL = 0.18f;
    protected const float SPEED_BACKSPIN_NORMAL = SPEED_NORMAL - 0.05f;
    protected const float SPEED_AUTO = 0.12f;
    protected const float SPEED_BACKSPIN_AUTO = SPEED_AUTO - 0.05f;

    protected void Awake()
    {
        ResultItem = defaultItem;
        if (itemList.Count == 0)
        {
            SlotItem item2 = Instantiate(defaultItem.gameObject, transform).GetComponent<SlotItem>();
            item2.transform.localPosition = new Vector2(defaultItem.transform.localPosition.x, defaultItem.transform.localPosition.y + defaultItem.GetComponent<RectTransform>().sizeDelta.y);
            item2.SetRandomData();

            SlotItem item3 = Instantiate(defaultItem.gameObject, transform).GetComponent<SlotItem>();
            item3.transform.localPosition = new Vector2(defaultItem.transform.localPosition.x, defaultItem.transform.localPosition.y + 2 * defaultItem.GetComponent<RectTransform>().sizeDelta.y);
            item3.SetRandomData();

            itemList.Add(item3);
            itemList.Add(item2);
            itemList.Add(defaultItem);
        }
    }

    public void SetInfo(BaseSlotView slotView, int index)
    {
        this.slotView = slotView;
        columnIndex = index;
    }
    public void StartSpin(SpinType spinType)
    {
        IsSpinning = true;
        StartCoroutine(SpinDuration());

        float speed = spinType == SpinType.NORMAL ? SPEED_NORMAL : SPEED_AUTO;
        float backSpinSpeed = spinType == SpinType.NORMAL ? SPEED_BACKSPIN_NORMAL : SPEED_BACKSPIN_AUTO;
        for (int i = 0; i < itemList.Count; i++)
        {
            SlotItem item = itemList[i];
            item.SetInfo(this);
            item.StartSpin(speed, backSpinSpeed);
        }
    }

    private IEnumerator SpinDuration()
    {
        float delayBetweenColumns = slotView.GetSpinType() == SpinType.NORMAL ? 0.3f : 0.2f;
        float defaultSpinDuration = slotView.GetSpinType() == SpinType.NORMAL ? DEFAULT_SPIN_DURATION : DEFAULT_SPIN_DURATION * 0.75f;
        float duration = defaultSpinDuration + delayBetweenColumns * (columnIndex - 1);

        float timer = 0f;
        while (timer < duration + ExtraTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        IsSpinning = false;
    }

    public void SetDarkAllItems()
    {
        foreach (SlotItem item in itemList)
        {
            item.SetDark(true);
        }
    }

    public void SetDarkItemAtIndex(int index)
    {
        foreach (SlotItem item in itemList)
        {
            item.SetDark(true, index);
        }
    }

    public void SetLightAllItems()
    {
        foreach (SlotItem item in itemList)
        {
            item.SetDark(false);
        }
    }

    public void SetLightItemAtIndex(int index)
    {
        foreach (SlotItem item in itemList)
        {
            item.SetDark(false, index);
        }
    }

    public void SetAnimationForItemAtIndex(int itemIndex)
    {
        ResultItem.SetItemAnimation(itemIndex);
    }

    public void ShowPackageValue()
    {
        foreach (SlotItem item in itemList)
        {
            if (item is SlotJuicyItem juicyItem)
            {
                juicyItem.SetItemValuePackage();
            }
        }
    }

    public void SetPackageValue(long[] packageValue)
    {
        foreach (SlotItem item in itemList)
        {
            if (item is SlotJuicyItem juicyItem)
            {
                juicyItem.ClearValuePackage();
                juicyItem.ValuePackageList = packageValue;
            }
        }
    }

    #region Events
    public void OnAllColumnsStop()
    {
        slotView.OnStopSpin();
    }

    public void OnColumnStop()
    {
        slotView.OnColumnStop(columnIndex);
    }

    public void CheckThirdScatter()
    {
        slotView.CheckThirdScatter(columnIndex);
    }

    #endregion
    public Vector2 GetItemPositionAtIndex(int index)
    {
        return ResultItem.GetItemPositionAtIndex(index);
    }

    public void SetStartView(int[] symbolIdArray)
    {
        defaultItem.SetFinishIndices(symbolIdArray);
        defaultItem.SetFinishView();
    }

    public void SetFinishView(int[] symbolIdArray)
    {
        foreach (SlotItem item in itemList)
        {
            item.SetFinishIndices(symbolIdArray);
        }
    }

    public void SetSpreadFinishView(int[] symbolIdArray)
    {
        foreach (SlotItem item in itemList)
        {
            item.SetSpreadFinishIndices(symbolIdArray);
        }
    }

    public void SetRandomSprite()
    {
        defaultItem.SetRandomData();
    }

    public int GetScatterCount() => slotView.ScatterCount;
    public void IncreaseScatterCount()
    {
        slotView.ScatterCount++;
    }
}
