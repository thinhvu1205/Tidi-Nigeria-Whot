using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Globals;
using Nakama;
using Newtonsoft.Json.Linq;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LeaderBoardTab;

public class LeaderBoardView : BaseView
{
    [SerializeField] private GameObject leaderBoardItemPrefab, leaderBoardTabPrefab;
    [SerializeField] private Transform leaderBoardItemParent, leaderBoardTabParent;

    [Header("Current User")]
    [SerializeField] private Image currentUserAvatarImage, topImage;
    [SerializeField] private TextMeshProUGUI currentUserNameText, currentUserTopText, currentUserChipValueText;
    [SerializeField] private List<Sprite> topSprites = new();

    private List<LeaderBoardItem> listLeaderboardItem = new();
    private List<LeaderBoardTab> listLeaderboardTab = new();
    private LeaderBoardTab selectedTab;
    private LeaderboardPresenter leaderboardPresenter;
    private List<Game> gameList = new();
    private List<IApiLeaderboardRecord> recordList = new();
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
        _ = LoadListGame();
        _ = LoadListLeaderBoard();
    }
    


    #region Data
    private async Task LoadListGame()
    {
        gameList.Clear();
        try
        {
            GameListResponse gameListResponse = await leaderboardPresenter.LoadGameList();
            gameList = gameListResponse.Games.ToList();
            UpdateUIListGame();
            Debug.Log("GAME LIST: " + gameListResponse.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log("err load list game : " + ex.Message);
            // throw;
        }
    }

    private async Task LoadListLeaderBoard()
    {
        Debug.Log("BAT DAU GOI GET LIST");

        IApiLeaderboardRecordList apiLeaderboardRecordList = await leaderboardPresenter.LoadList(currentTabGameCode);
        recordList = apiLeaderboardRecordList.Records.ToList();
        recordList.Sort((record1, record2) => int.Parse(record1.Rank) - int.Parse(record2.Rank));

        LeaderBoardRecord leaderBoardRecord = await leaderboardPresenter.LoadInfo(currentTabGameCode);
        recordList.Clear();
        foreach (var leaderboardItem in listLeaderboardItem)
        {
            Destroy(leaderboardItem.gameObject);
        }
        UpdateUIListRecord();
        
        selectedTab = listLeaderboardTab[0];
        selectedTab.SelectTab(true);

        topImage.gameObject.SetActive(true);
        currentUserNameText.text = "MQ";
        currentUserChipValueText.text = "9999999";
        currentUserTopText.gameObject.SetActive(false);
        topImage.sprite = topSprites[1];
    }

    #endregion

    #region UI
    private void UpdateUIListGame()
    {
        foreach (Game game in gameList)
        {
            LeaderBoardTab leaderBoardTab = Instantiate(leaderBoardTabPrefab, leaderBoardTabParent).GetComponent<LeaderBoardTab>();
            leaderBoardTab.SetData(Constants.GameNameFromCode[game.Code], game.Code);
            leaderBoardTab.OnTabClicked += LeaderBoardTab_OnTabClicked;
            listLeaderboardTab.Add(leaderBoardTab);
        }
        listLeaderboardTab[0].OnClickTab();
    }

    private void UpdateUIListRecord()
    {
        foreach (IApiLeaderboardRecord record in recordList)
        {
            LeaderBoardItem leaderBoardItem = Instantiate(leaderBoardItemPrefab, leaderBoardItemParent).GetComponent<LeaderBoardItem>();
            leaderBoardItem.SetData(record.Rank, record.Username, record.Score);
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
