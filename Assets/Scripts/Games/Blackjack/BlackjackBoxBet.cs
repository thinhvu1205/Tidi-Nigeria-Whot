using System;
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
using Color = UnityEngine.Color;

public class BlackjackBoxBet : MonoBehaviour
{
    [Header(" Texts ")]
    [SerializeField] private TextMeshProUGUI textChipValue;
    [SerializeField] private TextMeshProUGUI textChipWinValue;
    [SerializeField] private TextMeshProUGUI textTotalBet;
    [SerializeField] private TextMeshProUGUI textScore;
    [SerializeField] private TextMeshProUGUI textChipWinLose;
    [SerializeField] TMP_FontAsset fontWin, fontLose;

    [Header(" Images ")]
    [SerializeField] private Image iconChip;
    [SerializeField] private Image imageChip;
    [SerializeField] private Image imageState;
    [SerializeField] private Image imageScoreBox;
    [SerializeField] private Image imageAction;
    [SerializeField] private Image imageChipWin;

    [Header(" List Sprite ")]
    [SerializeField] private Sprite[] listImageChip;
    [SerializeField] private Sprite[] listImageState; // 0: Blackjack, 1: Bust, 2: Double, 3: Hit, 4: Push, 5: Split, 6: Stand
    [SerializeField] private Sprite[] listImageScoreBox; // 0: Normal, 1: Bust, 2: Blackjack, 3: Blackjack Pro
    [SerializeField] private Sprite[] listImageAction; // 0: Hit, 1: Stand, 2: Double, 3: Split

    [Header(" Transforms ")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform chipContainer;
    [SerializeField] private Transform effectContainer;

    [Header(" Spine Animations ")]
    [SerializeField] private SkeletonGraphic animationWaiting;
    [SerializeField] private SkeletonGraphic animationBlackjack;
    [SerializeField] private SkeletonGraphic animationBust;
    [SerializeField] private SkeletonGraphic animationWow;
    [SerializeField] private SkeletonGraphic animationWin;
    [SerializeField] private SkeletonGraphic animationLose;
    [SerializeField] private SkeletonGraphic animationPush;
    [SerializeField] private BlackjackBoxBet secondBoxBet;
    public Transform ChipPosition => imageChip.transform;
    public Transform ChipWinPosition => imageChipWin.transform;
    public Transform GetCardPosition => cardContainer;
    public BlackjackBoxBet SecondBoxBet => secondBoxBet;
    public bool isSecondBox = false;
    public bool isBankerBox = false;
    [HideInInspector] public List<CardModel> listCardModel = new();
    private const float CARD_SPACING = 35f;
    private readonly List<Tween> highlightTweens = new List<Tween>();
    private float boxWidth;
    private int minPoint, maxPoint;
    [SerializeField] private int index, chipIndex;
    public bool IsEnlarging { get; private set; } = false;
    public long TotalBet { get; private set; }
    public Vector2 BoxPosition { get; private set; }
    public bool HasBet { get; set; } = false;
    private Vector2 ImageScoreBoxPosition;
    private Vector2 initialPosition;
    private Sequence seqTextFly;

    private void Awake()
    {
        BoxPosition = transform.localPosition;
        ImageScoreBoxPosition = imageScoreBox.transform.localPosition;
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
        float offsetY = IsEnlarging && !isBankerBox ? 60f : 0f;
        Vector2 targetPos = new(startX + newIndex * CARD_SPACING, ImageScoreBoxPosition.y + offsetY);
        imageScoreBox.transform.localPosition = targetPos;
        imageScoreBox.gameObject.SetActive(true);
        imageScoreBox.transform.localScale = Vector3.zero; // bắt đầu nhỏ
        imageScoreBox.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        if (minPoint != maxPoint && point != 21)
        {
            textScore.text = $"{minPoint}/{maxPoint}";
            this.minPoint = minPoint;
            this.maxPoint = maxPoint;
        }
        else
        {
            this.maxPoint = point;
            this.minPoint = point;
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
                    ShrinkCurrentPlayerBoxbet();
                  
                };
                break;

            case BlackjackHandType._21P:
                animationWow.gameObject.SetActive(true);
                animationWow.AnimationState.SetAnimation(0, "animation", false);
                foreach (var card in listCardModel)
                {
                    card.ShowSparkleAnimation();
                }

                animationWow.AnimationState.Complete += delegate
                {
                    animationWow.gameObject.SetActive(false);
                    ShrinkCurrentPlayerBoxbet();
                };

                break;
            case BlackjackHandType.Busted:
                animationBust.gameObject.SetActive(true);
                animationBust.AnimationState.SetAnimation(0, "animation", false);
                foreach (var card in listCardModel)
                {
                    card.SetDark(true);
                }
                imageState.sprite = listImageState[1]; // Bust
                imageState.gameObject.SetActive(true);

                animationBust.AnimationState.Complete += delegate
                {
                    animationBust.gameObject.SetActive(false);
                    HideImageChip();
                    ShrinkCurrentPlayerBoxbet();
                };
                break;

            default:
                break;
        }
    }

