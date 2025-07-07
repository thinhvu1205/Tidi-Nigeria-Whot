using System.Collections;
using UnityEngine;

public class LayoutSizeControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform rectTransform;

    [Header("Layout Settings")]
    [SerializeField] private int spacing = 0;
    [SerializeField] private int paddingTop = 0;
    [SerializeField] private int paddingBottom = 0;
    [SerializeField] private int paddingLeft = 0;
    [SerializeField] private int paddingRight = 0;

    [SerializeField] private bool autoUpdate = true; // Tự động update theo interval
    [SerializeField] private float updateInterval = 0.25f;
    [SerializeField] private int maxAutoUpdates = 10; // Số lần update tối đa

    public enum LayoutType
    {
        Vertical,
        Horizontal
    }
    [SerializeField] private LayoutType layoutType = LayoutType.Vertical;

    private int updateCount = 0;
    private float timer = 0f;

    private void Update()
    {
        if (!autoUpdate || updateCount >= maxAutoUpdates) return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            updateCount++;
            Refresh();
        }
    }

    /// <summary>
    /// Tính toán và cập nhật lại size của RectTransform
    /// </summary>
    public void Refresh()
    {
        float totalSize = 0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect == null) continue;

            if (layoutType == LayoutType.Vertical)
            {
                totalSize += childRect.sizeDelta.y;
            }
            else
            {
                totalSize += childRect.sizeDelta.x;
            }

            // Thêm spacing trừ lần cuối
            if (i < transform.childCount - 1)
                totalSize += spacing;
        }

        // Cộng thêm padding
        if (layoutType == LayoutType.Vertical)
        {
            totalSize += paddingTop + paddingBottom;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, totalSize);
        }
        else
        {
            totalSize += paddingLeft + paddingRight;
            rectTransform.sizeDelta = new Vector2(totalSize, rectTransform.sizeDelta.y);
        }
    }

    /// <summary>
    /// Reset lại số lần tự động update và tính toán lại ngay
    /// </summary>
    public void ResetAndRefresh()
    {
        updateCount = 0;
        timer = 0f;
        Refresh();
    }
}
