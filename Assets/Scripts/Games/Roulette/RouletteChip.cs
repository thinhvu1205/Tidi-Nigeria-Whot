using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteChip : MonoBehaviour
{
    [SerializeField] private Sprite[] listSpriteChip;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textBet;
    public bool IsDealt { get; set; } = false;
    public long Value { get; private set; }

    public void SetInfo(int id, long value)
    {
        image.sprite = listSpriteChip[id];
        image.SetNativeSize();
        image.transform.localPosition = new Vector3(0, 32, 0);
        image.transform.DOLocalMove(Vector3.zero, 0.25f);
        image.transform.DOScale(Vector3.one * 0.5f, 0.25f);
        textBet.text = Utility.FormatMoney(value, true);
        IsDealt = false;
        Value = value;
    }
}
