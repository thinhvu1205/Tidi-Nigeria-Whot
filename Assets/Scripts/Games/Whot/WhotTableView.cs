using System.Collections;
using System.Collections.Generic;
using Api;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WhotTableView : BaseView
{
    [SerializeField] private Transform betItemParent;
    [SerializeField] private GameObject betItemPrefab;
    private Bets betsList;
    private void Start()
    {
        GetListBet();
    }

    public void OnClickBetItem()
    {
        UIManager.Instance.OpenGame("whot");
    }

    private void UpdateVisuals()
    {

    }

    private async UniTask GetListBet()
    {
        Bets bets = await DataSender.GetListBet("whot-game");
        betsList = bets;
        Debug.Log("List bet game whot : " + bets.ToString());
    }

    public void OnClickMatchMaking()
    {
        // HandleMatchMaking();
       // NetworkManager.INSTANCE.CreateMatch("whot-game");
    }

    private async UniTask HandleMatchMaking()
    {
        var response = await DataSender.CreateMatch("whot-game");
        DataSender.JoinMatch(response.MatchId);
        Debug.Log(response.MatchId);
    }
    
}
