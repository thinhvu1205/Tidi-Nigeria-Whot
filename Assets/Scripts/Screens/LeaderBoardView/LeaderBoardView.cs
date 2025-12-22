using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Newtonsoft.Json.Linq;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LeaderBoardTab;
using Avatar = Common.Objects.Avatar;
public class LeaderBoardView : BaseView
{
    [SerializeField] private GameObject leaderBoardItemPrefab, leaderBoardTabPrefab;
    [SerializeField] private Transform leaderBoardItemParent, leaderBoardTabParent;

    [Header("Current User")] [SerializeField]
    private Avatar currentUserAvatarImage;
    [SerializeField] private Image topImage;
    [SerializeField] private TextMeshProUGUI currentUserNameText, currentUserTopText, currentUserChipValueText;
    [SerializeField] private List<Sprite> topSprites = new();

    private List<LeaderBoardItem> listLeaderboardItem = new();
    private List<LeaderBoardTab> listLeaderboardTab = new();
    private LeaderBoardTab selectedTab;
    private LeaderboardPresenter leaderboardPresenter;
    private List<Game> gameList = new();
    private List<IApiLeaderboardRecord> recordList = new();
    private IApiLeaderboardRecord currentUserRecord;
    private string currentTabGameCode = "";

    protected override void Awake()
    {
        base.Awake();
        leaderboardPresenter = new LeaderboardPresenter();
        leaderboardPresenter.Init(this);
    }

    protected override void Start()
    {
        base.Start();
        _ = InitData();
       
    }

    private async UniTask InitData()
    {
        await LoadListGame();
        // await LoadListLeaderBoard();
    }
    
    #region Data
    private async UniTask LoadListGame()
    {
        gameList.Clear();
        try
        {
            GameListResponse gameListResponse = await leaderboardPresenter.LoadGameList();
            Debug.Log("Game List : " + gameListResponse.ToString());
            gameList = gameListResponse.Games.ToList();
            UpdateUIListGame();
        }
        catch (Exception ex)
        {
            Debug.Log("err load list game : " + ex.Message);
            // throw;
        }
    }

    private async UniTask LoadListLeaderBoard()
    {
        Debug.Log("get list record game "+ currentTabGameCode);

        IApiLeaderboardRecordList apiLeaderboardRecordList = await leaderboardPresenter.LoadList(currentTabGameCode, User.userProfile.UserId);
        
        currentUserRecord = apiLeaderboardRecordList.OwnerRecords.FirstOrDefault();
        if (currentUserRecord != null)
        {
            var json = JObject.Parse(currentUserRecord.Metadata);
            string avatarId = json["avatar_id"]?.ToString() ?? "";
            long vipLevel = json["vip_level"]?.Value<long>() ?? 0;
            Debug.Log("Info current user record " + avatarId + " : " + vipLevel);
            UpdateUserLeaderboardUI(currentUserRecord.Rank, currentUserRecord.Score, avatarId, vipLevel);
        }
        else
        {
            UpdateUserLeaderboardUI("1000", "0", User.userProfile.AvatarId, User.userProfile.VipLevel);
        }

        recordList = apiLeaderboardRecordList.Records.ToList();
        recordList.Sort((record1, record2) => int.Parse(record1.Rank) - int.Parse(record2.Rank));

        foreach (var leaderboardItem in listLeaderboardItem)
        {
            Destroy(leaderboardItem.gameObject);
        }
        listLeaderboardItem.Clear();
        UpdateUIListRecord();
        
        LeaderBoardRecord leaderBoardRecord = await leaderboardPresenter.LoadInfo(currentTabGameCode);
        
        
    }
    
    #endregion

    #region UI
    
    private void UpdateUserLeaderboardUI(string rank = "1000", string score = "0", string avatarId = "", long vipLevel = 0)
    {
        int rank1 = int.Parse(rank);
        if (rank1 > 3)
        {
            topImage.gameObject.SetActive(false);
            currentUserTopText.gameObject.SetActive(true);
            currentUserTopText.text = rank1 > 999 ? "999+" : rank;
        }
        else
        {
            topImage.gameObject.SetActive(true);
            currentUserTopText.gameObject.SetActive(false);
            topImage.sprite = topSprites[rank1 - 1];
        }

        currentUserNameText.text = User.userProfile.UserName;
        currentUserChipValueText.text = score;
        currentUserAvatarImage.LoadAvatar(avatarId, vipLevel);
    }

    private void UpdateUIListGame()
    {
        foreach (Game game in gameList)
        {
            LeaderBoardTab leaderBoardTab = Instantiate(leaderBoardTabPrefab, leaderBoardTabParent).GetComponent<LeaderBoardTab>();
            leaderBoardTab.SetData(Constants.GameNameFromCode[game.Code], game.Code);
            leaderBoardTab.OnTabClicked += LeaderBoardTab_OnTabClicked;
            listLeaderboardTab.Add(leaderBoardTab);
        }
        listLeaderboardTab[0]?.OnClickTab();
    }

    private void UpdateUIListRecord()
    {
        foreach (IApiLeaderboardRecord record in recordList)
        {
            var json = JObject.Parse(record.Metadata);
            string avatarId = json["avatar_id"]?.ToString() ?? "";
            long vipLevel = json["vip_level"]?.Value<long>() ?? 0;
            Debug.Log("ttt " + avatarId + " : " + vipLevel);
            LeaderBoardItem leaderBoardItem = Instantiate(leaderBoardItemPrefab, leaderBoardItemParent).GetComponent<LeaderBoardItem>();
            leaderBoardItem.SetData(record.Rank, record.Username, record.Score, avatarId, vipLevel);
            listLeaderboardItem.Add(leaderBoardItem);
        }
    }
    #endregion

    #region Event

    private void LeaderBoardTab_OnTabClicked(object sender, OnTabClickedEventArgs e)
    {
        selectedTab = sender as LeaderBoardTab;
        if (currentTabGameCode == e.gameCode)
        {
            return;
        }
        currentTabGameCode = e.gameCode;
        foreach (var leaderboardTab in listLeaderboardTab)
        {
            if (leaderboardTab == selectedTab)
            {
                leaderboardTab.SelectTab(true);
            }
            else
            {
                leaderboardTab.SelectTab(false);
            }
        }
        _ = LoadListLeaderBoard();
    }
        
    #endregion
}
