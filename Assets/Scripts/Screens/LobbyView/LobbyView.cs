using System;
using System.Collections.Generic;
using System.Linq;
using Api;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : BaseView
{
    [SerializeField] private TextMeshProUGUI displayNameText, userIdText, accountChip;
    [SerializeField] private Image allSlotGamesImage, allGamesImage;
    [SerializeField] private Transform bigGameIconParent, miniGameIconParent, slotGameIconParent, allGamesParent, slotGamesParent;
    [SerializeField] private GameObject gameIconPrefab;
    private List<Game> gameList = new();
    
    protected override void Awake()
    {
        base.Awake();
        OnClickAllGamesTab();
        _ = LoadGames();
        UpdateProfileData();
        UIManager.Instance.lobbyView = this;
    }

    private async UniTask LoadGames()
    {
        try
        {
            GameListResponse gameListResponse = await DataSender.GetListGame();
            gameList = gameListResponse.Games.ToList();
            UpdateUIListGame();
            Debug.Log("GAME LIST: " + gameListResponse.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log("err load list game : "+ ex.Message);
            // throw;
        }
    }
    
    private void UpdateUIListGame()
    {
        foreach (Game game in gameList)
        {
            ItemGame itemGame = Instantiate(gameIconPrefab).GetComponent<ItemGame>();
            if (game.Code == Constants.WHOT_GAME_ID)
            {
                itemGame.gameObject.transform.SetParent(bigGameIconParent);
                itemGame.gameObject.transform.SetAsFirstSibling();
                itemGame.SetInfo(game.Code, true);
            }
            else
            {
                itemGame.gameObject.transform.SetParent(miniGameIconParent);
                itemGame.SetInfo(game.Code, false);
            }

            if (Constants.SLOT_GAMES_ID.Contains(game.Code))
            {
                ItemGame slotItemGame = Instantiate(gameIconPrefab, slotGameIconParent).GetComponent<ItemGame>();
                slotItemGame.SetInfo(game.Code, true);
            }

        }
    }

    public void UpdateProfileData()
    {
        if (User.userMain != null)
        {
            displayNameText.text = User.userMain.displayName;
            userIdText.text = "ID: " + User.userMain.userSid;
            accountChip.text = User.userMain.accountChip;
        }
    }
    
    #region Buttons
    public void OnClickAllGamesTab()
    {
        allGamesImage.gameObject.SetActive(true);
        allSlotGamesImage.gameObject.SetActive(false);
        allGamesParent.gameObject.SetActive(true);
        slotGamesParent.gameObject.SetActive(false);
    }

    public void OnClickAllSlotsTab()
    {
        allGamesImage.gameObject.SetActive(false);
        allSlotGamesImage.gameObject.SetActive(true);
        allGamesParent.gameObject.SetActive(false);
        slotGamesParent.gameObject.SetActive(true);
    }
    public void OnClickProfile() => UIManager.Instance.OpenProfile();
    public void OnClickLeaderboard() => UIManager.Instance.OpenLeaderboard();
    public void OnClickFreeChips() => UIManager.Instance.OpenFreeChips();
    public void OnClickShop() => UIManager.Instance.OpenShop();
    public void OnClickMail() => UIManager.Instance.OpenMail();
    public void OnClickChipOnline() => UIManager.Instance.OpenChipOnline();
    public void OnClickFriend() => UIManager.Instance.OpenFriend();
    public void OnClickSetting() => UIManager.Instance.OpenSetting();
    #endregion
}
