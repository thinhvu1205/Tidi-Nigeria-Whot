using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextMarquee : MonoBehaviour
{
    public TextMeshProUGUI text;
    private float duration = 2f; // thời gian để chạy 1 chiều

    private RectTransform textRT;
    private RectTransform maskRT;

    void Start()
    {
        textRT = text.GetComponent<RectTransform>();
        maskRT = GetComponent<RectTransform>();

        StartMarquee();
    }

    void StartMarquee()
    {
        // Cập nhật width thực tế của text
        text.ForceMeshUpdate();

        float maskWidth = maskRT.rect.width;
        float textWidth = text.preferredWidth;

        // Nếu text không dài hơn mask thì không animate
        Debug.Log("textWidth: " + textWidth);
        Debug.Log("maskWidth: " + maskWidth);
        if (textWidth <= maskWidth)
        {
            textRT.anchoredPosition = Vector2.zero;
            return;
        }

        // Điểm bắt đầu (text nằm sát trái)
        float startX = 0;
        // Điểm kết thúc (đi hết bên trái)
        float endX = -(textWidth - maskWidth);

        // Reset vị trí
        textRT.anchoredPosition = new Vector2(startX, 0);

        // Tween qua lại liên tục
        textRT
            .DOAnchorPosX(endX, duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);  // ←→ loop
    }
}
