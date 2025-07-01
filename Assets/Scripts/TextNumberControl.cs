using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using Globals;

public class TextNumberControl : MonoBehaviour
{
    public enum FORMAT_TYPE
    {
        NUMBER = 0,
        MONEY = 1,
        MONEY_FROM_K = 2,
        NUMBER_FROM_M = 3
    }
    [SerializeField] TextMeshProUGUI numberText;
    [SerializeField] bool isFormatMoney = false;
    [SerializeField] private float scale = 1;
    [SerializeField] private FORMAT_TYPE FormatType = FORMAT_TYPE.NUMBER;

    private string text;
    private long number = 0;
    private Action callback;

    public string GetTextNumber() { return numberText.text; }
    public string Text   
    {
        get { return text; }   
        set { SetText(value); }  
    }

    private void Awake()
    {
        numberText = GetComponent<TextMeshProUGUI>();
    }

    public void ResetValue()
    {
        DOTween.Kill(numberText.transform);
        numberText.text = "0";
        text = "0";
        numberText.transform.localScale = new Vector2(scale, scale);
        number = 0;
    }
    private void SetText(string value)
    {
        text = value;
        numberText.text = text;
        number = Utility.SplitToLong(value);
    }
    public void SetValue(long value, bool isRun = false, float timeRun = 0.5f, string msg = "", Action cb = null)
    {
        callback = cb;
        long startNumber = number;
        if (msg != "")
        {
            Debug.Log(msg + value);
            Debug.Log(msg + "StartNUmber:" + startNumber);
        }
        if (value == number)
        {
            isRun = false;
        }
        if (isRun && value != number)
        {
            DOTween.To(() => startNumber, x => startNumber = x, value, timeRun)
                //.SetEase(Ease.InSine)
                .OnUpdate(() =>
                {
                    numberText.text = FormatValue(startNumber);
                })
                .OnComplete(() =>
                {
                    numberText.text = FormatValue(value);
                    number = value;

                    callback?.Invoke();
                    callback = null;
                })
                .SetId("tweenNumber");
            Vector2 normalScale = new Vector2(scale, scale);
            Vector2 biggerScale = new Vector2(scale + 0.2f, scale + 0.2f);
            DOTween.Kill(numberText.transform);
            DOTween.Sequence()
                .Append(numberText.transform.DOScale(biggerScale, timeRun * 0.45f))
                .AppendInterval(timeRun * 0.45f)
                .Append(numberText.transform.DOScale(normalScale, timeRun * 0.1f)).SetId("tweenScale");
        }
        else
        {
            numberText.text = Utility.FormatNumber(value);
            number = value;
        }
        number = value;
    }
    private string FormatValue(long number)
    {
        string valueStr = "0";
        if (FormatType == FORMAT_TYPE.NUMBER)
        {
            valueStr = Utility.FormatNumber(number);
        }
        else if (FormatType == FORMAT_TYPE.MONEY)
        {
            valueStr = Utility.FormatMoney(number);
        }
        else if (FormatType == FORMAT_TYPE.MONEY_FROM_K)
        {
            valueStr = Utility.FormatMoney(number, true);
        }
        else if (FormatType == FORMAT_TYPE.NUMBER_FROM_M)
        {
            if (number > 1000000)
            {
                valueStr = Utility.FormatMoney(number);
            }
            else
            {
                valueStr = Utility.FormatNumber(number);
            }
        }
        return valueStr;
    }
    public void SetLastValue()
    {
        if (callback != null)
        {
            callback.Invoke();
            callback = null;
        }
        DOTween.Kill(numberText.transform);
        DOTween.Kill("tweenNumber");
        DOTween.Kill("tweenScale");
        numberText.text = Utility.FormatNumber(number);
    }
}