    public void ShowHigherScore()
    {
        if (minPoint != 0 && minPoint != maxPoint)
        {
            ShowScore(maxPoint);
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

    public void ShowResult(int winType)
    {
        effectContainer.gameObject.SetActive(true);
        imageState.gameObject.SetActive(false);
        Debug.Log("WIN TYPE: " + winType);
        if (winType > 0)
        {
            Utility.PlayAnimation(animationWin, "animation", false);
            animationWin.AnimationState.Complete += delegate
            {
                effectContainer.gameObject.SetActive(false);
                animationWin.gameObject.SetActive(false);
            };
        }
        else if (winType == 0)
        {

            Utility.PlayAnimation(animationPush, "animation", false);
            animationPush.AnimationState.Complete += delegate
            {
                effectContainer.gameObject.SetActive(false);
                animationPush.gameObject.SetActive(false);
            };
        }
        else
        {
            Utility.PlayAnimation(animationLose, "lose", false);
            animationLose.AnimationState.Complete += delegate
            {
                animationLose.gameObject.SetActive(false);
                effectContainer.gameObject.SetActive(false);
                HideImageChip();
            };
        }
    }

    public void SetBetValue(int index, long value, long totalValue, bool isWaiting = false)
    {
        TotalBet = totalValue;
        chipIndex = index;
        if (index < 0 || totalValue <= 0)
        {
            imageChip.gameObject.SetActive(false);
            textTotalBet.text = Utility.FormatMoney(totalValue, true);
            iconChip.gameObject.SetActive(false);
            textTotalBet.gameObject.SetActive(false);
            return;
        }
        // if (!isSecondBox)
        // {
        //     imageChip.gameObject.SetActive(true);
        //     textChipValue.text = Utility.FormatMoney(value, true);
        //     imageChip.sprite = listImageChip[index];
        // }
            imageChip.sprite = listImageChip[index];
            imageChip.gameObject.SetActive(true);
            textChipValue.gameObject.SetActive(true);
            textChipValue.text = Utility.FormatMoney(value, true);
        iconChip.gameObject.SetActive(true);
        textTotalBet.gameObject.SetActive(true);
        textTotalBet.text = Utility.FormatMoney(totalValue, true);
        textChipValue.text = Utility.FormatMoney(value, true);
        animationWaiting.gameObject.SetActive(isWaiting);

        if (secondBoxBet != null && !secondBoxBet.gameObject.activeInHierarchy)
            secondBoxBet.SetBetValue(index, value, totalValue, isWaiting);
    }

    public void SetWinChipVisual(int index, long value)
    {
        Debug.Log("CHIP WIN VALUE: " + value);
        imageChipWin.gameObject.SetActive(true);
        imageChipWin.sprite = listImageChip[index];
        textChipWinValue.gameObject.SetActive(true);
        textChipWinValue.text = Utility.FormatMoney(value, true);
    }

    public void MoveWinChipToPlayer(Vector2 playerPosition, Action callback = null)
    {
        if (imageChipWin.gameObject.activeSelf)
        {
            imageChipWin.transform
                .DOMove(playerPosition, 0.5f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    // Vector2 randomPosition = new Vector2(
                    //     targetPosition.x + UnityEngine.Random.Range(-30, 30),
                    //     targetPosition.y + UnityEngine.Random.Range(-8, 8)
                    // );

                    // chip.transform.DOLocalJump(randomPosition, 20, 1, 0.2f);
                    imageChipWin.gameObject.SetActive(false);
                    callback?.Invoke();
                });
        }
    }

    public void AnimateFlyMoney(long mo, int fonzSize = 50)
    {
        if (mo == 0) return;

        textChipWinLose.fontSize = fonzSize;
        if (mo < 0)
        {
            textChipWinLose.font = fontLose;
            textChipWinLose.text = Utility.FormatMoney2(mo, true, true);
        }
        else
        {
            textChipWinLose.font = fontWin;
            textChipWinLose.text = "+" + Utility.FormatMoney2(mo, true, true);
        }

        textChipWinLose.transform.localPosition = Vector2.zero;
        int height = 140;

        textChipWinLose.gameObject.SetActive(true);
        if (seqTextFly != null)
        {
            seqTextFly.Kill();
        }
        seqTextFly = DOTween.Sequence()
             .Append(textChipWinLose.transform.DOLocalMove(new Vector2(0, height), 2.0f).SetEase(Ease.OutBack))
             .AppendInterval(1.0f)
             .AppendCallback(() =>
             {
                 textChipWinLose.gameObject.SetActive(false);
             });
    }

    public Vector2 GetNewCardPosition()
    {
        float totalWidth = (listCardModel.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = listCardModel.Count;
        float offsetY = IsEnlarging && !isBankerBox ? 30f : 0f;
        Vector2 targetPos = new(startX + newIndex * CARD_SPACING, cardContainer.localPosition.y  + offsetY);

        return transform.TransformPoint(targetPos); 
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
        float offsetY = IsEnlarging && !isBankerBox ? 30f : 0f;

        for (int i = 0; i < listCardModel.Count; i++)
        {
            CardModel cardModel = listCardModel[i];
            Vector2 targetPos = new Vector2(startX + i * CARD_SPACING, cardContainer.localPosition.y + offsetY);
            // cardModel.transform.localPosition = targetPos;
            cardModel.transform.SetSiblingIndex(i);
            // cardModel.transform.DOLocalMove(targetPos, 0.1f).OnComplete(UpdateContainerWidth);
            cardModel.transform.DOLocalMove(targetPos, 0.1f);
        }

        float scoreBoxoffsetX = 0f;
        float scoreBoxoffsetY = 0f;
        if (IsEnlarging)
        {
            scoreBoxoffsetX = isBankerBox ? 30f : 0f;
            scoreBoxoffsetY = isBankerBox ? 100f : 60f;
        }
        Vector2 scoreBoxTargetPos = new(ImageScoreBoxPosition.x + scoreBoxoffsetX, ImageScoreBoxPosition.y + scoreBoxoffsetY);
        imageScoreBox.transform.localPosition = scoreBoxTargetPos;
    }

    public void EnlargeCards(float scale = 1.3f)
    {
        DOVirtual.DelayedCall(0.3f, () =>
        {
            if (!IsEnlarging)
            {
                IsEnlarging = true;
                cardContainer.transform.DOScale(Vector3.one * scale, 0.25f);
                SpreadCards();
                // foreach (Transform transform in cardContainer)
                // {
                //     CardModel cardModel = transform.GetComponent<CardModel>();
                //     cardModel.transform.DOScale(Vector3.one * 0.7f, 0.25f);
                //     cardModel.transform.DOLocalMoveY(transform.localPosition.y + 50f, 0.25f);
                // }
            }

        });
    }

    public void ShrinkCards()
    {
        DOVirtual.DelayedCall(0.3f, () =>
        {
            if (IsEnlarging)
            {
                IsEnlarging = false;
                cardContainer.transform.DOScale(Vector3.one * 1, 0.25f);
                SpreadCards();
                // foreach (Transform transform in cardContainer)
                // {
                //     CardModel cardModel = transform.GetComponent<CardModel>();
                //     cardModel.transform.DOScale(Vector3.one * 0.5f, 0.25f);
                //     cardModel.transform.DOLocalMoveY(transform.localPosition.y - 30f, 0.25f);
                // }
            }
        });
    }

    private void ShrinkCurrentPlayerBoxbet()
    {
        if (IsEnlarging)
        {
            ShrinkCards();
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

    public void AnimateImageAction(BlackjackActionCode action)
    {
        imageAction.transform.DOKill();
        Vector2 initialPos = imageAction.transform.localPosition;
        imageAction.transform.localScale = Vector3.one * 1.1f;
        imageAction.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 20));
        if (action == BlackjackActionCode.BlackjackActionHit)
        {
            imageAction.transform.localScale = Vector3.one * 0.85f;
        }
        int actionIndex = action switch
        {
            BlackjackActionCode.BlackjackActionHit => 0,
            BlackjackActionCode.BlackjackActionStay => 1,
            BlackjackActionCode.BlackjackActionDouble => 2,
            BlackjackActionCode.BlackjackActionSplit => 3,
            _ => -1
        };
        if (actionIndex == -1) return;
        imageAction.sprite = listImageAction[actionIndex];
        imageAction.gameObject.SetActive(true);
        // Sequence s = DOTween.Sequence();
        //     s.Append(imageAction.transform.DOLocalRotate(new Vector3(0, 0, 30f), 0.35f)
        //             .SetEase(Ease.InOutSine))
        //     .Append(imageAction.transform.DOLocalRotate(new Vector3(0, 0, -30f), 0.35f)
        //             .SetEase(Ease.InOutSine))
        //     .SetLoops(-1);
        // imageAction.transform
        //     .DOBlendableRotateBy(new Vector3(0, 0, 30f), 0.35f)
        //     .SetEase(Ease.InOutSine)
        //     .SetLoops(-1, LoopType.Yoyo);   
        imageAction.transform.DORotate(new Vector3(0, 0, -20f), 0.4f)
                .SetLoops(-1, LoopType.Yoyo)     // lặp vô hạn, qua lại
                .SetEase(Ease.InOutSine);      // smooth
                // .SetDelay(0.5f); 
        imageAction.transform.DOMoveY(transform.position.y + 170f, 1f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                imageAction.transform.localPosition = initialPos;
                imageAction.gameObject.SetActive(false);
            });
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
                seq.Append(transform.DOLocalMoveY(transform.localPosition.y + 70f, 0.25f))
                    .Join(transform.DOLocalMoveX(BoxPosition.x - boxWidth, 0.25f))
                    .Join(secondBoxBet.transform.DOLocalMoveX(2 * (BoxPosition.x + boxWidth), 0.25f))
                    .Join(secondCard.transform.DOLocalMoveX(secondCard.transform.localPosition.x - CARD_SPACING, 0.25f));
                break;
            case 1:
                seq.JoinCallback(() =>
                {
                    secondBoxBet.transform.DOLocalMoveX(secondBoxBet.transform.localPosition.x + boxWidth * 1.5f + CARD_SPACING, 0.5f);
                    secondCard.transform.DOLocalMoveX(secondCard.transform.localPosition.x - CARD_SPACING, 0.5f);
                });
                break;
            case 2:
                seq.JoinCallback(() =>
                {
                    transform.DOLocalMoveX(transform.localPosition.x - 1.4f * boxWidth, 0.5f);
                    secondBoxBet.transform.DOLocalMoveX(BoxPosition.x + boxWidth * 1.5f + CARD_SPACING, 0.5f);
                    secondCard.transform.DOLocalMoveX(secondCard.transform.localPosition.x - CARD_SPACING, 0.5f);
                });
                break;
        }

        SetupSecondBox(secondHand);
        listCardModel.Remove(secondCard);

        if (firstHand == null) return;

        ShowScore(firstHand.Point, firstHand.MinPoint, firstHand.MaxPoint);
    }

