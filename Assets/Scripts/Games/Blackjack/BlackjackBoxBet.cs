using System.Collections;
using System.Collections.Generic;
using Common.Pool;
using DG.Tweening;
using Games.Card;
using Globals;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackBoxBet : MonoBehaviour
{
    [Header(" Texts ")]
    [SerializeField] private TextMeshProUGUI textChipValue;
    [SerializeField] private TextMeshProUGUI textTotalBet;
    [SerializeField] private TextMeshProUGUI textScore;

    [Header(" Images ")]
    [SerializeField] private Image iconChip;
    [SerializeField] private Image imageChip;
    [SerializeField] private Image imageState;
    [SerializeField] private Image imageScoreBox;

    [Header(" List Sprite ")]
    [SerializeField] private Sprite[] listImageChip;
    [SerializeField] private Sprite[] listImageState; // 0: Blackjack, 1: Bust, 2: Double, 3: Hit, 4: Push, 5: Split, 6: Stand
    [SerializeField] private Sprite[] listImageScoreBox; // 0: Normal, 1: Bust, 2: Blackjack, 3: Blackjack Pro

    [Header(" Transforms ")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform chipContainer;
    [SerializeField] private Transform effectContainer;

    [Header(" Spine Animations ")]
    [SerializeField] private SkeletonGraphic animationWaiting;
    [SerializeField] private SkeletonGraphic animationBlackjack;
    [SerializeField] private SkeletonGraphic animationBust;
    [SerializeField] private SkeletonGraphic animationWow;
    [SerializeField] private BlackjackBoxBet secondBoxBet;
    public Transform GetCardPosition => cardContainer;
    public BlackjackBoxBet SecondBoxBet => secondBoxBet;
    public bool isSecondBox = false;
    [HideInInspector] public List<CardModel> listCardModel = new();
    private const float CARD_SPACING = 35f;
    private readonly List<Tween> highlightTweens = new List<Tween>();
    private float boxWidth;
    private long totalBet;
    private int maxPoint;
    private int index;
    private bool isEnlarging = false;
    public Vector2 BoxPosition { get; private set; }
    public bool HasBet { get; set; } = false;
    private Vector2 initialPosition;

    private void Awake()
    {
        BoxPosition = transform.localPosition;
        boxWidth = GetComponent<RectTransform>().rect.width;
        if (!isSecondBox)
            Reset();
    }
    public void SetInfo(int index, Vector2 initialPosition)
    {
        this.index = index;
        this.initialPosition = initialPosition;
    }

    public void ShowScore(int point, int minPoint = 0, int maxPoint = 0, BlackjackHandType type = BlackjackHandType.Normal)
    {
        // ----- Hiện score text với scale animation -----
        textScore.gameObject.SetActive(true);
        float totalWidth = (listCardModel.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = listCardModel.Count;
        Vector2 targetPos = new(startX + newIndex * CARD_SPACING, imageScoreBox.transform.localPosition.y);
        imageScoreBox.transform.localPosition = targetPos;
        imageScoreBox.gameObject.SetActive(true);
        imageScoreBox.transform.localScale = Vector3.zero; // bắt đầu nhỏ
        imageScoreBox.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        if (minPoint != maxPoint && point != 21)
        {
            textScore.text = $"{minPoint}/{maxPoint}";
            this.maxPoint = maxPoint;
        }
        else
        {
            this.maxPoint = point;
            textScore.text = point.ToString();
        }

        if (point == 21)
        {
            imageScoreBox.sprite = listImageScoreBox[2]; // Blackjack
        }
        else if (point < 21)
        {
            imageScoreBox.sprite = listImageScoreBox[0]; // Normal
        }
        else
        {
            imageScoreBox.sprite = listImageScoreBox[1]; // Bust
        }

        effectContainer.gameObject.SetActive(true);

        // ----- Xử lý animation theo hand type -----
        switch (type)
        {
            case BlackjackHandType.Blackjack:
                animationBlackjack.gameObject.SetActive(true);
                animationBlackjack.AnimationState.SetAnimation(0, "animation", false);

                animationBlackjack.AnimationState.Complete += delegate
                {
                    animationBlackjack.gameObject.SetActive(false);
                    imageState.sprite = listImageState[0]; // Blackjack
                    imageState.gameObject.SetActive(true);
                };
                break;

            case BlackjackHandType._21P:
                animationWow.gameObject.SetActive(true);
                animationWow.AnimationState.SetAnimation(0, "animation", false);

                animationWow.AnimationState.Complete += delegate
                {
                    animationWow.gameObject.SetActive(false);
                };

                break;
            case BlackjackHandType.Busted:
                animationBust.gameObject.SetActive(true);
                animationBust.AnimationState.SetAnimation(0, "animation", false);

                animationBust.AnimationState.Complete += delegate
                {
                    animationBust.gameObject.SetActive(false);
                    imageState.sprite = listImageState[1]; // Bust
                    imageState.gameObject.SetActive(true);
                };
                break;

            default:
                break;
        }
    }

    public void ShowHigherScore()
    {
        ShowScore(maxPoint);
    }

    public void HideScore()
    {
        imageState.gameObject.SetActive(false);
        imageScoreBox.transform.localScale = Vector3.one;
        imageScoreBox.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                imageScoreBox.gameObject.SetActive(false);
            });
    }

    public void SetBetValue(int index, long value, long totalValue, bool isWaiting = false)
    {
        Debug.Log("SetBetValue: " + value);
        totalBet = totalValue;
        if (index < 0)
        {
            imageChip.gameObject.SetActive(false);
            textTotalBet.text = Utility.FormatMoney(totalValue, true);
            iconChip.gameObject.SetActive(false);
            textTotalBet.gameObject.SetActive(false);
            return;
        }
        if (!isSecondBox)
        {
            imageChip.gameObject.SetActive(true);
            textChipValue.gameObject.SetActive(true);
            textChipValue.text = Utility.FormatMoney(value, true);
            imageChip.sprite = listImageChip[index];
        }
        iconChip.gameObject.SetActive(true);
        textTotalBet.gameObject.SetActive(true);
        textTotalBet.text = Utility.FormatMoney(totalValue, true);

        animationWaiting.gameObject.SetActive(isWaiting);

        if (secondBoxBet != null)
            secondBoxBet.SetBetValue(index, value, totalValue, isWaiting);
    }

    public Vector2 GetNewCardPosition()
    {
        float totalWidth = (listCardModel.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = listCardModel.Count;
        float offsetY = isEnlarging ? 30f : 0f;
        Vector2 targetPos = new(startX + newIndex * CARD_SPACING, cardContainer.localPosition.y  + offsetY);

        return transform.TransformPoint(targetPos); ;
    }

    public void UpdateContainerWidth()
    {
        RectTransform rect = gameObject.transform as RectTransform;
        float totalWidth = boxWidth;
        if (listCardModel.Count > 2)
        {
            totalWidth += (listCardModel.Count - 2) * (0.4f * boxWidth);
            if (index == 1 && !isSecondBox)
            {
                secondBoxBet.transform.DOLocalMoveX(secondBoxBet.transform.localPosition.x + CARD_SPACING, 0.5f);
            }
            if (index == 2 && !isSecondBox)
            {
                secondBoxBet.transform.DOLocalMoveX(secondBoxBet.transform.localPosition.x + CARD_SPACING, 0.5f);
            }
        }
        if (rect != null)
        {
            Vector2 newSize = rect.sizeDelta;
            newSize.x = totalWidth;
            rect.DOSizeDelta(newSize, 0.5f).SetEase(Ease.OutQuad);
        }

    }

    public void SpreadCards()
    {
        float totalWidth = (listCardModel.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < listCardModel.Count; i++)
        {
            CardModel cardModel = listCardModel[i];
            Vector2 targetPos = new Vector2(startX + i * CARD_SPACING, cardContainer.localPosition.y);
            // cardModel.transform.localPosition = targetPos;
            cardModel.transform.SetSiblingIndex(i);
            // cardModel.transform.DOLocalMove(targetPos, 0.1f).OnComplete(UpdateContainerWidth);
            cardModel.transform.DOLocalMove(targetPos, 0.1f);
        }
    }

    public void EnlargeCards()
    {
        DOVirtual.DelayedCall(0.3f, () =>
        {
            if (!isEnlarging)
            {
                isEnlarging = true;
                foreach (Transform transform in cardContainer)
                {
                    CardModel cardModel = transform.GetComponent<CardModel>();
                    cardModel.transform.DOScale(Vector3.one * 0.7f, 0.25f);
                    cardModel.transform.DOLocalMoveY(transform.localPosition.y + 50f, 0.25f);
                }
            }

        });
    }

    public void ResetCards()
    {
        DOVirtual.DelayedCall(0.3f, () =>
        {
            if (isEnlarging)
            {
                foreach (Transform transform in cardContainer)
                {
                    CardModel cardModel = transform.GetComponent<CardModel>();
                    cardModel.transform.DOScale(Vector3.one * 0.5f, 0.25f);
                    // cardModel.transform.DOLocalMoveY(transform.localPosition.y - 30f, 0.25f);
                }
            }
        });
    }

    public void AnimateHighlightCards()
    {
        listCardModel[0].SetBorder(true);
        for (int i = 0; i < listCardModel.Count; i++)
        {
            CardModel cardModel = listCardModel[i];
            Tween t = cardModel.transform.DOScale(new Vector2(0.7f, 0.7f), 0.5f)
                .SetLoops(-1, LoopType.Yoyo)     // lặp vô hạn, qua lại
                .SetEase(Ease.InOutSine)         // smooth
                .SetDelay(0.5f); 
            highlightTweens.Add(t);
        }
    }

    public void StopHighlightCards()
    {
        foreach (var t in highlightTweens)
        {
            if (t != null && t.IsActive()) t.Kill(); // dừng tween
        }
        highlightTweens.Clear();

        // Optionally reset scale về mặc định
        foreach (var card in listCardModel)
        {
            card.transform.localScale = new Vector2(0.5f, 0.5f);
            card.SetBorder(false);
        }
    }

    public void ShowAnimationWaiting()
    {
        effectContainer.gameObject.SetActive(true);
        animationWaiting.gameObject.SetActive(true);
    }

    public void HideAnimationWaiting()
    {
        effectContainer.gameObject.SetActive(false);
        animationWaiting.gameObject.SetActive(false);
    }

    public void ShowAnimationBlackjack()
    {
        effectContainer.gameObject.SetActive(true);
        animationBlackjack.gameObject.SetActive(true);
    }

    public void SplitBoxBet(BlackjackHand firstHand = null, BlackjackHand secondHand = null)
    {
        // Lấy component CanvasGroup để fade (nếu là UI)
        secondBoxBet.gameObject.SetActive(true);
        if (!secondBoxBet.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = secondBoxBet.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

        }
        CardModel secondCard = listCardModel[1];
        Sequence seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(1f, 0.5f));

        secondCard.transform.SetParent(secondBoxBet.GetCardPosition);
        switch (index)
        {
            case 0:
                // seq.Append(transform.DOLocalMoveY(BoxPosition.y + 10f, 0.25f))
                // .AppendCallback(() =>
                // {
                //     transform.DOLocalMoveX(BoxPosition.x - boxWidth, 0.25f);
                //     secondBoxBet.transform.DOLocalMoveX(2 * (BoxPosition.x + boxWidth), 0.25f);
                //     secondCard.transform.DOLocalMoveX(secondCard.transform.localPosition.x - CARD_SPACING, 0.25f);
                // });
                seq.Append(transform.DOLocalMoveY(transform.localPosition.y + 40f, 0.25f))
                    .Join(transform.DOLocalMoveX(BoxPosition.x - boxWidth, 0.25f))
                    .Join(secondBoxBet.transform.DOLocalMoveX(2 * (BoxPosition.x + boxWidth), 0.25f))
                    .Join(secondCard.transform.DOLocalMoveX(secondCard.transform.localPosition.x - CARD_SPACING, 0.25f));
                break;
            case 1:
                seq.JoinCallback(() =>
                {
                    secondBoxBet.transform.DOLocalMoveX(secondBoxBet.transform.localPosition.x + boxWidth + CARD_SPACING, 0.5f);
                    secondCard.transform.DOLocalMoveX(secondCard.transform.localPosition.x - CARD_SPACING, 0.5f);
                });
                break;
            case 2:
                seq.JoinCallback(() =>
                {
                    transform.DOLocalMoveX(transform.localPosition.x - 1.4f * boxWidth, 0.5f);
                    secondBoxBet.transform.DOLocalMoveX(BoxPosition.x + boxWidth + CARD_SPACING, 0.5f);
                    secondCard.transform.DOLocalMoveX(secondCard.transform.localPosition.x - CARD_SPACING, 0.5f);
                });
                break;
        }

        SetupSecondBox(secondHand);
        listCardModel.Remove(secondCard);

        if (firstHand == null) return;

        ShowScore(firstHand.Point, firstHand.MinPoint, firstHand.MaxPoint);
    }

    public void DoubleBoxBet()
    {
        totalBet *= 2;
        textTotalBet.text = Utility.FormatMoney(totalBet, true);
    }

    private void SetupSecondBox(BlackjackHand secondHand)
    {
        secondBoxBet.ResetSecondBox();
        CardModel secondCard = listCardModel[1];
        // CardModel cardModel = gameView.InitCard();
        // cardModel.SetData(secondCard.GetRank(), secondCard.GetSuit());
        // cardModel.transform.SetParent(secondBoxBet.GetCardPosition);
        // cardModel.transform.localPosition = Vector3.zero;
        // cardModel.transform.localScale = Vector3.one * 0.5f;
        // cardModel.gameObject.SetActive(true);
        secondBoxBet.listCardModel.Add(secondCard);
        secondBoxBet.SpreadCards();

        if (secondHand == null) return;
        secondBoxBet.ShowScore(secondHand.Point, secondHand.MinPoint, secondHand.MaxPoint);
    }

    public void HideImageChip()
    {
        imageChip.gameObject.SetActive(false);
    }

    public void Reset()
    {
        listCardModel.Clear();
        imageState.gameObject.SetActive(false);
        imageScoreBox.gameObject.SetActive(false);
        imageChip.gameObject.SetActive(false);
        iconChip.gameObject.SetActive(false);
        if (animationWaiting != null)
        {
            animationWaiting.gameObject.SetActive(false);
        }
        animationBlackjack.gameObject.SetActive(false);
        animationBust.gameObject.SetActive(false);
        animationWow.gameObject.SetActive(false);
        textTotalBet.gameObject.SetActive(false);
        textScore.gameObject.SetActive(false);
        textChipValue.gameObject.SetActive(false);
        effectContainer.gameObject.SetActive(false);

        RectTransform rect = gameObject.transform as RectTransform;
        Vector2 size = rect.sizeDelta;
        size.x = boxWidth;
        rect.sizeDelta = size;
        if (secondBoxBet != null)
        {
            secondBoxBet.gameObject.SetActive(false);
            secondBoxBet.transform.position = transform.position;
            secondBoxBet.transform.DOLocalMoveX(BoxPosition.x, 0.5f);
            transform.localPosition = initialPosition;
        }

        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        textChipValue.text = "";
        HasBet = false; 

    }

    public void ResetSecondBox()
    {
        imageState.gameObject.SetActive(false);
        animationWaiting.gameObject.SetActive(false);
        animationBlackjack.gameObject.SetActive(false);
        animationBust.gameObject.SetActive(false);
        animationWow.gameObject.SetActive(false);
        textTotalBet.gameObject.SetActive(false);
        effectContainer.gameObject.SetActive(false);
        imageChip.gameObject.SetActive(false);
        textChipValue.gameObject.SetActive(false);

    }

}
