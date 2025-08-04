using System.Collections;
using System.Collections.Generic;
using Games.Card;
using Nakama;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class HongKongPokerView : BaseDiceGameView
{
    [SerializeField] private Transform cardContainer, boxBetContainer, arrowSwapContainer, buttonChangeCardContainer, chipContainer, startTime;
    // [SerializeField] private PlayerViewHongKongPoker dealerHkPoker;
    [SerializeField] private HongKongPokerPot pot;
    [SerializeField] private HongKongPokerToggleBetContainer toggleContainer;
    [SerializeField] private HongKongPokerChip chipPrefab;
    [SerializeField] private HongKongPokerBoxBet boxBetPrefab;
    [SerializeField] private SkeletonGraphic animationStart;
    [SerializeField] private Sprite spriteFrameMask;
    [SerializeField] private Sprite[] listImageWinLose;
    [SerializeField] private TextMeshProUGUI textTipChip, textThanks;
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

    protected override void Awake()
    {
        base.Awake();
        InitPool();
    }

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        Debug.Log("Match: " + match);
    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        UpdateTable data = UpdateTable.Parser.ParseFrom(matchState.State);

        Debug.Log("Update Table: " + data);
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
