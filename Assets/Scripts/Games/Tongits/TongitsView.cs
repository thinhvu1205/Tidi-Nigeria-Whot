using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Pool;
using Games.Card;
using Globals;
using Google.Protobuf.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yuujins.Api.V1;
using Yuujins.Match.V1;

/// <summary>
/// Tongits table UI — map <see cref="TongitsService"/> events (proto opcodes 1–6) tới hành vi tương tự client cũ
/// (HandleTongits / TongitsView): deal → cập nhật tay/lượt/nút → turn action → fight/finish/reject.
/// Gán reference trên prefab <c>TongitsView</c> (nút, container bài, TMP). Pool bài dùng chung <see cref="PrefabType.Card"/> như Baccarat.
/// </summary>
public class TongitsView : BaseTableView
{
    [Header("Tongits — cards")]
    [SerializeField] private CardModel cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private Transform discardPileContainer;
    [SerializeField] private float handCardSpacing = 72f;

    [Header("Tongits — HUD")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI drawPileCountText;
    [SerializeField] private TextMeshProUGUI turnInfoText;
    [SerializeField] private TextMeshProUGUI fightInfoText;
    [SerializeField] private TextMeshProUGUI hitPotText;

    [Header("Tongits — actions")]
    [SerializeField] private Button btnDraw;
    [SerializeField] private Button btnDiscard;
    [SerializeField] private Button btnFight;
    [SerializeField] private Button btnAcceptFight;
    [SerializeField] private Button btnDeclare;
    [SerializeField] private Button btnHaPhom;
    [SerializeField] private Button btnSendCard;
    /// <summary>Bật để chọn nhiều lá tay (Ha phỏm / Gửi bài). Nếu null, giữ Ctrl+click khi lượt mình và CanHaPhom/CanSendCard.</summary>
    [SerializeField] private Toggle toggleMeldMultiSelect;
    /// <summary>Bật để chọn nhiều lá gom phỏm khi <see cref="TongitsGameStateUpdate.CanDeclare"/>. Nếu null, dùng Alt+click.</summary>
    [SerializeField] private Toggle toggleDeclareBuildMode;
    [SerializeField] private Button btnGroup;
    [SerializeField] private Button btnClearDeclare;
    [SerializeField] private TextMeshProUGUI declareBuildInfoText;

    private readonly List<CardModel> _handPoolItems = new();
    private readonly List<CardModel> _meldSelected = new();
    /// <summary>Các nhóm phỏm nháp trước khi gửi Declare (mỗi nhóm ≥3 lá, <see cref="TongitsMeldValidator.IsGroup"/>).</summary>
    private readonly List<List<CardModel>> _declareDraftGroups = new();
    private readonly List<TongitsPlayerSnapshot> _lastSnapshots = new();
    private CardModel _selectedHandCard;
    private string _localUserId;
    private TongitsGameStateUpdate _lastGameState;
    private Card _discardTopProto;
    private bool _hasDiscardTop;

    protected override void Awake()
    {
        base.Awake();
        if (cardPrefab != null && handContainer != null)
        {
            PoolService.Instance.Register(PrefabType.Card, handContainer, cardPrefab, 16, 40, 8);
        }

        WireButtons();
    }

    private void WireButtons()
    {
        if (btnDraw != null) btnDraw.onClick.AddListener(OnClickDraw);
        if (btnDiscard != null) btnDiscard.onClick.AddListener(OnClickDiscard);
        if (btnFight != null) btnFight.onClick.AddListener(OnClickFight);
        if (btnAcceptFight != null) btnAcceptFight.onClick.AddListener(OnClickAcceptFight);
        if (btnDeclare != null) btnDeclare.onClick.AddListener(OnClickDeclare);
        if (btnHaPhom != null) btnHaPhom.onClick.AddListener(OnClickHaPhom);
        if (btnSendCard != null) btnSendCard.onClick.AddListener(OnClickSendCard);
        if (btnGroup != null) btnGroup.onClick.AddListener(OnClickGroup);
        if (btnClearDeclare != null) btnClearDeclare.onClick.AddListener(OnClickClearDeclare);
    }

    private void OnEnable()
    {
        _localUserId = User.Profile != null ? User.Profile.UserId : "";
        TongitsService.OnGameStateUpdate += HandleGameState;
        TongitsService.OnDealUpdate += HandleDeal;
        TongitsService.OnTurnActionUpdate += HandleTurnAction;
        TongitsService.OnFightStateUpdate += HandleFight;
        TongitsService.OnFinishUpdate += HandleFinish;
        TongitsService.OnReject += HandleReject;
        TongitsService.OnDispatchError += HandleDispatchError;
    }

    protected override void OnDestroy()
    {
        TongitsService.OnGameStateUpdate -= HandleGameState;
        TongitsService.OnDealUpdate -= HandleDeal;
        TongitsService.OnTurnActionUpdate -= HandleTurnAction;
        TongitsService.OnFightStateUpdate -= HandleFight;
        TongitsService.OnFinishUpdate -= HandleFinish;
        TongitsService.OnReject -= HandleReject;
        TongitsService.OnDispatchError -= HandleDispatchError;

        ReleaseHandCards();
        // Không gọi ClearPool(Card): pool chung với Baccarat / game khác (DontDestroyOnLoad).

        base.OnDestroy();
    }

    public override void LoadInfoMatch(MatchInfo match)
    {
        base.LoadInfoMatch(match);
        ReleaseHandCards();
        ClearDiscardPileVisualAndState();
        _lastGameState = null;
        SetFightInfo("");
        if (statusText != null) statusText.text = "";
        if (turnInfoText != null) turnInfoText.text = "";
        ApplyButtonInteractableAll(false);
    }

    #region TongitsService handlers (server → UI)

    private void  HandleGameState(TongitsGameStateUpdate msg)
    {
        _lastGameState = msg;
        if (statusText != null)
            statusText.text = $"Phase: {msg.Phase} · Round {msg.Round}";

        if (hitPotText != null)
            hitPotText.text = msg.HitPot > 0 ? $"Hit pot: {msg.HitPot}" : "";

        if (drawPileCountText != null)
            drawPileCountText.text = msg.DrawPileCount.ToString();

        if (turnInfoText != null)
        {
            var name = ResolveDisplayName(msg.CurrentUserId);
            turnInfoText.text = string.IsNullOrEmpty(msg.CurrentUserId)
                ? "—"
                : $"{name} · {msg.CountdownSec}s";
        }

        bool isMyTurn = !string.IsNullOrEmpty(_localUserId) && msg.CurrentUserId == _localUserId;
        if (btnDraw != null) btnDraw.interactable = isMyTurn && msg.CanDraw;
        if (btnDiscard != null) btnDiscard.interactable = isMyTurn && msg.CanDiscard && _selectedHandCard != null;
        if (btnFight != null) btnFight.interactable = isMyTurn && msg.CanFight;
        if (btnAcceptFight != null) btnAcceptFight.interactable = isMyTurn && msg.CanAcceptFight;
        if (btnDeclare != null) btnDeclare.interactable = isMyTurn && msg.CanDeclare;
        if (btnHaPhom != null) btnHaPhom.interactable = isMyTurn && msg.CanHaPhom;
        if (btnSendCard != null) btnSendCard.interactable = isMyTurn && msg.CanSendCard;
        if (btnGroup != null) btnGroup.interactable = isMyTurn && msg.CanDeclare;
        if (btnClearDeclare != null) btnClearDeclare.interactable = isMyTurn && msg.CanDeclare && _declareDraftGroups.Count > 0;

        RefreshDeclareBuildInfoText();
    }

    private void HandleDeal(TongitsDealUpdate msg)
    {
        ReleaseHandCards();
        ClearDiscardVisual();
        ApplySnapshots(msg.Players, msg.BocCard, msg.DrawPileCount);

        if (drawPileCountText != null)
            drawPileCountText.text = msg.DrawPileCount.ToString();

        if (turnInfoText != null)
        {
            var name = ResolveDisplayName(msg.FirstTurnUserId);
            turnInfoText.text = $"First: {name}";
        }

        if (_lastGameState != null)
            HandleGameState(_lastGameState);
    }

    private void HandleTurnAction(TongitsTurnActionUpdate msg)
    {
        if (discardPileContainer != null)
        {
            if (msg.DiscardCard != null)
                ShowDiscardTop(msg.DiscardCard, trackForHaPhom: true);
            else if (string.Equals(msg.ActionType, "ha_phom", System.StringComparison.Ordinal))
                ClearDiscardPileVisualAndState();
        }

        ApplySnapshots(msg.Players, null, msg.DrawPileCount);

        if (drawPileCountText != null)
            drawPileCountText.text = msg.DrawPileCount.ToString();

        if (turnInfoText != null)
        {
            var nextName = ResolveDisplayName(msg.NextTurnUserId);
            var act = string.IsNullOrEmpty(msg.ActionType) ? "—" : msg.ActionType;
            turnInfoText.text = $"{act} · next: {nextName}";
        }

        if (_lastGameState != null)
            HandleGameState(_lastGameState);
    }

    private void HandleFight(TongitsFightUpdate msg)
    {
        var sb = new StringBuilder();
        sb.Append("Fight · declarer: ").Append(ResolveDisplayName(msg.DeclarerUserId));
        if (msg.AcceptUserIds != null && msg.AcceptUserIds.Count > 0)
            sb.Append(" · accept: ").Append(string.Join(", ", msg.AcceptUserIds.Select(ResolveDisplayName)));
        if (msg.RefuseUserIds != null && msg.RefuseUserIds.Count > 0)
            sb.Append(" · refuse: ").Append(string.Join(", ", msg.RefuseUserIds.Select(ResolveDisplayName)));
        if (msg.FightResolved)
            sb.Append(" · resolved");
        SetFightInfo(sb.ToString());
    }

    private void HandleFinish(TongitsFinishUpdate msg)
    {
        ApplySnapshots(msg.Players, null, 0);

        var sb = new StringBuilder();
        sb.AppendLine("Ván xong");
        foreach (var p in msg.Players)
        {
            sb.AppendLine($"{ResolveDisplayName(p.UserId)} · score {p.Score}");
        }
        if (msg.BalanceDelta != null && msg.BalanceDelta.Count > 0)
        {
            sb.AppendLine("Chip Δ:");
            foreach (var kv in msg.BalanceDelta)
                sb.AppendLine($"  {ResolveDisplayName(kv.Key)}: {kv.Value}");
        }
        if (msg.HitPot != 0)
            sb.Append("Hit pot: ").Append(msg.HitPot);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowAlertDialog(sb.ToString());
    }

    private void HandleReject(TongitsReject msg)
    {
        var line = $"{msg.Reason}: {msg.Message}";
        if (UIManager.Instance != null)
            UIManager.Instance.ShowToast(line, 2f, transform);
    }

    private void HandleDispatchError(long op, System.Exception ex)
    {
        Debug.LogWarning($"[TongitsView] dispatch op={op} err={ex.Message}");
    }

    #endregion

    #region Snapshots → hand / opponent counts (giống lc / bc / dc / viewTable tóm tắt)

    private void ApplySnapshots(
        RepeatedField<TongitsPlayerSnapshot> players,
        Card bocCard,
        int drawPileCount)
    {
        if (players == null) return;

        _lastSnapshots.Clear();
        foreach (var snap in players)
            _lastSnapshots.Add(snap.Clone());

        foreach (var snap in _lastSnapshots)
        {
            bool isLocal = !string.IsNullOrEmpty(_localUserId) && snap.UserId == _localUserId;
            if (isLocal)
            {
                RebuildLocalHand(snap.HandCards);
            }
            else
            {
                int hiddenCount = snap.HiddenHandCount > 0
                    ? snap.HiddenHandCount
                    : (snap.HandCards != null ? snap.HandCards.Count : 0);
                var pv = FindPlayerView(snap.UserId) as TongitsPlayerView;
                if (pv != null)
                    pv.SetHiddenHandCount(hiddenCount);
            }
        }

        if (drawPileCountText != null)
            drawPileCountText.text = drawPileCount.ToString();

        // Bài cái — không gắn vào lá “chồng rác” dùng cho Ha phỏm (server DiscardCard lúc deal = null).
        if (bocCard != null && discardPileContainer != null)
            ShowDiscardTop(bocCard, trackForHaPhom: false);
    }

    private void RebuildLocalHand(RepeatedField<Card> handCards)
    {
        ReleaseHandCards();
        _selectedHandCard = null;
        if (handContainer == null || handCards == null) return;

        for (int i = 0; i < handCards.Count; i++)
        {
            var proto = handCards[i];
            var cm = PoolService.Instance.Get<CardModel>(PrefabType.Card);
            if (cm == null) continue;
            var rt = cm.transform as RectTransform;
            if (rt != null)
            {
                rt.SetParent(handContainer, false);
                rt.localScale = Vector3.one;
            }
            else
                cm.transform.SetParent(handContainer, false);

            cm.SetData((int)proto.Rank, ProtoSuitToCardModelSuit(proto.Suit));
            cm.ShowCard();
            AddCardButton(cm);
            _handPoolItems.Add(cm);
        }
        LayoutHand();
        RefreshDiscardButtonState();
    }

    private static int ProtoSuitToCardModelSuit(int protoSuit0To3) => (int)protoSuit0To3 + 1;

    private void AddCardButton(CardModel cm)
    {
        var btn = cm.GetComponent<Button>();
        if (btn == null)
            btn = cm.gameObject.AddComponent<Button>();
        if (cm.imgBackground != null)
            btn.targetGraphic = cm.imgBackground;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnHandCardSelected(cm));
    }

