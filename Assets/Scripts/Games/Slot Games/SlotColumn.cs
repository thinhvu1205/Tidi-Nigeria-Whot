using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Globals;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SlotColumn : MonoBehaviour
{
    [SerializeField] protected SlotItem defaultItem;
    protected SlotItem resultItem;
    protected List<SlotItem> itemList = new();
    protected BaseSlotView slotView;
    protected int columnIndex = 0;
    public bool IsLastColumn { get { return columnIndex == 4; } }
    public bool IsSpinning { get; set; } = false;

    [Header("Constants")]

    protected const float DEFAULT_SPIN_DURATION = 1.5f;
    protected const float SPEED_NORMAL = 0.18f;
    protected const float SPEED_BACKSPIN_NORMAL = SPEED_NORMAL - 0.05f;
    protected const float SPEED_AUTO = 0.12f;
    protected const float SPEED_BACKSPIN_AUTO = SPEED_AUTO - 0.05f;

    protected void Awake()
    {
        // foreach(Imag)
    }

    protected void Update()
    {

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
        float speed = spinType == SpinType.NORMAL ? SPEED_NORMAL : SPEED_AUTO;
        float backSpinSpeed = spinType == SpinType.NORMAL ? SPEED_BACKSPIN_NORMAL : SPEED_BACKSPIN_AUTO;
        for (int i = 0; i < itemList.Count; i++)
        {
            SlotItem item = itemList[i];
            item.SetInfo(this);
            item.SetFinishIndices(new List<int> { 1, 1, 1 });
            item.StartSpin(speed, backSpinSpeed);
        }
    }

    private IEnumerator SpinDuration()
    {
        float duration = DEFAULT_SPIN_DURATION + 0.3f * (columnIndex - 1);
        yield return new WaitForSeconds(duration);
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

    #region Events
    public void OnAllColumnsStop()
    {
        slotView.OnStopSpin();
    }

    #endregion
    public Vector2 GetItemPositionAtIndex(int index)
    {
        return resultItem.GetItemPositionAtIndex(index);
    }

    public void SetResultItem(SlotItem item)
    {
        resultItem = item;
    }
    public void SetRandomSprite()
    {
        defaultItem.SetRandomData();
    }
}
