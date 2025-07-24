using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteView : BaseGameView
{
    [SerializeField] private Image imageSpin, imageBall, imagePopupHistory;
    [SerializeField]
    private TextMeshProUGUI textResult, textNumWin, textNumLose, textPercentRed, textPercentBlack,
    textClearValue, textDealValue, textCoinValue, textMoney, textDeal;
    [SerializeField] private GameObject chipPrefab;
    [SerializeField] private Button buttonDouble, buttonDeal, buttonClear, buttonHistory, buttonCloseHistory, buttonRebet;
    [SerializeField] private SkeletonGraphic animationResult, animationWinLose;
    [SerializeField] private RectTransform transformTabResult, transformButtonMenu, tableBet, tableSpin;

}
