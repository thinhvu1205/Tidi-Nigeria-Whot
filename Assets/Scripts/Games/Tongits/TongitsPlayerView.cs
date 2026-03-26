using TMPro;
using UnityEngine;

/// <summary>
/// Player slot Tongits — hiển thị số lá ẩn đối thủ (tương tự showNumbOfCard phía client cũ).
/// Gán <see cref="textHandCount"/> trên prefab <c>TongitsPlayerView</c> nếu cần.
/// </summary>
public class TongitsPlayerView : BasePlayerView
{
    [SerializeField] private TextMeshProUGUI textHandCount;

    public void SetHiddenHandCount(int count)
    {
        if (textHandCount == null) return;
        if (count <= 0)
        {
            textHandCount.gameObject.SetActive(false);
            return;
        }
        textHandCount.gameObject.SetActive(true);
        textHandCount.text = count.ToString();
    }
}
