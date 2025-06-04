using System.Collections;
using System.Collections.Generic;
using Api;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TableViewWhot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CallApi();
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
        DataSender.MakingMatch("whot-game");
        // DataSender.CreateMatch("whot-game");
    }
    
}
