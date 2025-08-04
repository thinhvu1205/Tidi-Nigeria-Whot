using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;
using Api;
using Google.Protobuf;
using SixiangSymbol = Api.SiXiangSymbol;
public class SixiangChooseGameBonus : MonoBehaviour
{
    [SerializeField] Transform container;
    private bool isInteractable = true;

    protected Dictionary<int, SiXiangSymbol> BonusGameDictionary => new()
    {
        { 0, SiXiangSymbol.SixangbonusDragonpearlGame }, // Dragon Pearl
        { 1, SiXiangSymbol.SixangbonusGoldpickGame}, // Gold Pick
        { 2, SiXiangSymbol.SixangbonusRapidpayGame }, // Rapid Pay
        { 3, SiXiangSymbol.SixangbonusLuckydrawGame }, // Lucky Draw
    };

    private void OnEnable()
    {
        container.localScale = Vector2.one;
        isInteractable = true;
    }
    public void OnClickSelectGame(int index)
    {
        if (!isInteractable) return;
        InfoBet infoBet = new()
        {
            Id = (int)BonusGameDictionary[index],
        };
        DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
        isInteractable = false;
    }
    public void OnClose()
    {
        container.DOScale(new Vector2(0.8f, 0.8f), 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
