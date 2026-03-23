using System;
using System.Collections.Generic;
using System.Linq;
using Globals;
using Nakama;
using Newtonsoft.Json;
using Proto;
using TMPro;
using UnityEngine;
using Yuujins.Api.V1;
using Yuujins.Match.V1;
using Player = Yuujins.Api.V1.Player;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BaseTableView : BaseGameView
{
    [SerializeField] private TextMeshProUGUI textMatchInfo, textGameName;
    [SerializeField] protected List<Vector2> listPosView;
    [SerializeField] protected BasePlayerView playerViewPrefab;
    [SerializeField] protected GameObject invitePrefab;
    [SerializeField] protected Transform inviteContainer, playerContainer, hiddenPlayerContainer, emojiContainer;
    [SerializeField] protected EmojiItem emojiItemPrefab;
    public int MarkUnit { get; private set; }

    protected Dictionary<string, BasePlayerView> userIdToView;
    protected Dictionary<string, Player> playerStates;
    protected BasePlayerView currentPlayerView;
    protected List<Player> players = new List<Player>();
    protected List<Player> rearrangedPlayers = new List<Player>();
    [SerializeField] protected List<Player> playingPlayers = new List<Player>();
    protected List<GameObject> listBtnInvite = new List<GameObject>();
    protected long currentRevision = 0;
    /// <summary>Đã gửi opcode 101 — chờ bản roster <c>is_snapshot</c> tiếp theo, bỏ qua check revision kỳ vọng tuần tự.</summary>
    private bool rosterAwaitingResync;
    private float interactTimer = 0, interactCountdown = 2;
    private ChatInGameView chatInGameView;
    private EmojiInGameView emojiInGameView;


    protected override void Start()
    {
        userIdToView ??= new Dictionary<string, BasePlayerView>();
        playerStates ??= new Dictionary<string, Player>();
        base.Start();
        MatchRosterService.OnRosterUpdate += OnRosterUpdate;
        // chatInGameView = UIManager.Instance.OpenChatInGame();
        // chatInGameView.Init();
        // emojiInGameView = UIManager.Instance.OpenEmojiInGame();
        // NetworkManager.INSTANCE.OnMessageTableReceived += NetworkManager_OnMessageTableReceived;
    }

    protected override void OnDestroy()
    {
        MatchRosterService.OnRosterUpdate -= OnRosterUpdate;
        base.OnDestroy();
        // Destroy(chatInGameView.gameObject);
        // Destroy(emojiInGameView.gameObject);
        // NetworkManager.INSTANCE.OnMessageTableReceived -= NetworkManager_OnMessageTableReceived;
    }

    protected override void Update()
    {
        interactTimer -= Time.unscaledDeltaTime;
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && interactTimer <= 0)
        {
            interactTimer = interactCountdown;
            // DataSender.SendMatchState((long)OpCodeRequest.OpcodeUserInteractCards, Array.Empty<byte>());
        }
    }

    #region SYNC

    protected virtual void RequestSnapshot()
    {
        Debug.Log("Requesting roster snapshot (opcode 101) due to revision gap...");
        MatchRosterService.RequestRosterSnapshot();
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
        RequestSnapshot();
        rosterAwaitingResync = true;
    }

    
    #endregion
    

    private void OnRosterUpdate(MatchRosterUpdate roster)
    {
        Debug.Log("roster update received "+ roster.ToString());

        if (rosterAwaitingResync)
        {
            if (!roster.IsSnapshot) return;
            ApplySnapshot(roster);
            currentRevision = roster.Revision;
            rosterAwaitingResync = false;
            return;
        }

        if (roster.Revision <= currentRevision) return;

        if (roster.Revision > currentRevision + 1)
        {
            RequestSnapshot();
            rosterAwaitingResync = true;
            return;
        }

        if (roster.IsSnapshot)
        {
            ApplySnapshot(roster);
        }
        else
        {
            ApplyDelta(roster);
        }

        currentRevision = roster.Revision;
    }
    
    #region SNAPSHOT

    private void ApplySnapshot(MatchRosterUpdate roster)
    {
        ClearAll();

        foreach (var p in roster.Players)
        {
            playerStates[p.Id] = p;
            CreateOrUpdateView(p);
        }

        RefreshPositions();
    }

    #endregion

    #region DELTA

    private void ApplyDelta(MatchRosterUpdate roster)
    {
        foreach (var p in roster.LeftPlayers)
        {
            if (p == null || string.IsNullOrEmpty(p.Id)) continue;
            RemovePlayer(p.Id);
        }

        foreach (var p in roster.JoinedPlayers)
        {
            if (p == null || string.IsNullOrEmpty(p.Id)) continue;
            playerStates[p.Id] = p;
            CreateOrUpdateView(p);
        }

        RefreshPositions();
    }

    #endregion
    
    #region PLAYER VIEW

    private void CreateOrUpdateView(Player p)
    {
        if (!userIdToView.TryGetValue(p.Id, out var view) || view == null)
        {
            view = Instantiate(playerViewPrefab, playerContainer);
            userIdToView[p.Id] = view;
        }

        view.SetData(p);
    }

    private void RemovePlayer(string userId)
    {
        if (userIdToView.TryGetValue(userId, out var view))
        {
            Destroy(view.gameObject);
            userIdToView.Remove(userId);
        }

        playerStates.Remove(userId);
    }

    private void ClearAll()
    {
        foreach (var v in userIdToView.Values)
        {
            if (v != null) Destroy(v.gameObject);
        }

        userIdToView.Clear();
        playerStates.Clear();
    }

    #endregion

    #region POSITIONING

    private void RefreshPositions()
    {
        var players = playerStates.Values
            .OrderBy(p => p.SeatIndex)
            .ToList();

        var localId = User.Profile.UserId;

        int localIndex = players.FindIndex(p => p.Id == localId);
        if (localIndex > 0)
        {
            players = Rotate(players, localIndex);
        }

        for (int i = 0; i < players.Count && i < listPosView.Count; i++)
        {
            var p = players[i];
            if (userIdToView.TryGetValue(p.Id, out var view))
            {
                SetPosition(view, listPosView[i]);
            }
        }
    }

    private List<Player> Rotate(List<Player> list, int startIndex)
    {
        List<Player> result = new();
        for (int i = 0; i < list.Count; i++)
        {
            result.Add(list[(startIndex + i) % list.Count]);
        }
        return result;
    }

    private void SetPosition(BasePlayerView view, Vector2 pos)
    {
        if (view.transform is RectTransform rt)
        {
            rt.anchoredPosition = pos;
        }
        else
        {
            view.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        }
    }
    
    #endregion
    

    public override void LoadInfoMatch(MatchInfo match)
    {
        base.LoadInfoMatch(match);
        if (!Constants.SELECT_TABLE_GAMES_ID.Contains(Config.currentGameName)) return;
        userIdToView ??= new Dictionary<string, BasePlayerView>();
        MarkUnit = (int) match.MarkUnit;
        WantSwitchTable = false;
        currentRevision = 0;
        rosterAwaitingResync = false;
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
        
    }
    
    // protected virtual void UpdatePosUserTable(UpdateTable update, bool isRearrange = false)
    // {
    //     // Debug.Log("UpdatePosUserTable: " + update.ToString());
    //     var localUserId = User.Profile.UserId;
    //     if (listPosView == null || listPosView.Count == 0 || playerViewPrefab == null || localUserId == "") return;
    //
    //     // 1) Cập nhật danh sách playing players
    //     if (update.PlayingPlayers.ToList().Count != 0)
    //     {
    //         playingPlayers = update.PlayingPlayers.ToList();
    //         // Debug.Log($"Playing players updated: {string.Join(", ", playingPlayers.Select(p => p.UserName))}");
    //     }
    //
    //     // 2) Cập nhật danh sách players
    //     players = update.Players.ToList();
    //
    //     // Sắp xếp lại sao cho local player luôn ở vị trí đầu tiên
    //     if (isRearrange)
    //     {
    //         Player currentPlayer = players.Find((player) => player.Id == localUserId);
    //         int startIndex = players.IndexOf(currentPlayer);
    //
    //         if (startIndex >= 0)
    //         {
    //             List<Player> reordered = new();
    //
    //             for (int i = 0; i < players.Count; i++)
    //             {
    //                 int index = (startIndex + i) % players.Count;
    //                 reordered.Add(players[index]);
    //             }
    //             // players = reordered;
    //             rearrangedPlayers = new(reordered);
    //         }
    //
    //     }
    //     else
    //     {
    //         rearrangedPlayers = new(players);
    //     }
    //
    //     // 3) Xử lý players leave
    //     foreach (var lp in update.LeavePlayers)
    //     {
    //         // Debug.Log($"Player {lp.UserName} left the table");
    //         if (userIdToView.TryGetValue(lp.Id, out var view))
    //         {
    //             SoundManager.Instance.PlayEffectFromPath(Sound.REMOVE);
    //             Destroy(view.gameObject);
    //             RemovePlayerBoxBet(lp.Id);
    //             userIdToView.Remove(lp.Id);
    //         }
    //     }
    //
    //     // 4) Xử lý players join
    //     foreach (var jp in update.JoinPlayers)
    //     {
    //         // Debug.Log($"Player {jp.UserName} joined the table");
    //     }
    //     
    //     // 5) Tạo/update player views theo danh sách players mới
    //     var localInPlayers = players.Exists(p => p.Id == localUserId);
    //
    //     // Gán vị trí 0 cho local player (nếu có)
    //     int positionIndex = 0;
    //     if (localInPlayers)
    //     {
    //         var local = players.Find(p => p.Id == localUserId);
    //         CreatePlayerView(local, listPosView[0]);
    //         positionIndex = 1;
    //     }
    //     else
    //     {
    //         // Nếu local không nằm trong players, giữ view local nếu đang có ở vị trí 0
    //         if (userIdToView.TryGetValue(localUserId, out var localView))
    //         {
    //             SetAnchoredPosition(localView, listPosView[0]);
    //             positionIndex = 1;
    //         }
    //     }
    //
    //     // Gán các vị trí tiếp theo theo thứ tự trong update.players, bỏ qua local
    //     for (int i = 0; i < rearrangedPlayers.Count && positionIndex < listPosView.Count; i++)
    //     {
    //         var p = rearrangedPlayers[i];
    //         if (p.Id == localUserId) continue;
    //         CreatePlayerView(p, listPosView[positionIndex]);
    //         positionIndex++;
    //     } 
    //     
    //     if (currentPlayerView == userIdToView.GetValueOrDefault(localUserId)) return;
    //
    //     // 6) Update UI current Player
    //     currentPlayerView = userIdToView.GetValueOrDefault(localUserId);
    //     if (currentPlayerView != null)
    //     {
    //         currentPlayerView.SetPositionInfoThisPlayer();
    //     }
    // }
    //
    // protected virtual void CreatePlayerView(Player player, Vector2 anchoredPos)
    // {
    //     if (!userIdToView.TryGetValue(player.Id, out var view) || view == null)
    //     {
    //         view = Instantiate(playerViewPrefab, playerContainer);
    //         userIdToView[player.Id] = view;
    //         view.gameObject.SetActive(true);
    //     }
    //
    //     // Luôn cập nhật data và vị trí kể cả khi view đã tồn tại
    //     view.SetData(player);
    //     SetAnchoredPosition(view, anchoredPos);
    // }
    //
    // private void SetAnchoredPosition(BasePlayerView view, Vector2 anchoredPos)
    // {
    //     var rt = view.transform as RectTransform;
    //     if (rt != null) rt.anchoredPosition = anchoredPos;
    //     else view.transform.localPosition = new Vector3(anchoredPos.x, anchoredPos.y, 0f);
    // }

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
        
        // Chat
        if (string.IsNullOrEmpty(emojiData.emojiId))
        {
            BasePlayerView player = userIdToView[message.SenderId];
            if (player != null)
            {
                ChatPayload chatPayload = ConvertToChatPayload(message);
                if (chatPayload.IsAudio)
                {
                    player.ShowBubbleChat("Sent a voice message");
                }
                else
                {
                    player.ShowBubbleChat(chatPayload.Content);                    
                }
            }
        }

        // Emoji
        if (!string.IsNullOrEmpty(emojiData.emojiId)
            && !string.IsNullOrEmpty(emojiData.senderId)
            && string.IsNullOrEmpty(emojiData.receiverId)
        )
        {
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
        return data;
    }

    protected ChatPayload ConvertToChatPayload(IApiChannelMessage message)
    {
        ChatPayload chatPayload = new ChatPayload();

        try
        {
            // Parse message content (JSON string từ server)
            if (!string.IsNullOrEmpty(message.Content))
            {
                // Parse JSON content
                var contentData = JsonConvert.DeserializeObject<ChatContentData>(message.Content);

                if (contentData != null)
                {
                    // 1. Check voice message first
                    if (!string.IsNullOrEmpty(contentData.voice_url))
                    {
                        chatPayload.IsAudio = true;
                        chatPayload.Content = contentData.voice_url; 
                    //    _ = Test(contentData.voice_url);
                    }
                    else
                    {
                        // 2. Text message
                        chatPayload.IsAudio = false;
                        chatPayload.Content = contentData.text ?? "";
                    }

                    // 3. Sender info từ sender_profile (server tự thêm)
                    if (contentData.sender_profile != null)
                    {
                        chatPayload.Name = message.Username; // Fallback to message.Username
                        chatPayload.Avatar = contentData.sender_profile.avt ?? "";
                        chatPayload.Vip = (int) contentData.sender_profile.vip_level;
                        chatPayload.Time = Utility.ConvertISOToHHMM(message.CreateTime);
                    }
                    else
                    {
                        chatPayload.Name = message.Username;
                    }

                    // 4. Sender ID
                    chatPayload.ID = message.SenderId;
                    
                    
                }
                else
                {
                    // Fallback: treat as plain text if JSON parse fails
                    chatPayload.Content = message.Content;
                    chatPayload.IsAudio = false;
                }
            }
            else
            {
                // Empty content
                chatPayload.IsAudio = false;
            }
        }
        catch (Exception e)
        {
            // Fallback to basic info
            chatPayload.Content = message.Content ?? "";
            chatPayload.IsAudio = false;
        }

        return chatPayload;
    }
    
}


public class EmojiData
{
    public string isEmoji;
    public string senderId;
    public string receiverId;
    public string emojiId;
}