using System;
using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
public class BaseSlotView : BaseGameView
{
    protected enum WinType
    {
        BIG_WIN,
        MEGA_WIN,
        HUGE_WIN,
        FREE_SPIN,
        FIVE_OF_A_KIND,
        SCATTER,
        NONE
    }
    [SerializeField] protected Button maxBetButton, plusBetButton, minusBetButton;
    [SerializeField] protected TextMeshProUGUI betAmountText, betInfoSessionText, betStateText, stateWinText, freeSpinLeftText, bigWinText, chipWinText, currentChipText;
    [SerializeField] protected Image spinBackgroundImage, chipImage, spinButton;
    [SerializeField] protected List<Sprite> stateWinSpriteList, itemSpriteList;
    [SerializeField] protected Transform lineContainer, effectContainer, paylineInfoContainer, columnContainer, coinParent;
    [SerializeField] protected GameObject rulePrefab, linePrefab, coinPrefab, columnPrefab;
    [SerializeField] protected SkeletonGraphic backgroundFreeSpinAnimation, thirdScatterAnimation, buttonSpinAnimation, animationEffect;

    [Header("Constants")]
    protected readonly List<string> colorsList = new List<string>
    {
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE",
        "#920E48", "#F277E2", "#BC8B15", "#AC6456", "#E17512",
        "#E8C500", "#F0E915", "#FD93A1", "#C735D4", "#FF0C04",
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE",
        "#920E48", "#F277E2", "#BC8B15", "#AC6456", "#E17512",
        "#E8C500", "#F0E915", "#FD93A1", "#C735D4", "#FF0C04",
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE"
    };
    protected readonly Vector2 RECT_SIZE = new(200, 170);

    protected const float AUTO_SPIN_HOLD_DURATION = 1.3f;
    protected const string BIG_WIN_ANIMATION_PATH = "SlotSpine/Noel/big_megawinNoel/skeleton_SkeletonData";
    protected const string MEGA_WIN_ANIMATION_PATH = "SlotSpine/Noel/big_megawinNoel/skeleton_SkeletonData";
    protected const string HUGE_WIN_ANIMATION_PATH = "SlotSpine/";
    protected const string FIVE_OF_A_KIND_ANIMATION_PATH = "SlotSpine/FiveOfAKind/skeleton_SkeletonData";
    protected const string FREE_SPIN_ANIMATION_PATH = "SlotSpine/freespin/skeleton_SkeletonData";
    protected const string BIG_WIN_ANIMATION_NAME = "big";
    protected const string MEGA_WIN_ANIMATION_NAME = "mega";
    protected const string HUGE_WIN_ANIMATION_NAME = "hugethai";
    protected const string FIVE_OF_A_KIND_ANIMATION_NAME = "animation";
    protected const string FREE_SPIN_ANIMATION_NAME = "eng";
    [Header("Game State")]
    protected SpinType spinType = SpinType.NORMAL;
    protected SlotGameState gameState = SlotGameState.JOIN_GAME;
    protected WinType winType = WinType.NONE;

    [Header(" Object Pools ")]
    protected UnityEngine.Pool.ObjectPool<Image> coinPool;
    protected UnityEngine.Pool.ObjectPool<GameObject> linePool;

    [Header("Game Data")]
    protected List<SlotColumn> slotColumnList = new();
    protected List<int> winningLineIdList = new();
    protected List<GameObject> allLinesList = new();
    protected List<GameObject> lineOneByOneList = new();
    protected Queue<TweenCallback> tweenQueue = new();
    protected Sequence lineOneByOneSequence;
    protected long playerWallet;
    protected int totalLineWin;
    public int ScatterCount { get; set; } = 0;
    public bool IsSpinning { get; set; } = false;
    protected bool isHoldingSpin, isFreeSpin = false;
    protected float holdingSpinTime = 0;

    protected override void Awake()
    {
        base.Awake();
        Init();
        InitColumns();
        UpdateSpinButtonUI();
    }

    protected void Update()
    {
        HandleHoldingSpin();
    }

    #region Spin Actions
    protected void OnStartSpin()
    {
        Debug.Log("START SPIN!!!");
        UpdateGameState(SlotGameState.SPINNING);
        IsSpinning = true;
        foreach (SlotColumn column in slotColumnList)
        {
            column.StartSpin(spinType);
        }
    }

