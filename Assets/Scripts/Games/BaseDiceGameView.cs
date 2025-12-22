using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Games;
using Games.Card;
using Globals;
using Nakama;
using Proto;
using TMPro;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BaseDiceGameView : BaseGameView
{
    [SerializeField] private TextMeshProUGUI textMatchInfo, textGameName;
    [SerializeField] protected List<Vector2> listPosView;
    [SerializeField] protected BasePlayerView playerViewPrefab;
    [SerializeField] protected GameObject invitePrefab;
    [SerializeField] protected Transform inviteContainer, playerContainer, hiddenPlayerContainer, emojiContainer;
    [SerializeField] protected EmojiItem emojiItemPrefab;
    public int MarkUnit { get; private set; }

    protected Dictionary<string, BasePlayerView> userIdToView = new();
    protected BasePlayerView currentPlayerView = new();
    protected List<Player> players = new List<Player>();
    protected List<Player> rearrangedPlayers = new List<Player>();
    [SerializeField] protected List<Player> playingPlayers = new List<Player>();
    protected List<GameObject> listBtnInvite = new List<GameObject>();
    private float interactTimer = 0, interactCountdown = 2;
    private ChatInGameView chatInGameView;
    private EmojiInGameView emojiInGameView;


    protected override void Start()
    {
        base.Start();
        chatInGameView = UIManager.Instance.OpenChatInGame();
        chatInGameView.Init();
        emojiInGameView = UIManager.Instance.OpenEmojiInGame();
        NetworkManager.INSTANCE.OnMessageTableReceived += NetworkManager_OnMessageTableReceived;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(chatInGameView.gameObject);
        Destroy(emojiInGameView.gameObject);
        NetworkManager.INSTANCE.OnMessageTableReceived -= NetworkManager_OnMessageTableReceived;
    }

    protected override void Update()
    {
        interactTimer -= Time.unscaledDeltaTime;
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && interactTimer <= 0)
        {
            interactTimer = interactCountdown;
            DataSender.SendMatchState((long)OpCodeRequest.OpcodeUserInteractCards, Array.Empty<byte>());
        }
    }


    protected override void OnApplicationPause(bool pause)
    {
        if (!pause && UIManager.Instance.gameView != null)
        {
            RequestSyncStateTable();
        }
    }

    protected virtual void RequestSyncStateTable()
    {
        
    }

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        Debug.Log("LOAD INFO MATCH: " + match.ToString());
        Debug.Log("CURRENT GAME ID: " + Config.currentGameId);
        if (!Constants.SELECT_TABLE_GAMES_ID.Contains(Config.currentGameId)) return;
        Debug.Log("LOAD INFO TABLE GAME");
        MarkUnit = match.MarkUnit;
        WantSwitchTable = false;
        foreach (var userId in userIdToView.Keys.ToList())
        {
            var view = userIdToView[userId];
            if (view != null)
            {
                Destroy(view.gameObject);
            }

            userIdToView.Remove(userId);
        }
        if (textMatchInfo != null)
        {
            textMatchInfo.text = $"ID {match.TableId}\nBet: {Utility.FormatMoney(MarkUnit)}";
        }
    }

    protected virtual void UpdatePosUserTable(UpdateTable update, bool isRearrange = false)
    {
        Debug.Log("UpdatePosUserTable: " + update.ToString());
        var localUserId = User.userProfile.UserId;
        if (listPosView == null || listPosView.Count == 0 || playerViewPrefab == null || localUserId == "") return;

        // 1) Cập nhật danh sách playing players
        if (update.PlayingPlayers.ToList().Count != 0)
        {
            playingPlayers = update.PlayingPlayers.ToList();
            Debug.Log($"Playing players updated: {string.Join(", ", playingPlayers.Select(p => p.UserName))}");
        }

        // 2) Cập nhật danh sách players
        players = update.Players.ToList();

        // Sắp xếp lại sao cho local player luôn ở vị trí đầu tiên
        if (isRearrange)
        {
            Debug.Log("REARRANGE PLAYER");
            Player currentPlayer = players.Find((player) => player.Id == localUserId);
            int startIndex = players.IndexOf(currentPlayer);

            if (startIndex >= 0)
            {
                List<Player> reordered = new();

                for (int i = 0; i < players.Count; i++)
                {
                    int index = (startIndex + i) % players.Count;
                    reordered.Add(players[index]);
                }
                // players = reordered;
                rearrangedPlayers = new(reordered);
            }

        }
        else
        {
            rearrangedPlayers = new(players);
        }

        // 3) Xử lý players leave
        foreach (var lp in update.LeavePlayers)
        {
            Debug.Log($"Player {lp.UserName} left the table");
            if (userIdToView.TryGetValue(lp.Id, out var view))
            {
                SoundManager.Instance.PlayEffectFromPath(Sound.REMOVE);
                Destroy(view.gameObject);
                RemovePlayerBoxBet(lp.Id);
                userIdToView.Remove(lp.Id);
            }
        }

        // 4) Xử lý players join
        foreach (var jp in update.JoinPlayers)
        {
            Debug.Log($"Player {jp.UserName} joined the table");
        }
        
        // 5) Tạo/update player views theo danh sách players mới
        var localInPlayers = players.Exists(p => p.Id == localUserId);

        // Gán vị trí 0 cho local player (nếu có)
        int positionIndex = 0;
        if (localInPlayers)
        {
            var local = players.Find(p => p.Id == localUserId);
            CreatePlayerView(local, listPosView[0]);
            positionIndex = 1;
        }
        else
        {
            // Nếu local không nằm trong players, giữ view local nếu đang có ở vị trí 0
            if (userIdToView.TryGetValue(localUserId, out var localView))
            {
                SetAnchoredPosition(localView, listPosView[0]);
                positionIndex = 1;
            }
        }

        // Gán các vị trí tiếp theo theo thứ tự trong update.players, bỏ qua local
        for (int i = 0; i < rearrangedPlayers.Count && positionIndex < listPosView.Count; i++)
        {
            var p = rearrangedPlayers[i];
            if (p.Id == localUserId) continue;
            CreatePlayerView(p, listPosView[positionIndex]);
            positionIndex++;
        } 
        
        if (currentPlayerView == userIdToView.GetValueOrDefault(localUserId)) return;

        // 6) Update UI current Player
        currentPlayerView = userIdToView.GetValueOrDefault(localUserId);
        if (currentPlayerView != null)
        {
            currentPlayerView.SetPositionInfoThisPlayer();
        }
    }

    protected virtual void CreatePlayerView(Player player, Vector2 anchoredPos)
    {
        if (!userIdToView.TryGetValue(player.Id, out var view) || view == null)
        {
            view = Instantiate(playerViewPrefab, playerContainer);
            userIdToView[player.Id] = view;
            view.gameObject.SetActive(true);
        }

        // Luôn cập nhật data và vị trí kể cả khi view đã tồn tại
        view.SetData(player);
        SetAnchoredPosition(view, anchoredPos);
    }

    private void SetAnchoredPosition(BasePlayerView view, Vector2 anchoredPos)
    {
        var rt = view.transform as RectTransform;
        if (rt != null) rt.anchoredPosition = anchoredPos;
        else view.transform.localPosition = new Vector3(anchoredPos.x, anchoredPos.y, 0f);
    }

    protected virtual void RemovePlayerBoxBet(string leavePlayerId)
    {

    }

    public void OnClickChat()
    {
        chatInGameView.transform.localScale = Vector3.one;
        chatInGameView.Show();
    }

    public void OnClickEmoji()
    {
        emojiInGameView.transform.localScale = Vector3.one;
        emojiInGameView.Show();
    }

    protected virtual void NetworkManager_OnMessageTableReceived(IApiChannelMessage message)
    {
        EmojiData emojiData = ConvertEmojiData(message);
        if (!string.IsNullOrEmpty(emojiData.emojiId)
            && !string.IsNullOrEmpty(emojiData.senderId)
            && string.IsNullOrEmpty(emojiData.receiverId)
        )
        {
            Debug.Log("TU GUI ");
            if (userIdToView.TryGetValue(emojiData.senderId, out BasePlayerView senderView) 
            )
            {
                EmojiItem emojiItem = Instantiate(emojiItemPrefab, senderView.transform);
                emojiItem.transform.SetParent(emojiContainer);
                emojiItem.ShowEmote(int.Parse(emojiData.emojiId));
            }
        }


        // Send emote tới người chơi khác
        else if (!string.IsNullOrEmpty(emojiData.emojiId)
            && !string.IsNullOrEmpty(emojiData.senderId)
            && !string.IsNullOrEmpty(emojiData.receiverId)
        )
        {
            Debug.Log("GUI CHO NGUOI KHAC");
            if (emojiData.senderId == emojiData.receiverId)
            {
                if (userIdToView.TryGetValue(emojiData.senderId, out BasePlayerView senderView))
                {
                    foreach(KeyValuePair<string, BasePlayerView> kvp in userIdToView)
                    {
                        if (emojiData.senderId == kvp.Key) continue;
                        EmojiItem emojiItem = Instantiate(emojiItemPrefab, senderView.transform);
                        emojiItem.transform.SetParent(emojiContainer);
                        StartCoroutine(emojiItem.SendEmojiTo(int.Parse(emojiData.emojiId), kvp.Value.transform));
                    }
                }
            }
            else if (userIdToView.TryGetValue(emojiData.senderId, out BasePlayerView senderView) && 
                userIdToView.TryGetValue(emojiData.receiverId, out BasePlayerView receiverView)
            )
            {
                EmojiItem emojiItem = Instantiate(emojiItemPrefab, senderView.transform);
                emojiItem.transform.SetParent(emojiContainer);
                StartCoroutine(emojiItem.SendEmojiTo(int.Parse(emojiData.emojiId), receiverView.transform));
            }
            
        }

    }

    protected EmojiData ConvertEmojiData(IApiChannelMessage message)
    {
        EmojiData data = JsonUtility.FromJson<EmojiData>(message.Content);
        // EmojiPayload chatPayload = new()
        // {
        //     ID = message.SenderId,
        //     Name = message.Username,
        //     Time = Utility.ConvertISOToHHMM(message.CreateTime),
        //     Content = data.content,
        //     Avatar = data.sender_profile.avt,
        //     Vip = data.sender_profile.vip_level
        // };
        Debug.Log("SENDER ID: " + data.senderId);
        Debug.Log("RECEIVER ID: " + data.receiverId);
        Debug.Log("EMOJI ID: " + data.emojiId);
        return data;
    }
    
}


public class EmojiData
{
    public string senderId;
    public string receiverId;
    public string emojiId;
}