    public void ShowSecondBox()
    {
        Debug.Log("SHOW SECOND BOX");
        secondBoxBet.gameObject.SetActive(true);
        switch (index)
        {
            case 0:
                transform.localPosition = new(initialPosition.x - boxWidth, initialPosition.y + 70f);
                secondBoxBet.GetComponent<RectTransform>().anchoredPosition = new(2 * (BoxPosition.x + boxWidth), secondBoxBet.transform.localPosition.y);
                break;
            case 1:
                secondBoxBet.GetComponent<RectTransform>().anchoredPosition= new(BoxPosition.x + boxWidth * 1.5f + CARD_SPACING, secondBoxBet.transform.localPosition.y);
                break;
            case 2:
                transform.localPosition = new(initialPosition.x - 1.4f * boxWidth, initialPosition.y);
                secondBoxBet.GetComponent<RectTransform>().anchoredPosition= new(BoxPosition.x + boxWidth * 1.5f + CARD_SPACING, secondBoxBet.transform.localPosition.y);
                break;
        }
    }

    public void DoubleBoxBet()
    {
        TotalBet *= 2;
        // textTotalBet.text = Utility.FormatMoney(TotalBet, true);
        // textChipValue.text = Utility.FormatMoney(TotalBet, true);
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
        secondBoxBet.listCardModel.Clear();
        secondBoxBet.listCardModel.Add(secondCard);
        secondBoxBet.SpreadCards();

        if (secondHand == null) return;
        secondBoxBet.imageChip.gameObject.SetActive(true);
        secondBoxBet.textChipValue.gameObject.SetActive(true);
        secondBoxBet.SetBetValue(chipIndex, TotalBet, TotalBet);
        secondBoxBet.ShowScore(secondHand.Point, secondHand.MinPoint, secondHand.MaxPoint);
    }

