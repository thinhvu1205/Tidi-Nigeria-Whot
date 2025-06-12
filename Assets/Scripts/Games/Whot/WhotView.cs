using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotView : GameView
{
    [SerializeField] private Transform gameContainer;
    private WhotGame game;
    protected override void Awake()
    {
        WhotHandler whotHandler = new(this);
        GameManager.Instance.SetGameHandler(whotHandler);
        game = gameContainer.GetComponent<WhotGame>();
        game.gameObject.SetActive(true);
    }

    public void HandleJoinMatch(IMatch match)
    {
        game.HandleJoinMatch(match);
    }

    public void HandleUpdateTable(UpdateTable data)
    {
        game.HandleUpdateTable(data);
        // Handle match state updates
    }

    public void HandleUpdateGameState(UpdateGameState data)
    {
        game.HandleUpdateGameState(data);
    }

    public void HandleUpdateDeal(UpdateDeal data)
    {
        game.HandleUpdateDeal(data);
    }

    public void HandleUpdateTurn(UpdateTurn data)
    {
        game.HandleUpdateTurn(data);
    }

    public void HandleUpdateCardState(UpdateCardState data)
    {
        game.HandleUpdateCardState(data);
    }

    public void HandleMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        // Handle player presence updates
    }

    public void HandleMatchLeave()
    {
        // Handle match leave logic
    }
}
