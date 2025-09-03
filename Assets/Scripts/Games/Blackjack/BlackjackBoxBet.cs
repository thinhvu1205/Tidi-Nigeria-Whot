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
    [SerializeField] private Transform effectContainer;

    [Header(" Spine Animations ")]
    [SerializeField] private SkeletonGraphic animationWaiting;
    [SerializeField] private SkeletonGraphic animationBlackjack;
    [SerializeField] private SkeletonGraphic animationBust;
    [SerializeField] private SkeletonGraphic animationWow;
    [SerializeField] private BlackjackBoxBet secondBoxBet;
    public Transform GetCardPosition => cardContainer;
    public BlackjackBoxBet SecondBoxBet => secondBoxBet;
    private BlackjackView gameView;
    public bool isSecondBox = false;
    private int minScore = 0;
    [HideInInspector] public List<CardModel> listCardModel = new();
    private const float CARD_SPACING = 35f;
    private readonly List<Tween> highlightTweens = new List<Tween>();
    private float boxWidth;
    public Vector2 BoxPosition { get; private set; }

    private void Awake()
    {
        BoxPosition = transform.position;
        boxWidth = GetComponent<RectTransform>().rect.width;
        if (!isSecondBox)
            Reset();
    }
    public void SetInfo(BlackjackView blackjackView)
    {
        gameView = blackjackView;
    }

    public void ShowScore(int point, int minPoint = 0, int maxPoint = 0, BlackjackHandType type = BlackjackHandType.Normal)
    {
        // ----- Hiện score text với scale animation -----
        textScore.gameObject.SetActive(true);
        minScore = minPoint;
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
        }
        else
        {
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
        if (index < 0)
        {
            imageChip.gameObject.SetActive(false);
            textTotalBet.text = Utility.FormatMoney(totalValue, true);
            iconChip.gameObject.SetActive(false);
            textTotalBet.gameObject.SetActive(false);
            return;
        }
        imageChip.gameObject.SetActive(true);
        iconChip.gameObject.SetActive(true);
        textTotalBet.gameObject.SetActive(true);
        textChipValue.gameObject.SetActive(true);
        textTotalBet.text = Utility.FormatMoney(totalValue, true);

        textChipValue.text = Utility.FormatMoney(value, true);

        imageChip.sprite = listImageChip[index];

        animationWaiting.gameObject.SetActive(isWaiting);

        if (secondBoxBet != null)
            secondBoxBet.SetBetValue(index, value, totalValue, isWaiting);
    }

    public Vector2 GetNewCardPosition()
    {
        float totalWidth = (listCardModel.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = listCardModel.Count;
        Vector2 targetPos = new(startX + newIndex * CARD_SPACING, cardContainer.localPosition.y);

        return transform.TransformPoint(targetPos);;
    }

    public void UpdateContainerWidth()
    {
        RectTransform rect = gameObject.transform as RectTransform;
        float totalWidth = boxWidth;
        if (listCardModel.Count > 2)
        {
            totalWidth += (listCardModel.Count - 2) * (0.4f * boxWidth);
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
            cardModel.transform.DOLocalMove(targetPos, 0.1f).OnComplete(UpdateContainerWidth);
        }
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

    public void ShowAnimationBlackjack()
    {
        effectContainer.gameObject.SetActive(true);
        animationBlackjack.gameObject.SetActive(true);
    }

    public void SplitBoxBet(int playerIndex)
    {
        // Lấy component CanvasGroup để fade (nếu là UI)
        secondBoxBet.gameObject.SetActive(true);
        if (!secondBoxBet.TryGetComponent<CanvasGroup>(out var canvasGroup))
            canvasGroup = secondBoxBet.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        CardModel secondCard = listCardModel[1];
        Sequence seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(1f, 0.5f));

        switch (playerIndex)
        {
            case 0:
                seq.JoinCallback(() =>
                {
                    transform.DOLocalMoveX(BoxPosition.x - 40f - boxWidth, 0.5f);
                    secondBoxBet.transform.DOLocalMoveX(2 * (BoxPosition.x + 40f + boxWidth), 0.5f);
                    secondCard.transform.DOLocalMoveX(2 * (BoxPosition.x + 40f + boxWidth - CARD_SPACING), 0.5f);
                });
                break;
            case 1:
                seq.Join(secondBoxBet.transform.DOLocalMoveX(BoxPosition.x + 40f + boxWidth, 0.5f));
                seq.Join(secondCard.transform.DOLocalMoveX(BoxPosition.x + 40f + boxWidth - CARD_SPACING, 0.5f));
                break;
            case 2:
                seq.Join(secondBoxBet.transform.DOLocalMoveX(BoxPosition.x - 40f - boxWidth, 0.5f));
                seq.Join(secondCard.transform.DOLocalMoveX(BoxPosition.x - 40f - boxWidth + CARD_SPACING, 0.5f));
                break;
        }
        SetupSecondBox();
        ShowScore(minScore, minScore, minScore);
    }

    private void SetupSecondBox()
    {
        secondBoxBet.ResetSecondBox();
        CardModel secondCard = listCardModel[1];
        CardModel cardModel = gameView.InitCard();
        cardModel.SetData(secondCard.GetRank(), secondCard.GetSuit());
        cardModel.transform.SetParent(secondBoxBet.GetCardPosition);
        cardModel.transform.localPosition = Vector3.zero;
        cardModel.transform.localScale = Vector3.one * 0.5f;
        cardModel.gameObject.SetActive(true);
        secondBoxBet.listCardModel.Add(cardModel);
        secondBoxBet.SpreadCards();
        

        secondBoxBet.ShowScore(minScore, minScore, minScore);
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
        
        // transform.po = boxPosition;
        if (secondBoxBet != null)
        {
            secondBoxBet.gameObject.SetActive(false);
            secondBoxBet.transform.position = transform.position;
            secondBoxBet.transform.DOLocalMoveX(BoxPosition.x, 0.5f);
        }

        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        textChipValue.text = "";      

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