    private void OnHandCardSelected(CardModel cm)
    {
        if (cm == null) return;

        if (IsDeclareGroupPickMode())
        {
            if (IsCardInDeclareDraft(cm))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowToast("Lá đã nằm trong phỏm nháp — bấm Clear declare để xóa nhóm.", 2f, transform);
                return;
            }
            ToggleMeldSelection(cm);
            RefreshDiscardButtonState();
            return;
        }

        if (IsHaPhomOrSendPickMode())
        {
            ToggleMeldSelection(cm);
            RefreshDiscardButtonState();
            return;
        }

        ClearMeldSelection();
        if (_selectedHandCard != null)
            _selectedHandCard.SetBorder(false);
        _selectedHandCard = cm;
        _selectedHandCard.SetBorder(true);
        RefreshDiscardButtonState();
    }

    private bool IsMyPlayTurn() =>
        _lastGameState != null
        && !string.IsNullOrEmpty(_localUserId)
        && _lastGameState.CurrentUserId == _localUserId;

    private bool IsDeclareGroupPickMode()
    {
        if (!IsMyPlayTurn() || _lastGameState == null || !_lastGameState.CanDeclare)
            return false;
        if (toggleDeclareBuildMode != null && toggleDeclareBuildMode.isOn)
            return true;
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }

    private bool IsHaPhomOrSendPickMode()
    {
        if (!IsMyPlayTurn() || _lastGameState == null)
            return false;
        if (!_lastGameState.CanHaPhom && !_lastGameState.CanSendCard)
            return false;
        if (toggleMeldMultiSelect != null && toggleMeldMultiSelect.isOn)
            return true;
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    private bool IsCardInDeclareDraft(CardModel cm)
    {
        if (cm == null) return false;
        foreach (var g in _declareDraftGroups)
        {
            if (g.Contains(cm))
                return true;
        }
        return false;
    }

    private void ToggleMeldSelection(CardModel cm)
    {
        int idx = _meldSelected.IndexOf(cm);
        if (idx >= 0)
        {
            _meldSelected.RemoveAt(idx);
            cm.SetBorder(false);
            if (_selectedHandCard == cm)
                _selectedHandCard = null;
        }
        else
        {
            _meldSelected.Add(cm);
            cm.SetBorder(true);
        }
    }

    private void ClearMeldSelection()
    {
        foreach (var cm in _meldSelected)
        {
            if (cm != null)
                cm.SetBorder(false);
        }
        _meldSelected.Clear();
    }

    private static Card CardModelToProto(CardModel cm)
    {
        if (cm?.data == null)
            return new Card();
        return new Card { Rank = cm.data.rank, Suit = cm.data.suit - 1 };
    }

    private void LayoutHand()
    {
        float startX = -(_handPoolItems.Count - 1) * handCardSpacing * 0.5f;
        for (int i = 0; i < _handPoolItems.Count; i++)
        {
            var rt = _handPoolItems[i].transform as RectTransform;
            if (rt == null) continue;
            rt.anchoredPosition = new Vector2(startX + i * handCardSpacing, 0f);
        }
    }

    private void ReleaseHandCards()
    {
        ClearDeclareDraft();
        foreach (var cm in _handPoolItems)
        {
            if (cm == null) continue;
            var btn = cm.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveAllListeners();
            PoolService.Instance.Release(PrefabType.Card, cm);
        }
        _handPoolItems.Clear();
        _selectedHandCard = null;
        ClearMeldSelection();
    }

    private void ShowDiscardTop(Card proto, bool trackForHaPhom)
    {
        if (discardPileContainer == null) return;
        ClearDiscardVisual();
        if (trackForHaPhom)
        {
            _discardTopProto = proto.Clone();
            _hasDiscardTop = true;
        }
        else
        {
            _discardTopProto = null;
            _hasDiscardTop = false;
        }
        var cm = PoolService.Instance.Get<CardModel>(PrefabType.Card);
        if (cm == null) return;
        var rt = cm.transform as RectTransform;
        if (rt != null)
        {
            rt.SetParent(discardPileContainer, false);
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }
        else
            cm.transform.SetParent(discardPileContainer, false);
        cm.SetData((int)proto.Rank, ProtoSuitToCardModelSuit(proto.Suit));
        cm.ShowCard();
        cm.gameObject.name = "DiscardTop";
    }

    private void ClearDiscardVisual()
    {
        if (discardPileContainer == null) return;
        for (int i = discardPileContainer.childCount - 1; i >= 0; i--)
        {
            var ch = discardPileContainer.GetChild(i).GetComponent<CardModel>();
            if (ch != null)
                PoolService.Instance.Release(PrefabType.Card, ch);
            else
                Destroy(discardPileContainer.GetChild(i).gameObject);
        }
    }

    private void ClearDiscardPileVisualAndState()
    {
        ClearDiscardVisual();
        _discardTopProto = null;
        _hasDiscardTop = false;
    }

    private BasePlayerView FindPlayerView(string userId)
    {
        if (string.IsNullOrEmpty(userId) || userIdToView == null) return null;
        return userIdToView.TryGetValue(userId, out var v) ? v : null;
    }

    private string ResolveDisplayName(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return "—";
        if (userId == _localUserId) return "You";
        if (playerStates != null && playerStates.TryGetValue(userId, out var p) && p != null && !string.IsNullOrEmpty(p.UserName))
            return p.UserName;
        return userId.Length > 6 ? userId.Substring(0, 6) + "…" : userId;
    }

    private void SetFightInfo(string text)
    {
        if (fightInfoText == null) return;
        fightInfoText.gameObject.SetActive(!string.IsNullOrEmpty(text));
        fightInfoText.text = text;
    }

    private void ApplyButtonInteractableAll(bool on)
    {
        if (btnDraw != null) btnDraw.interactable = on;
        if (btnDiscard != null) btnDiscard.interactable = on;
        if (btnFight != null) btnFight.interactable = on;
        if (btnAcceptFight != null) btnAcceptFight.interactable = on;
        if (btnDeclare != null) btnDeclare.interactable = on;
        if (btnHaPhom != null) btnHaPhom.interactable = on;
        if (btnSendCard != null) btnSendCard.interactable = on;
        if (btnGroup != null) btnGroup.interactable = on;
        if (btnClearDeclare != null) btnClearDeclare.interactable = on && _declareDraftGroups.Count > 0;
    }

    private void RefreshDiscardButtonState()
    {
        if (btnDiscard == null) return;
        bool isMyTurn = _lastGameState != null && _lastGameState.CurrentUserId == _localUserId;
        btnDiscard.interactable = isMyTurn && _lastGameState != null && _lastGameState.CanDiscard && _selectedHandCard != null;
    }

    #endregion

    #region Client → server (tương ứng sendTgBc / sendTgDc / …)

    private void OnClickDraw()
    {
        TongitsService.SendDrawCard();
    }

    private void OnClickDiscard()
    {
        if (_selectedHandCard == null || _selectedHandCard.data == null)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast("Chọn một lá để đánh", 1.5f, transform);
            return;
        }
        TongitsService.SendDiscard(_selectedHandCard.data.rank, _selectedHandCard.data.suit - 1);
    }

    private void OnClickFight()
    {
        TongitsService.SendFight();
    }

    private void OnClickAcceptFight()
    {
        TongitsService.SendAcceptFight();
    }

    private void OnClickDeclare()
    {
        if (_lastGameState == null || !_lastGameState.CanDeclare)
            return;

        if (_declareDraftGroups.Count == 0)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast(
                    "Tongits: bật Toggle declare hoặc giữ Alt + chọn lá, bấm GROUP cho từng phỏm, rồi DECLARE khi đã gom hết bài.",
                    3.5f,
                    transform);
            return;
        }

        var covered = new HashSet<CardModel>();
        foreach (var g in _declareDraftGroups)
        {
            foreach (var c in g)
            {
                if (c == null || !c.gameObject.activeInHierarchy)
                    continue;
                if (!covered.Add(c))
                {
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowToast("Trùng lá trong nhiều phỏm nháp.", 2f, transform);
                    return;
                }
            }
        }

        if (covered.Count != _handPoolItems.Count)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast(
                    $"Phải gom hết bài tay ({_handPoolItems.Count} lá) thành phỏm. Đã gom: {covered.Count} lá.",
                    2.5f,
                    transform);
            return;
        }

        foreach (var g in _declareDraftGroups)
        {
            if (!TongitsMeldValidator.IsGroup(g))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowToast("Có nhóm không phải phỏm hợp lệ (sảnh cùng chất hoặc bộ cùng số, ≥3 lá).", 2.5f, transform);
                return;
            }
        }

        var req = new TongitsDeclareRequest();
        foreach (var g in _declareDraftGroups)
        {
            var dg = new TongitsDeclareGroup();
            foreach (var cm in g)
                dg.Cards.Add(CardModelToProto(cm));
            req.Groups.Add(dg);
        }

        TongitsService.SendDeclare(req);
        // Giữ nháp nếu server reject; xóa khi chia bài mới (ReleaseHandCards).
    }

    private void OnClickGroup()
    {
        if (_lastGameState == null || !_lastGameState.CanDeclare)
            return;

        if (_meldSelected.Count < 3)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast("Mỗi phỏm cần ít nhất 3 lá.", 1.5f, transform);
            return;
        }

        if (!TongitsMeldValidator.IsGroup(_meldSelected))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast("Nhóm đã chọn không phải phỏm hợp lệ.", 2f, transform);
            return;
        }

        foreach (var cm in _meldSelected)
        {
            if (IsCardInDeclareDraft(cm))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowToast("Một lá đã thuộc phỏm khác.", 1.5f, transform);
                return;
            }
        }

        _declareDraftGroups.Add(new List<CardModel>(_meldSelected));
        foreach (var cm in _meldSelected)
            cm.SetBorder(false);
        _meldSelected.Clear();
        _selectedHandCard = null;
        RefreshDiscardButtonState();
        RefreshDeclareBuildInfoText();
        if (_lastGameState != null)
            HandleGameState(_lastGameState);
    }

    private void OnClickClearDeclare()
    {
        ClearDeclareDraft();
        ClearMeldSelection();
        if (_lastGameState != null)
            HandleGameState(_lastGameState);
    }

    private void ClearDeclareDraft()
    {
        foreach (var g in _declareDraftGroups)
        {
            foreach (var cm in g)
            {
                if (cm != null)
                    cm.SetBorder(false);
            }
        }
        _declareDraftGroups.Clear();
        RefreshDeclareBuildInfoText();
    }

    private void RefreshDeclareBuildInfoText()
    {
        if (declareBuildInfoText == null)
            return;
        if (_lastGameState == null || !_lastGameState.CanDeclare)
        {
            declareBuildInfoText.text = "";
            return;
        }
        int inDraft = 0;
        foreach (var g in _declareDraftGroups)
            inDraft += g.Count;
        int rest = _handPoolItems.Count - inDraft;
        declareBuildInfoText.text = $"Declare: {_declareDraftGroups.Count} phỏm · đã gom {inDraft}/{_handPoolItems.Count} lá · còn {rest}";
    }

    private void OnClickHaPhom()
    {
        if (!_hasDiscardTop || _discardTopProto == null)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast("Không có lá trên chồng rác để hạ phỏm.", 2f, transform);
            return;
        }

        if (_meldSelected.Count < 2)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast("Chọn ít nhất 2 lá trên tay (bật Toggle chọn phỏm hoặc Ctrl+click).", 2.5f, transform);
            return;
        }

        var req = new TongitsHaPhomRequest { DiscardCard = _discardTopProto.Clone() };
        foreach (var cm in _meldSelected)
            req.MeldCards.Add(CardModelToProto(cm));

        TongitsService.SendHaPhom(req);
        ClearMeldSelection();
    }

    private void OnClickSendCard()
    {
        if (_meldSelected.Count < 1)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast("Chọn ít nhất 1 lá trên tay để gửi (Toggle/Ctrl+click).", 2f, transform);
            return;
        }

        TongitsPlayerSnapshot target = null;
        int meldIdx = 0;
        foreach (var snap in _lastSnapshots.OrderBy(s => s.SeatIndex))
        {
            if (snap.UserId == _localUserId)
                continue;
            if (snap.Melds != null && snap.Melds.Count > 0)
            {
                target = snap;
                meldIdx = 0;
                break;
            }
        }

        if (target == null)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowToast("Không thấy phỏm trên bàn đối thủ để gửi vào.", 2f, transform);
            return;
        }

        var req = new TongitsSendCardRequest
        {
            TargetPlayerIndex = target.SeatIndex,
            TargetMeldIndex = meldIdx
        };
        foreach (var cm in _meldSelected)
            req.Cards.Add(CardModelToProto(cm));

        TongitsService.SendSendCard(req);
        ClearMeldSelection();
    }

    #endregion
}
