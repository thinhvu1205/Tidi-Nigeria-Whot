using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HongKongPokerPot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] listNumber;
    private int value = 0;
    public int PotValue { get; private set; } = 0;
    public int ValueChange { get; private set; } = 0;

    private void Awake()
    {
        for (int i = 0; i < listNumber.Length; i++)
        {
            listNumber[i].text = "0";
        }
    }

    public void SetValue(int valueNew, float delayTime = 0)
    {
        ValueChange = valueNew - value;
        PotValue = valueNew;
        DOTween.Sequence()
            .AppendInterval(delayTime)
            .AppendCallback(() =>
            {
                TweenPotTo(valueNew);
            });
    }

    public void TweenPotTo(int newValue)
    {
        DOTween
            .To(() => value, x => value = x, newValue, 2.0f)
            .OnUpdate(() =>
            {
                string valueStr = "";
                valueStr = value.ToString();
                int count = 0;
                for (int i = 0; i < listNumber.Length; i++)
                {
                    if (i >= listNumber.Length - valueStr.Length)
                    {
                        listNumber[i].text = valueStr[count].ToString();
                        DOTween.Sequence()
                            .Append(listNumber[i].transform.DOScale(new Vector2(1.4f, 1.4f), 0.1f))
                            .Append(listNumber[i].transform.DOScale(Vector2.one, 0.1f)).SetEase(Ease.InBack);
                        count++;
                    }
                    else
                    {
                        listNumber[i].text = "0";
                    }
                }
            })
            .SetEase(Ease.OutSine);
    }
}
