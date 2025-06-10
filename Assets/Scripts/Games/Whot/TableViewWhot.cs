using System.Collections;
using System.Collections.Generic;
using Api;
using Cysharp.Threading.Tasks;
using Games.Whot;
using UnityEngine;

public class TableViewWhot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CallApi();
        GameManager.Instance.SetGameHandler(new WhotHandler());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async UniTask CallApi()
    {
        Bets bets = await  DataSender.GetListBet("whot-game");
        Debug.Log("List bet game whot : " + bets.ToString());
    }

    public void OnClickMatchMaking()
    {
        // HandleMatchMaking();
        DataSender.MakingMatch("whot-game");
       // NetworkManager.INSTANCE.CreateMatch("whot-game");
    }

    private async UniTask HandleMatchMaking()
    {
        var response = await DataSender.CreateMatch("whot-game");
        DataSender.JoinMatch(response.MatchId);
        Debug.Log(response.MatchId);
    }
    
}