    public void HideImageChip()
    {
        imageChip.gameObject.SetActive(false);
    }

    public void HideImageChipWin()
    {
        imageChipWin.gameObject.SetActive(false);
    }

    public bool IsSplittableBox()
    {
        if (listCardModel.Count < 2) return false;
        CardModel card1 = listCardModel[0];
        CardModel card2 = listCardModel[1];

        int rank1 = card1.GetRank();
        int rank2 = card2.GetRank();

        // Nếu hai lá cùng rank → có thể split
        if (rank1 == rank2)
            return true;

        // Nếu đều là 10 hoặc là 10 / J / Q / K → cho phép split
        bool isTenLike1 = rank1 == 10 || rank1 == 11 || rank1 == 12 || rank1 == 13;
        bool isTenLike2 = rank2 == 10 || rank2 == 11 || rank2 == 12 || rank2 == 13;

        if (isTenLike1 && isTenLike2)
            return true;

        return false;
    }

    public void Reset(bool isRejoin = false)
    {
        TotalBet = minPoint = maxPoint = 0;
        listCardModel.Clear();
        imageState.gameObject.SetActive(false);
        imageScoreBox.gameObject.SetActive(false);
        imageChip.gameObject.SetActive(false);
        iconChip.gameObject.SetActive(false);

        if (!isBankerBox)
        {
            animationWaiting.gameObject.SetActive(false);
            imageChipWin.gameObject.SetActive(false);
            textChipWinValue.gameObject.SetActive(false);
            animationWin.gameObject.SetActive(false);
            animationLose.gameObject.SetActive(false);
            animationPush.gameObject.SetActive(false);
        }
        animationBlackjack.gameObject.SetActive(false);
        animationBust.gameObject.SetActive(false);
        animationWow.gameObject.SetActive(false);
        textTotalBet.gameObject.SetActive(false);
        textScore.gameObject.SetActive(false);
        textChipValue.gameObject.SetActive(false);
        effectContainer.gameObject.SetActive(false);

        if (!isRejoin)
        {
            if (secondBoxBet != null)
            {
                secondBoxBet.Reset();
                secondBoxBet.gameObject.SetActive(false);
                secondBoxBet.transform.position = transform.position;
                secondBoxBet.transform.DOLocalMoveX(BoxPosition.x, 0.5f);
            }
        }

        if (!isBankerBox)
        {
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
        // imageChip.gameObject.SetActive(false);
        textChipValue.gameObject.SetActive(false);

    }

}
