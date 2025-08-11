using System.Collections;
using System.Collections.Generic;
using Games;
using Games.Card;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using GameState = Proto.GameState;

public class BaseDiceGameView : BaseGameView
{
    [SerializeField] protected List<BasePlayerView> listPlayerView = new List<BasePlayerView>();
    [SerializeField] protected GameObject invitePrefab, cardPrefab;
    [SerializeField] protected Transform inviteContainer, playerContainer, hiddenPlayerContainer;

    protected List<Player> players = new();
    protected Player thisPlayer = new();
    protected List<CardModel> cardPool = new List<CardModel>();
    protected List<CardModel> cardsOnTable = new List<CardModel>();
    protected List<ChipBet> chipPool = new List<ChipBet>();
    protected List<GameObject> listBtnInvite = new List<GameObject>();
    public GameState GameState { get; protected set; } = GameState.Idle;



    private int GetNextIndex()
    {
        for (int i = 0; i < listPlayerView.Count; i++)
        {
            if (!listPlayerView[i].gameObject.activeSelf)
            {
                return i;
            }
        }
        return -1;
    }
    
    protected virtual void UpdateListPlayer(List<Player> data)
    {
        // oldData = data;
        int lastIndex = listPlayerView.Count - 1;

        for (int idx = 0; idx < listPlayerView.Count; idx++)
        {

            if (!listPlayerView[idx].gameObject.activeSelf )
                continue;

            bool isStillExist = false;
            foreach (var p in data)
            {
                if (p.Id == listPlayerView[idx].id)
                {
                    isStillExist = true;
                    break;
                }
            }

            if (!isStillExist)
                listPlayerView[idx].gameObject.SetActive(false);
        }

        BasePlayerView lastPlayer = listPlayerView[lastIndex];
        if (lastPlayer.gameObject.activeSelf && lastPlayer ==null && data.Count <= lastIndex + 1)
        {
            lastPlayer.gameObject.SetActive(false);
        }

        foreach (var playerInfo in data)
        {
            if (playerInfo.Id == User.userMain.userId)
                continue;

            bool isExist = false;
            foreach (var player in listPlayerView)
            {
                if (player.gameObject.activeSelf && player != null && player.id == playerInfo.Id)
                {
                    isExist = true;
                    break;
                }
            }

            if (!isExist)
            {
                UpdatePlayer(playerInfo, data.Count);
            }
        }
    }
    
    private void UpdatePlayer(Player playerInfo, int totalPlayers)
    {
        int index = GetNextIndex();
        Debug.Log($"UpdatePlayer at index {index}: {playerInfo.UserName}");

        if (index != -1)
        {
            listPlayerView[index].gameObject.SetActive(true);
        }

        if (index == listPlayerView.Count - 1 || index == -1)
        {
            if (listPlayerView[index] == null)
            {
                index = listPlayerView.Count;
                // Text label = lastSlot.GetChild(0).GetComponent<Text>();
                // label.text = $"+{totalPlayers - bgPlayer.childCount}";
                return;
            }
            else if (index == -1)
            {
                return;
            }
        }

        // var playerNode = bgPlayer.GetChild(index);
        // var playerCasino = playerNode.GetComponent<PlayerCasino>();
        listPlayerView[index].SetData(playerInfo);
        //
        // Button avatarButton = playerNode.Find("avatar").GetComponent<Button>();
        // if (avatarButton == null)
        // {
        //     avatarButton = playerNode.Find("avatar").gameObject.AddComponent<Button>();
        // }
        //
        // avatarButton.onClick.RemoveAllListeners();
        // avatarButton.onClick.AddListener(() => ProfileClick(playerCasino.id));
    }
}
