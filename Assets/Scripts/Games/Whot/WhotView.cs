using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotView : GameView
{
    [SerializeField] private Transform matchMakingContainer, gameContainer;
    private WhotMatchMaking matchMaking;
    private WhotGame game;
    protected override void Awake()
    {
        WhotHandler whotHandler = new(this);
        GameManager.Instance.SetGameHandler(whotHandler);
        matchMaking = matchMakingContainer.GetComponent<WhotMatchMaking>();
        game = gameContainer.GetComponent<WhotGame>();

        matchMakingContainer.gameObject.SetActive(true);
        gameContainer.gameObject.SetActive(false);
    }

    public void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        matchMaking.HandleMatchFound(matchmakerMatched);
    }

    public void HandleJoinMatch(IMatch match)
    {
        game.HandleJoinMatch(match);

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            gameContainer.gameObject.SetActive(true);
        })
        .AppendInterval(3f)
        .OnComplete(() =>
        {
            matchMakingContainer.gameObject.SetActive(false);
        });
    }

    public void HandleMatchState(IMatchState state)
    {
        // Handle match state updates
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