    public void OnStopSpin()
    {
        winningLineIdList.Add(1);
        winningLineIdList.Add(2);

        ///------------------CHECK SHOW WIN SCATTER--------------------//
        // if (CheckWinScatter())
        // {
        //     tweenQueue.Enqueue(() => ShowWinAnimation(WinType.SCATTER));
        // }

        ///------------------CHECK SHOW FIVE OF A KIND--------------------///
        if (CheckFiveOfAKind())
        {
            tweenQueue.Enqueue(() => ShowWinAnimation(WinType.FIVE_OF_A_KIND));
        }

        ///------------------CHECK SHOW FREESPIN--------------------//
        if (CheckGetFreeSpin())
        {
            // Nếu đang quay thường hoặc quay auto mà đc freespin -> dừng lại
            tweenQueue.Enqueue(() => ShowWinAnimation(WinType.FREE_SPIN));
        }
        
        ///------------------CHECK SHOW ALL LINE--------------------///
        if (winningLineIdList.Count > 0)
        {
            tweenQueue.Enqueue(() => ShowAllWinLines());
        }


        ///------------------CHECK SHOW TYPE WIN--------------------///
        if (!isFreeSpin)
        {
            switch (winType)
            {
                case WinType.BIG_WIN:
                    tweenQueue.Enqueue(() => ShowWinAnimation(WinType.BIG_WIN));
                    break;
                case WinType.MEGA_WIN:
                    tweenQueue.Enqueue(() => ShowWinAnimation(WinType.MEGA_WIN));
                    break;
                case WinType.HUGE_WIN:
                    tweenQueue.Enqueue(() => ShowWinAnimation(WinType.HUGE_WIN));
                    break;
            }
        }

        ///------------------CHECK SHOW ONE BY ONE--------------------//
        if (winningLineIdList.Count > 0)
        {
            if (spinType == SpinType.NORMAL)
            {
                // if (freespinLeft == 0) listActionHandleSpin.Add(acShowOneWinLine);
                tweenQueue.Enqueue(() => ShowWinLineOneByOne());
            }
            else if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
            {
                if (winningLineIdList.Count == 1) tweenQueue.Enqueue(() => ShowWinLineOneByOne());
                // if (!isInFreeSpin) listActionHandleSpin.Add(acShowAnimChipBay);
            }
        }

        NextTween();
    }

    public void OnColumnStop(int columnIndex)
    {
        if (slotColumnList[columnIndex].IsShowingThirdScatter)
        {
            slotColumnList[columnIndex].IsShowingThirdScatter = false;
            HideThirdScatterColumn();
        }
    }

    public void CheckThirdScatter(int columnIndex)
    {
        Debug.Log("SCATTER COUNT: " + ScatterCount);
        if (ScatterCount == 2)
        {
            foreach (SlotColumn column in slotColumnList)
            {
                if (column.IsSpinning)
                {
                    column.ExtraTime += 2f;
                }
            }
            slotColumnList[columnIndex + 1].IsShowingThirdScatter = true;
            ShowThirdScatterColumn(columnIndex + 1);
        }
    }
    #endregion

    #region Buttons
    // giữ nút spin
    public void OnTriggerDownSpinButton()
    {
        holdingSpinTime = 0f;
        isHoldingSpin = true;
    }

    // thả nút spin
    public void OnTriggerUpSpinButton()
    {
        isHoldingSpin = false;
        if (holdingSpinTime < AUTO_SPIN_HOLD_DURATION)
        {
            if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
            {
                switch (gameState)
                {
                    case SlotGameState.PREPARE:
                    case SlotGameState.JOIN_GAME:
                        InfoBet infoBet = new()
                        {
                            Id = 1,
                            Chips = 2,
                            NUserBet = 3
                        };
                        // DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
                        OnStartSpin();
                        break;
                    case SlotGameState.SHOWING_RESULT:
                        tweenQueue.Clear();
                        NextTween();
                        break;

                }
            }
            else
            {
                switch (gameState)
                {
                    case SlotGameState.SPINNING:
                    case SlotGameState.SHOWING_RESULT:
                        spinType = SpinType.NORMAL;
                        UpdateSpinButtonUI();
                        break;

                }
                tweenQueue.Clear();
                NextTween();
                // spinType = SpinType.NORMAL;
            }
        }

    }
    protected void HandleHoldingSpin()
    {
        // Ko Auto thì mới hold dc
        if (isHoldingSpin && spinType != SpinType.AUTO && spinType != SpinType.FREE_AUTO)
        {
            holdingSpinTime += Time.deltaTime;
            if (holdingSpinTime > AUTO_SPIN_HOLD_DURATION)
            {
                // if (agPlayer < totalListBetRoom[currentMarkBet])
                // {
                //     lbInfoSession.text = Config.getTextConfig("msg_warrning_send");
                //     return;
                // }
                if (spinType == SpinType.NORMAL)
                    spinType = SpinType.AUTO;
                else
                    spinType = SpinType.FREE_AUTO;
                OnStartSpin();
                UpdateSpinButtonUI();
                isHoldingSpin = false;
            }
        }
    }

