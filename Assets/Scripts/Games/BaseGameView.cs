using System.Collections;
using System.Collections.Generic;
using Proto;
using Games;
using Games.Card;
using Nakama;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class BaseGameView : BaseView
{
    
    public List<Player> players = new List<Player>();
    [SerializeField]
    protected List<Vector2> listPosView = new List<Vector2>();

    public Player thisPlayer;

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
    public int GetCountListPosView() { return listPosView.Count; }
    
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
    
    public virtual void OnClickBack()
    {
        // SoundManager.instance.soundClick();
        // var subView = Instantiate(UIManager.instance.loadPrefab("GameView/Objects/GroupMenu"), transform);
        // subView.transform.localScale = Vector3.one;
    }
    
    public virtual void OpenRule()
    {

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
