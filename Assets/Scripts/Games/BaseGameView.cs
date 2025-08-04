using System.Collections;
using System.Collections.Generic;
using Proto;
using Games;
using Games.Card;
using Globals;
using Nakama;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using GameState = Proto.GameState;

public class BaseGameView : BaseView
{
    
    public List<Player> players = new List<Player>();
    [SerializeField]
    protected List<BasePlayerView> listPlayerView = new List<BasePlayerView>();

    public Player thisPlayer = new Player();

    [SerializeField]
    protected BasePlayerView playerViewPrefab;
    [SerializeField]
    TextMeshProUGUI lbInfo;

    [SerializeField]
    GameObject invitePrefab;
    [SerializeField]
    Transform inviteContainer;
    [SerializeField]
    public Transform playerContainer;
    [SerializeField]
    TextMeshProUGUI txtGameName;
    [SerializeField] private Transform m_HiddenPlayersTf;


    public int agTable;
    public int maxbet = 0;

    protected List<CardModel> cardPool = new List<CardModel>();
    protected List<JObject> listDelayEvt = new List<JObject>();
    protected List<CardModel> cardsOnTable = new List<CardModel>();
    protected List<ChipBet> chipPool = new List<ChipBet>();
    protected List<GameObject> listBtnInvite = new List<GameObject>();
    public List<string> delayEvents = new List<string>();
    public GameState stateGame = GameState.Idle;
    [HideInInspector]
    public JObject dataLeave;
    // [HideInInspector]
    // public string soundBg = SOUND_GAME.IN_GAME_COMMON;
    
    public override void OnDestroy()
    {
        // Logging.Log("-=-=OnDestroy ");
        // Config.lastGameIDSave = Config.curGameId;
        // UIManager.instance.lobbyView.setQuickPlayGame(Config.lastGameIDSave);
        // User.userMain.lastGameID = 0;
        // foreach (var c in cardPool)
        // {
        //     Destroy(c.gameObject);
        // }
        // cardPool.Clear();
        // //foreach (var c in chipPool)
        // //{
        // //    Destroy(c.gameObject);
        // //}
        // chipPool.ForEach(chip =>
        // {
        //     Destroy(chip.gameObject);
        // });
        // UIManager.instance.destroyAllPopup();
        // chipPool.Clear();
        // HandleGame.listDelayEvt.Clear();
        // SoundManager.instance.playMusic();
        // SoundManager.instance.stopAllCurrentEffect();
        // SocketSend.sendUAG();
        // SocketSend.getInfoSafe();
        //if (!Globals.Config.listGamePlaynow.Contains(Globals.Config.curGameId)) //game ko phai playnow thi back ra tableview moi get farminfo con game playnow mac dinh lobbyview da send roi
        //{
        //}
        base.OnDestroy();
    }

    public virtual void LoadInfoMatch(Match match)
    {
        agTable = (int) match.Bet.MarkUnit;
        if (lbInfo != null)
        {
            lbInfo.text = $"ID {match.TableId}\nBet: {Utility.FormatMoney(agTable)}";
        }
    }
    
    public virtual void OnClickBack()
    {
        // SoundManager.instance.soundClick();
        // var subView = Instantiate(UIManager.instance.loadPrefab("GameView/Objects/GroupMenu"), transform);
        // subView.transform.localScale = Vector3.one;
    }
    
    public virtual void OpenRule()
    {

    }
    
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

    public virtual void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        
    }

    public virtual void HandleMatchJoin(Match match)
    {

    }

    public virtual void HandleMatchPresence(IMatchPresenceEvent presenceEvent)
    {
    }

    public virtual void HandleMatchLeave()
    {
        
    }

    public virtual void HandleUpdateTable(IMatchState matchState)
    {
        
    }
    
    public virtual void HandleUpdateUserInTable(IMatchState matchState)
    {
        
    }
    
    public virtual void HandleUpdateDeal(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateTurn(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateCardState(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateGameState(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateWallet(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        
    }

    public virtual void HandleFinish(IMatchState matchState)
    {
        
    }
}
