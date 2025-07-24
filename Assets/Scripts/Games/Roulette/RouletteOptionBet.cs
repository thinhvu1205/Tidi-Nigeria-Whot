using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public bool ÍsIn1To18 => id >= 0 && id <= 18;
    public bool ÍsIn19To36 => id >= 19 && id <= 36;
    public bool IsInFirstLine => id % 3 == 0;
    public bool IsInSecondLine => id % 3 == 2;
    public bool IsInThirdLine => id % 3 == 1;
    private bool isHolding = false;
    private Coroutine turnOffCoroutine = null;

    private void Awake()
    {
        imageChoosing = GetComponent<Image>();
        button = GetComponent<Button>();
    }
    private void Start()
    {
        // buttonBetOption.onClick.AddListener(ClickButtonBetOption);
    }

    private void OnEnable()
    {
        imageChoosing.gameObject.SetActive(false);
    }

    public void OnTriggerUp()
    {
        // if (!RouLetteView.instance.isBetTime) return;

        // isHolding = false;
        // if (turnOffCoroutine != null)
        // {
        //     StopCoroutine(turnOffCoroutine);
        // }
        // turnOffCoroutine = StartCoroutine(TurnOffFlashEffect());
    }

    public void OnTriggerDown()
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
    }

    private void ClickButtonBetOption()
    {
        // if (!RouLetteView.instance.isBetTime) return;
        // if ((RouLetteView.instance.totalBetDeal + RouLetteView.instance.totalBetValue) >= RouLetteView.instance.agTable * 100)
        // {
        //     UIManager.instance.showToast($"Pinakamataas ng pagtaya: {RouLetteView.instance.agTable * 100}");
        //     return;
        // }

        // RouLetteView.instance.ClickButtonSendBet(id);
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
