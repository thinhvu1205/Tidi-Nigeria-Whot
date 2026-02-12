using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FriendSortBox : MonoBehaviour
{
    [SerializeField] private RectTransform filterBox;
    [SerializeField] private Button buttonSortIntimacyPoint, buttonSortVipLevel, buttonSortOnlineStatus;
    [SerializeField] private float showY = -90f;
    [SerializeField] private float hideY = 200f;
    [SerializeField] private float duration = 0.25f;
    private CanvasGroup canvasGroup;
    private Tween currentTween;
    private FriendsView friendsView;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        buttonSortIntimacyPoint.onClick.AddListener(() => OnClickSort(FriendsView.SortMode.INTIMACY_POINT));
        buttonSortVipLevel.onClick.AddListener(() => OnClickSort(FriendsView.SortMode.VIP_LEVEL));
        buttonSortOnlineStatus.onClick.AddListener(() => OnClickSort(FriendsView.SortMode.ONLINE_STATUS));
    }

    public void Setup(FriendsView friendsView)
    {
        this.friendsView = friendsView;
    }

    public void OnClickSort(FriendsView.SortMode sortMode)
    {
        friendsView.ChangeSortMode(sortMode);
        Hide();
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
