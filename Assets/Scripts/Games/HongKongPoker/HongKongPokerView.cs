using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Games.Card;
using Globals;
using Nakama;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using GameState = Proto.GameState;

public class HongKongPokerView : BaseDiceGameView
{
    [SerializeField] private Transform cardContainer, boxBetContainer, arrowSwapContainer, buttonChangeCardContainer, chipContainer, dealer;
    // [SerializeField] private PlayerViewHongKongPoker dealerHkPoker;
    [SerializeField] private HongKongPokerPot pot;
    [SerializeField] private HongKongPokerToggleBetContainer toggleContainer;
    [SerializeField] private HongKongPokerChip chipPrefab;
    [SerializeField] private HongKongPokerBoxBet boxBetPrefab;
    [SerializeField] private SkeletonGraphic animationStart;
    [SerializeField] private Sprite spriteFrameMask;
    [SerializeField] private Sprite[] listImageWinLose;
    [SerializeField] private TextMeshProUGUI textTipChip, textThanks, textCountDown;
    private List<HongKongPokerBoxBet> listBoxBet = new() { null, null, null, null, null };
    private List<List<CardModel>> playerCards = new()
    {
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>()
    };

    private Vector2[] listCardPosition = new Vector2[]
    {
        new(-12, -207),
        new(-404, -49),
        new(-357, 168),
        new(350, 168),
        new(403, -60)
    };

    private Vector2[] listBoxBetPosition = new Vector2[]
    {
        new(10, -105),
        new(-352, -126),
        new(-309, 93),
        new(306, 72),
        new(395, -144)
    };

    private UnityEngine.Pool.ObjectPool<HongKongPokerBoxBet> boxBetPool;
    private UnityEngine.Pool.ObjectPool<HongKongPokerChip> chipPool;
    private GameState gameState = GameState.Preparing;

    protected override void Awake()
    {
        base.Awake();
        InitPool();
    }

    #region API Handlers

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        Debug.Log("Match: " + match);
                HandleStartGame();

    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        UpdateTable data = UpdateTable.Parser.ParseFrom(matchState.State);

        // Debug.Log("Update Table: " + data);
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
        var data = UpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Deal: " + data);

    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
        var data = UpdateGameState.Parser.ParseFrom(matchState.State);
        // Debug.Log("Update Game State: " + data);
        gameState = data.State;
        switch (gameState)
        {
            case GameState.Preparing:
                textCountDown.gameObject.SetActive(true);
                textCountDown.text = data.CountDown.ToString();
                break;
            case GameState.Play:
                HandleStartGame();
                break;
            case GameState.Matching:
                
                break;
            case GameState.Idle:
                break;
            case GameState.Reward:
                
                break;
            case GameState.Finish:
                break;
            default:
                break;
        }
    }

    public override void HandleUpdateCardState(IMatchState matchState)
    {
        base.HandleUpdateCardState(matchState);
        var data = UpdateCardState.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Card State: " + data);

    }

    public override void HandleUpdateTurn(IMatchState matchState)
    {
        base.HandleUpdateTurn(matchState);
        var data = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Turn: " + data);
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var data = UpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Finish: " + data);
    }
    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var data = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Wallet: " + data);
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        Destroy(gameObject);
    }

    #endregion

    #region Button
    public void OnClickSwap()
    {

    }

    public void OnClickCancel()
    {

    }

    public void OnClickSendTip()
    {
        
    }

    #endregion

    #region Animation
    private IEnumerator AnimateArrowSwapLoop()
    {
        for (int i = 0; i < 10; i++)
        {
            if (arrowSwapContainer == null) yield break;

            Vector2 startPos = new(0, 0);
            Vector2 endPos = new(0, -25);

            yield return arrowSwapContainer.transform.DOLocalMove(startPos, 0.4f).SetEase(Ease.OutCubic).WaitForCompletion();

            yield return new WaitForSeconds(0.05f);

            yield return arrowSwapContainer.transform.DOLocalMove(endPos, 0.15f).SetEase(Ease.OutCubic).WaitForCompletion();

        }
    }

    private IEnumerator FoldDown(int index, CardModel card, float delay)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;
        yield return new WaitForSeconds(delay);

        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.55f, 1f), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    // card.setTextureWithCode(0);
                    // card.setDark(true, spriteFrameMask);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.15f).SetEase(Ease.OutCubic));

        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutCubic));
    }

    private void FoldUp(int index, CardModel card, int code)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;

        // Scale animation
        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.65f, 1f), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    // card.setTextureWithCode(code);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.2f).SetEase(Ease.OutCubic));

        // Skew animation (simulated via rotation)
        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
    }
    #endregion

    private void HandleStartGame()
    {
        Debug.Log("handle start game");
        textCountDown.gameObject.SetActive(false);
        Utility.PlayAnimation(animationStart, "startgame", false);
    }

    private void InitPool()
    {
        chipPool = new UnityEngine.Pool.ObjectPool<HongKongPokerChip>(
            createFunc: () =>
            {
                var chip = Instantiate(chipPrefab, chipContainer);
                chip.gameObject.SetActive(false); // bắt đầu ẩn
                return chip.GetComponent<HongKongPokerChip>();
            },
            actionOnGet: (chip) =>
            {
                chip.gameObject.SetActive(true);
                chip.transform.localScale = Vector3.one;
            },
            actionOnRelease: (chip) =>
            {
                chip.gameObject.SetActive(false);
            },
            actionOnDestroy: (chip) =>
            {
                Destroy(chip);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
        boxBetPool = new UnityEngine.Pool.ObjectPool<HongKongPokerBoxBet>(
            createFunc: () =>
            {
                var boxBet = Instantiate(boxBetPrefab, boxBetContainer);
                boxBet.gameObject.SetActive(false); // bắt đầu ẩn
                return boxBet.GetComponent<HongKongPokerBoxBet>();
            },
            actionOnGet: (boxBet) =>
            {
                boxBet.gameObject.SetActive(true);
                boxBet.transform.localScale = Vector3.one;
            },
            actionOnRelease: (boxBet) =>
            {
                boxBet.gameObject.SetActive(false);
            },
            actionOnDestroy: (boxBet) =>
            {
                Destroy(boxBet);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
    }
}
