using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private JArray mockArray = new();
    private JArray mockArrayGame = new();
    private LeaderBoardTab selectedTab;

    protected override void Start()
    {
        base.Start();
        MockData();
        LoadListLeaderBoard();
    }

    #region Data
    private void LoadListLeaderBoard()
    {
        foreach (var leaderboardItem in listLeaderboardItem)
        {
            Destroy(leaderboardItem.gameObject);
        }
        foreach (var leaderboardTab in listLeaderboardTab)
        {
            Destroy(leaderboardTab.gameObject);
        }
        foreach (var leaderboardData in mockArray)
        {
            string top = leaderboardData["top"].ToString();
            string name = leaderboardData["name"].ToString();
            GameObject leaderboardItemObj = Instantiate(leaderBoardItemPrefab, leaderBoardItemParent);
            LeaderBoardItem leaderboardItem = leaderboardItemObj.GetComponent<LeaderBoardItem>();
            leaderboardItem.SetData(top, name);
        }

        foreach (var leaderboardTabData in mockArrayGame)
        {
            string name = leaderboardTabData["name"].ToString();
            GameObject leaderboardTabObj = Instantiate(leaderBoardTabPrefab, leaderBoardTabParent);
            LeaderBoardTab leaderboardTab = leaderboardTabObj.GetComponent<LeaderBoardTab>();
            leaderboardTab.SetData(name);
            leaderboardTab.OnTabClicked += LeaderBoardTab_OnTabClicked;
            listLeaderboardTab.Add(leaderboardTab);
        }
        selectedTab = listLeaderboardTab[0];
        selectedTab.SelectTab(true);

        topImage.gameObject.SetActive(true);
        currentUserNameText.text = "MQ";
        currentUserChipValueText.text = "9999999";
        currentUserTopText.gameObject.SetActive(false);
        topImage.sprite = topSprites[1];
    }

    private void MockData()
    {

        for (int i = 0; i < 5; i++)
        {
            JObject leaderboardItem = new()
            {
                ["top"] = i + 1,
                ["avatar"] = "",
                ["name"] = "Minh Quan",
            };

            mockArray.Add(leaderboardItem);
        }

        for (int i = 0; i < 10; i++)
        {
            JObject leaderboardTab = new()
            {
                ["name"] = "Game " + (i + 1),
            };

            mockArrayGame.Add(leaderboardTab);
        }

        Debug.Log(mockArray.ToString());
    }
    #endregion

    #region Event

    private void LeaderBoardTab_OnTabClicked(object sender, EventArgs e)
    {
        selectedTab = sender as LeaderBoardTab;
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
    }
        
    #endregion
}
