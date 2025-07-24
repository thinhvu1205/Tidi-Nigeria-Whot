using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaccaratView : BaseGameView
{
    [SerializeField] private GameObject chipPrefab, playerPrefab, popupHistoryPrefab;
    [SerializeField]
    private TextMeshProUGUI textCountDown, textTime, textWaiting, textMaxbet, textNotEnoughGold,
    textBankerValue, textPlayerValue, textTieValue, textPlayerScore, textBankerScore;
    [SerializeField] private SkeletonGraphic animationVictory;
    [SerializeField] private Button buttonRebet, buttonDouble, buttonEmoji, buttonMenu;
    [SerializeField] private Button[] listButtonBet;
    [SerializeField] private TextMeshProUGUI[] listBetValue, listBetPair;
    [SerializeField] private GameObject[] listBoxBet, listCardBanker, listCardPlayer;
    [SerializeField] private Transform[] listPlayerPosition, listInvitePosition;
    private UnityEngine.Pool.ObjectPool<GameObject> chipPool, cardPool;

}