    public void OnClickPlusBetButton()
    {

    }

    public void OnClickMinusBetButton()
    {

    }

    public void OnClickMaxBetButton()
    {

    }

    public void OnClickShopButton()
    {

    }

    public void OnClickMenuButton()
    {
        UIManager.Instance.OpenGroupMenu();
    }

    public override void OpenRule()
    {
        Instantiate(rulePrefab, transform);
    }

    #endregion


    #region Win Effects
    protected void ShowAllWinLines()
    {
        SetDarkAllItems();
        // Draw Line và lưu vào listLine

        foreach (int lineId in winningLineIdList)
        {
            // List<int> lineWinID = getPaylineWithID(lineId);
            List<int> lineWinID = new() { 0, 1, 0, 1, 0 };
            ColorUtility.TryParseHtmlString(colorsList[lineId % colorsList.Count], out Color colorLine);
            List<Vector2> listPosition = new();
            for (int j = 0; j < lineWinID.Count; j++)
            {
                Vector2 positionItem = slotColumnList[j].GetItemPositionAtIndex(lineWinID[j]);
                Vector2 positionInContainer = lineContainer.transform.InverseTransformPoint(positionItem);
                listPosition.Add(positionInContainer);
            }
            DrawLines(listPosition, colorLine);
        }

        // Show Line từ listLine
        int totalLines = allLinesList.Count;
        Sequence sequence = DOTween.Sequence().SetAutoKill(true); ;

        // Hiện line lên, mỗi line cách nhau 0.1s
        for (int i = 0; i < totalLines; i++)
        {
            GameObject line = allLinesList[i];
            sequence
                .AppendCallback(() => line.SetActive(true))
                .AppendInterval(0.1f);
        }

        // Sau 1.5s thì ẩn hết line đi và show tween tiếp theo
        sequence
            .AppendInterval(1.5f)
            .OnComplete(() =>
            {
                foreach (GameObject line in allLinesList)
                {
                    linePool.Release(line);
                }
                SetLightAllItems();
                NextTween();
            });
    }

    protected void ShowWinLineOneByOne()
    {
        AnimateCoinsFly();
        UpdateGameState(SlotGameState.SHOWING_RESULT);
        for (int i = 0; i < winningLineIdList.Count; i++)
        {

            int index = i;
            int lineId = winningLineIdList[i];
            // List<int> lineWinID = getPaylineWithID(lineId);
            List<int> lineWinID = new() { 0, 2, 1, 2, 0 };
            ColorUtility.TryParseHtmlString(colorsList[lineId % colorsList.Count], out Color colorLine);

            lineOneByOneSequence = DOTween.Sequence();
            // sequence.Add(s);
            lineOneByOneSequence
                .AppendInterval(2.0f * i)
                .AppendCallback(() =>
                {
                    if (transform == null)
                    {
                        DOTween.Kill(lineOneByOneSequence);
                    }
                    else
                    {
                        // playSound(SOUND_SLOT.SHOW_LINE);
                        SetDarkAllItems();
                        DrawRectangularAndConnectingLines(lineWinID, colorLine);
                        // showPaylinesInfo(lineWinID, index);
                    }

                })
                .AppendInterval(2.0f)
                .AppendCallback(() =>
                {
                    foreach (GameObject line in lineOneByOneList)
                    {
                        linePool.Release(line);
                    }
                    lineOneByOneList.Clear();
                })
                .AppendCallback(() =>
                {
                    if (index == winningLineIdList.Count - 1)
                    {
                        if (spinType == SpinType.NORMAL)
                        {
                            SetLightAllItems();
                            Reset();
                        }
                        NextTween();
                    }
                });
        }
    }

