using System;
using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RouletteOptionBet : MonoBehaviour
{
    // 1 -> 36: Bet số
    // 37 -> 39: Bet hàng 1, 2, 3
    // 40 -> 42: Bet dozen 1, 2, 3
    // 43: Bet 1 to 18
    // 44: Bet 19 to 36
    // 45: Bet Red
    // 46: Bet Black
    // 47: Bet Even
    // 48: Bet Odd
    public enum RouletteOptionColor
    {
        NONE,
        RED,
        BLACK,
    }
    private Image imageChoosing;
    private Button button;
    [SerializeField] private int id;
    [SerializeField] private RouletteOptionColor color;
    public int Id => id;
    public bool IsEven => id % 2 == 0;
    public bool IsOdd => id % 2 != 0;
    public bool IsRed => color == RouletteOptionColor.RED;
    public bool IsBlack => color == RouletteOptionColor.BLACK;
    public bool IsInFirstDozen => id / 12 == 0;
    public bool IsInSecondDozen => id / 12 == 1;
    public bool IsInThirdDozen => id / 12 == 2;
    public bool IsIn1To18 => id >= 0 && id <= 18;
    public bool IsIn19To36 => id >= 19 && id <= 36;
    public bool IsInFirstLine => id % 3 == 0;
    public bool IsInSecondLine => id % 3 == 2;
    public bool IsInThirdLine => id % 3 == 1;
    public Image HighlightImage => imageChoosing;
    private bool isHolding = false;
    private Coroutine turnOffCoroutine = null;
    public List<RouletteChip> Chips { get; private set; } = new List<RouletteChip>();

    public event Action<int> OnTriggerDown;
    public event Action<int> OnTriggerUp;

    private void Awake()
    {
        imageChoosing = GetComponent<Image>();
        imageChoosing.enabled = true;
        Utility.SetAlpha0(imageChoosing);
        button = GetComponent<Button>();

        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        // Pointer Down
        EventTrigger.Entry downEntry = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        downEntry.callback.AddListener((data) => TriggerDown());
        trigger.triggers.Add(downEntry);

        // Pointer Up
        EventTrigger.Entry upEntry = new()
        {
            eventID = EventTriggerType.PointerUp
        };
        upEntry.callback.AddListener((data) => TriggerUp());
        trigger.triggers.Add(upEntry);
    }
    private void Start()
    {
        // buttonBetOption.onClick.AddListener(ClickButtonBetOption);
    }

    public void TriggerUp()
    {
        // if (!RouLetteView.instance.isBetTime) return;

        // isHolding = false;
        // if (turnOffCoroutine != null)
        // {
        //     StopCoroutine(turnOffCoroutine);
        // }
        // turnOffCoroutine = StartCoroutine(TurnOffFlashEffect());
        OnTriggerUp?.Invoke(id);
    }

    public void TriggerDown()
    {
        // if (!RouLetteView.instance.isBetTime) return;

        // isHolding = true;
        // if (turnOffCoroutine != null)
        // {
        //     StopCoroutine(turnOffCoroutine);
        //     turnOffCoroutine = null;
        // }

        // if (imageChoosing != null)
        // {
        //     foreach (var item in imageChoosing)
        //     {
        //         if (item != null)
        //             item.gameObject.SetActive(true);
        //     }
        // }
        OnTriggerDown?.Invoke(id);
    }


    public void AddChip(RouletteChip chip)
    {
        Chips.Add(chip);
    }

    public void RemoveChip(RouletteChip chip)
    {
        Chips.Remove(chip);
    }

    private IEnumerator TurnOffFlashEffect()
    {
        if (!isHolding)
        {
            yield return new WaitForSeconds(0.1f);
            imageChoosing.gameObject.SetActive(false);
        }
    }
}
