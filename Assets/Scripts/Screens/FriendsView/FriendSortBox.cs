using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FriendSortBox : MonoBehaviour
{
    [SerializeField] private RectTransform filterBox;
    [SerializeField] private float showY = -90f;
    [SerializeField] private float hideY = 200f;
    [SerializeField] private float duration = 0.25f;
    private CanvasGroup canvasGroup;
    private Tween currentTween;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void Show()
    {
        currentTween?.Kill();

        filterBox.gameObject.SetActive(true);
        filterBox.anchoredPosition = new Vector2(
            filterBox.anchoredPosition.x,
            hideY
        );

        currentTween = DOTween.Sequence()
            .Append(filterBox.DOAnchorPosY(showY, duration))
            .Join(canvasGroup.DOFade(1f, 0.5f))
            .SetEase(Ease.OutCubic);
    }

    public void Hide()
    {
        currentTween?.Kill();

        currentTween = DOTween.Sequence()
            .Append(filterBox.DOAnchorPosY(hideY, duration))
            .Join(canvasGroup.DOFade(0f, 0.05f))
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                filterBox.gameObject.SetActive(false);
            });
    }
}