    protected void ShowWinScatter()
    {

    }

    private void ShowWinAnimation(WinType winType)
    {
        effectContainer.gameObject.SetActive(true);
        animationEffect.gameObject.SetActive(true);

        switch (winType)
        {
            case WinType.BIG_WIN:
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                Utility.TweenNumberTo(bigWinText, 100000, 0, 2.0f);
                animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, BIG_WIN_ANIMATION_PATH, BIG_WIN_ANIMATION_NAME, false);

                break;
            case WinType.MEGA_WIN:
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                Utility.TweenNumberTo(bigWinText, 100000, 0, 2.0f);
                animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, MEGA_WIN_ANIMATION_PATH, MEGA_WIN_ANIMATION_NAME, false);
                break;
            case WinType.HUGE_WIN:
                break;
            case WinType.FIVE_OF_A_KIND:
                animationEffect.transform.localScale = Vector2.one;
                animationEffect.transform.localPosition = Vector2.zero;
                bigWinText.transform.parent.gameObject.SetActive(false);
                Utility.PlayAnimationByPath(animationEffect, FIVE_OF_A_KIND_ANIMATION_PATH, FIVE_OF_A_KIND_ANIMATION_NAME, false);
                break;
            case WinType.FREE_SPIN:
                animationEffect.transform.localScale = Vector2.one;
                animationEffect.transform.localPosition = Vector2.zero;
                bigWinText.transform.parent.gameObject.SetActive(false);
                Utility.PlayAnimationByPath(animationEffect, FREE_SPIN_ANIMATION_PATH, FREE_SPIN_ANIMATION_NAME, false);
                break;

        }

        animationEffect.AnimationState.Complete += delegate
        {
            effectContainer.gameObject.SetActive(false);
            bigWinText.transform.parent.gameObject.SetActive(false);
            NextTween();
            effectContainer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        };
    }

    private void ShowThirdScatterColumn(int indexCol)
    {
        thirdScatterAnimation.gameObject.SetActive(true);
        thirdScatterAnimation.transform.localPosition = new Vector2(thirdScatterAnimation.transform.parent.InverseTransformPoint(slotColumnList[indexCol].transform.position).x, thirdScatterAnimation.transform.localPosition.y);
    }

    private void HideThirdScatterColumn()
    {
        thirdScatterAnimation.gameObject.SetActive(false);
    }

    private void HideWinAnimation()
    {

    }

    private bool CheckWinScatter()
    {
        return true;
    }

    private bool CheckFiveOfAKind()
    {
        return false;
    }

    private bool CheckGetFreeSpin()
    {

        return false;
    }
    #endregion

    #region Draw Lines
    private void DrawLines(List<Vector2> positionList, Color colorLine)
    {
        Vector2 startPosition = new(positionList[0].x - 90, positionList[0].y);
        Vector2 lastPosition = new(positionList[^1].x + 90, positionList[^1].y);
        positionList.Insert(0, startPosition);
        positionList.Add(lastPosition);
        GameObject line = linePool.Get();

        RectTransform rectTransform = line.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
        line.SetActive(false);
        allLinesList.Add(line);
        LineController lineController = line.GetComponent<LineController>();
        lineController.DrawLine(positionList, colorLine);
    }

    private void DrawLineBetween2Points(List<Vector2> listPos, Color colorLine)
    {
        GameObject line = linePool.Get();
        line.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        lineOneByOneList.Add(line);
        LineController lineController = line.GetComponent<LineController>();
        lineController.DrawLine(listPos, colorLine);
    }

    private void DrawSquare(Vector2 startPos, Color colorLine)
    {
        GameObject lineRect = linePool.Get();
        RectTransform rectTransform = lineRect.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
        LineController lineController = lineRect.GetComponent<LineController>();
        lineController.DrawRect(startPos, RECT_SIZE, colorLine);
        lineOneByOneList.Add(lineRect);
    }

    private void DrawRectangularAndConnectingLines(List<int> lineWinID, Color colorLine)
    {
        // lineWinID: { 0, 1, 0, 1, 0 }
        List<Vector2> itemPositions = new();
        List<int> itemIdsInLine = new();

        // Bước 1: Thu thập vị trí & id của các item trên line
        for (int colIndex = 0; colIndex < lineWinID.Count; colIndex++)
        {
            // Lấy vị trí item theo world position
            Vector2 worldPos = slotColumnList[colIndex].GetItemPositionAtIndex(lineWinID[colIndex]);

            // Chuyển về local position trong container
            Vector2 localPos = lineContainer.transform.InverseTransformPoint(worldPos);
            itemPositions.Add(localPos);

            // Lấy ID item từ view
            // int itemId = slotViews[colIndex][lineWinID[colIndex]];
            int itemId = 9;
            itemIdsInLine.Add(itemId);
        }

        // Bước 2: Đếm số lượng item trùng khớp liên tiếp
        int matchedItemCount = CountConsecutiveMatchingItems(itemIdsInLine);
        // Bước 3: Highlight các item thắng
        for (int colIndex = 0; colIndex < matchedItemCount; colIndex++)
        {
            int itemIndex = lineWinID[colIndex];
            slotColumnList[colIndex].SetLightItemAtIndex(itemIndex);
            slotColumnList[colIndex].SetAnimationForItemAtIndex(itemIndex);
        }

        // Bước 4: Vẽ line highlight
        List<Vector2> remainingLinePoints = new();

        for (int i = 0; i < itemPositions.Count; i++)
        {
            Vector2 currentPosition = itemPositions[i];

            // Vẽ hình vuông nếu còn trong số lượng item
            if (i < matchedItemCount)
            {
                DrawSquare(currentPosition, colorLine);
            }

            // Vẽ đường nối tiếp nếu cần
            if (i < itemPositions.Count - 1)
            {
                Vector2 nextPos = itemPositions[i + 1];
                if (Mathf.Abs(nextPos.y - currentPosition.y) > 200 && i < matchedItemCount - 1)
                {
                    List<Vector2> listPos = new();
                    Vector2 firstIntersectPos = GetIntersectPoint(itemPositions[i], itemPositions[i + 1]);
                    Vector2 nextIntersectPos = GetIntersectPoint(itemPositions[i + 1], itemPositions[i]);
                    listPos.Add(firstIntersectPos);
                    listPos.Add(nextIntersectPos);
                    DrawLineBetween2Points(listPos, colorLine);
                }
            }

            // Tính toán các điểm bắt đầu của line còn lại
            if (i >= matchedItemCount)
            {
                if (remainingLinePoints.Count == 0)
                {
                    Vector2 previousPos = itemPositions[i - 1];
                    Vector2 startPosLineRemain;
                    if (Mathf.Abs(previousPos.y - currentPosition.y) < 100)
                    {
                        startPosLineRemain = new Vector2(itemPositions[i - 1].x + RECT_SIZE.x / 2, itemPositions[i - 1].y);
                    }
                    else
                    {
                        bool isTwoItemSpace = Mathf.Abs(currentPosition.y - previousPos.y) > 200;
                        if (previousPos.y < currentPosition.y)
                        {
                            startPosLineRemain = isTwoItemSpace ? new Vector2(previousPos.x, previousPos.y + RECT_SIZE.y / 2) : new Vector2(previousPos.x + RECT_SIZE.x / 2, previousPos.y + RECT_SIZE.y / 2);
                        }
                        else
                        {
                            startPosLineRemain = isTwoItemSpace ? new Vector2(previousPos.x, previousPos.y - RECT_SIZE.y / 2) : new Vector2(previousPos.x + RECT_SIZE.x / 2, previousPos.y - RECT_SIZE.y / 2);
                        }

                    }
                    remainingLinePoints.Add(startPosLineRemain);

                }
                remainingLinePoints.Add(currentPosition);
            }
        }

        // Thêm đoạn line cuối từ icon cuối ra mép phải
        Vector2 lastPos = itemPositions[^1];
        Vector2 endRemainingLine = new Vector2(lastPos.x + RECT_SIZE.x / 2, lastPos.y);
        remainingLinePoints.Add(endRemainingLine);

        // Vẽ line còn lại
        DrawLineBetween2Points(remainingLinePoints, colorLine);

    }

    // Đếm số Item giống nhau liên tục trong 1 line
    private int CountConsecutiveMatchingItems(List<int> listIds)
    {
        if (listIds == null || listIds.Count == 0)
            return 0;

        int count = 1;
        int referenceId = listIds[0];

        for (int i = 1; i < listIds.Count; i++)
        {
            int currentId = listIds[i];

            // Nếu reference ko phải là Wild
            if (referenceId != 11)
            {
                if (currentId == referenceId || currentId == 11)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                referenceId = currentId;
                count++;
            }
        }

        return count;
    }

    protected Vector2 GetIntersectPoint(Vector2 vector1, Vector2 vector2)
    {

        int delta = vector1.y > vector2.y ? -1 : 1;
        Vector2 crossPoint = new(vector1.x - RECT_SIZE.x / 2, vector1.y + RECT_SIZE.y / 2 * delta);
        if (Mathf.Abs(vector1.y - vector2.y) < 1)
        {
            crossPoint = new Vector2(vector1.x + RECT_SIZE.x / 2, vector1.y + RECT_SIZE.y / 2);
        }
        return new(crossPoint.x + RECT_SIZE.x / 2, crossPoint.y);
    }
    #endregion

    #region UI
    protected virtual void UpdateSpinButtonUI()
    {
        if (spinType == SpinType.NORMAL)
        {
            switch (gameState)
            {
                case SlotGameState.PREPARE:
                case SlotGameState.JOIN_GAME:
                    break;
                case SlotGameState.SPINNING:
                    break;
                case SlotGameState.SHOWING_RESULT:

                    break;
            }
        }
        else
        {
            switch (gameState)
            {
                case SlotGameState.PREPARE:
                case SlotGameState.JOIN_GAME:

                    break;
                case SlotGameState.SPINNING:
                    break;
                case SlotGameState.SHOWING_RESULT:
                    break;
            }
        }
    }
    protected void SetInfoSessionText(string text)
    {
        betInfoSessionText.gameObject.SetActive(true);
        betInfoSessionText.text = text;
    }
    #endregion

    #region Effects
    protected void AnimateCoinsFly(int totalCoins = 5, float timeInterval = 0.1f)
    {

        for (int i = 0; i < totalCoins; i++)
        {
            DOTween.Sequence().AppendInterval(i * timeInterval).AppendCallback(() =>
            {
                Image coin = coinPool.Get();
                coin.gameObject.SetActive(true);
                coin.GetComponent<Animator>().Play("Idle");
                AnimateCoinFly(coin, chipWinText.transform, chipImage.transform);
            });
        }
    }
    protected void AnimateCoinFly(Image coin, Transform from, Transform to)
    {
        coin.transform.position = from.position;
        coin.transform.DOJump(to.position, 1, 1, 2).SetEase(Ease.InOutCubic);
        Color fadedColor = coin.color;
        fadedColor.a = .2f;
        coin.color = fadedColor;
        coin.DOFade(1, .75f);

        DOTween.Sequence()
            .AppendInterval(.5f)
            .Append(coin.transform.DOScale(2, 0.25f))
            .AppendInterval(0.15f)
            .Append(coin.transform.DOScale(0.85f, 0.25f))//0.85
            .AppendInterval(0.6f)
            .Append(coin.DOFade(0, .25f)).AppendCallback(() =>
            {
                coinPool.Release(coin);
            });
    }
    #endregion

    #region Helpers

    private void Init()
    {
        paylineInfoContainer.gameObject.SetActive(false);
        coinPool = new UnityEngine.Pool.ObjectPool<Image>(
            createFunc: () =>
            {
                var coin = Instantiate(coinPrefab, coinParent).GetComponent<Image>();
                coin.gameObject.SetActive(false); // bắt đầu ẩn
                return coin;
            },
            actionOnGet: (coin) =>
            {
                coin.gameObject.SetActive(true);
                coin.transform.localScale = Vector3.one;
                coin.color = new Color(coin.color.r, coin.color.g, coin.color.b, 0f);
            },
            actionOnRelease: (coin) =>
            {
                coin.gameObject.SetActive(false);
            },
            actionOnDestroy: (coin) =>
            {
                Destroy(coin.gameObject);
            },
            collectionCheck: false,  // không cần check trùng (cho nhanh)
            defaultCapacity: 10,     // số lượng khởi tạo
            maxSize: 20             // tối đa object trong pool
        );
        linePool = new UnityEngine.Pool.ObjectPool<GameObject>(
            createFunc: () =>
            {
                var line = Instantiate(linePrefab, lineContainer);
                line.SetActive(false); // bắt đầu ẩn
                return line;
            },
            actionOnGet: (line) =>
            {
                line.SetActive(true);
                line.transform.localScale = Vector3.one;
            },
            actionOnRelease: (line) =>
            {
                line.SetActive(false);
            },
            actionOnDestroy: (line) =>
            {
                Destroy(line);
            },
            collectionCheck: false,  // không cần check trùng (cho nhanh)
            defaultCapacity: 20,     // số lượng khởi tạo
            maxSize: 100             // tối đa object trong pool
        );
    }


    private void InitColumns()
    {
        for (int i = 0; i < 5; i++)
        {
            SlotColumn column = Instantiate(columnPrefab, columnContainer).GetComponent<SlotColumn>();
            column.SetInfo(this, i);
            column.SetRandomSprite();
            slotColumnList.Add(column);
        }
    }

    private void NextTween()
    {
        if (tweenQueue.Count > 0)
        {
            TweenCallback nextTween = tweenQueue.Dequeue();
            DOTween.Sequence().AppendCallback(nextTween);
        }
        // Hết tween = hết show win line
        else
        {
            if (CheckGetFreeSpin())
            {
                if (spinType == SpinType.NORMAL || spinType == SpinType.AUTO)
                {
                    spinType = SpinType.FREE_NORMAL;
                }
            }
            Reset();
            // Nếu đang auto spin thì spin tiếp
            if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
            {
                OnStartSpin();
            }
        }
    }

    protected void SetDarkAllItems()
    {
        spinBackgroundImage.color = Color.gray;
        foreach (SlotColumn column in slotColumnList)
        {
            column.SetDarkAllItems();
        }
    }

    protected void SetLightAllItems()
    {
        spinBackgroundImage.color = Color.white;
        foreach (SlotColumn column in slotColumnList)
        {
            column.SetLightAllItems();
        }
    }

    private void UpdateGameState(SlotGameState gameState)
    {
        this.gameState = gameState;
        UpdateSpinButtonUI();
    }

    private void Reset()
    {
        if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
        {
            UpdateGameState(SlotGameState.PREPARE);
        }
        if (lineOneByOneSequence.IsActive())
        {
            lineOneByOneSequence.Kill();
        }
        allLinesList.Clear();
        lineOneByOneList.Clear();
        winningLineIdList.Clear();
        SetLightAllItems();
        foreach (Transform lineRect in lineContainer)
        {
            linePool.Release(lineRect.gameObject);
        }
        foreach (SlotColumn column in slotColumnList)
        {
            column.ExtraTime = 0f;
        }
        SetInfoSessionText("");
        ScatterCount = 0;
        paylineInfoContainer.gameObject.SetActive(false);


        // isFiveOfaKind = false;
        // isWinScatter = false;
        // listActionHandleSpin.Clear();
        // slotViews.Clear();
        // countScatter = 0;

        // if (finishData != null)
        // {
        //     if (isInFreeSpin && !isFreeSpin)
        //     {
        //         countTotalAgFreespin = 0;
        //     }
        // }
        // setStateBtnSpin();
        // if (freespinLeft > 0)
        // {
        //     if (Config.curGameId == (int)GAMEID.SLOTTARZAN)
        //     {
        //         lbFreespinLeft.gameObject.SetActive(true);
        //         lbFreespinLeft.text = freespinLeft.ToString();
        //     }
        //     else
        //     {
        //         animFreespinNum.gameObject.SetActive(true);
        //         lbFreespinLeft.text = Config.getTextConfig("txt_freespinRM") + ": " + freespinLeft;
        //     }
        // }
        // else
        // {
        //     if (animBgFreeSpin != null)
        //     {
        //         animBgFreeSpin.gameObject.SetActive(false);
        //     }
        //     if (Config.curGameId != (int)GAMEID.SLOTTARZAN)
        //     {
        //         animFreespinNum.gameObject.SetActive(false);
        //     }
        //     else
        //     {
        //         lbFreespinLeft.gameObject.SetActive(false);
        //     }
        // }
    }

    #endregion

    public SpinType GetSpinType() => spinType;
}
