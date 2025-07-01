using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        GetGameList().Forget();
        UpdateVisuals();
    }

    private async UniTask GetGameList()
    {
        try
        {
            GameListResponse gameListResponse = await DataSender.GetListGame();
            gameList = gameListResponse.Games.ToList();
            LoadGameList();
            Debug.Log("GAME LIST: " + gameListResponse.ToString());
        }
        catch (Exception ex)
        {

            throw;
        }
    }
    private void LoadGameList()
    {
        foreach (Game game in gameList)
        {
            GameIcon gameIcon = Instantiate(gameIconPrefab).GetComponent<GameIcon>();
            if (game.Code == Constants.WHOT_GAME_ID)
            {
                gameIcon.gameObject.transform.SetParent(bigGameIconParent);
                gameIcon.gameObject.transform.SetAsFirstSibling();
                gameIcon.SetInfo(game.Code, true);
            }
            else
            {
                gameIcon.gameObject.transform.SetParent(miniGameIconParent);
                gameIcon.SetInfo(game.Code, false);
            }

            if (new string[] {
                Constants.FRUIT_SLOT_GAME_ID,
                Constants.INCA_GAME_ID,
                Constants.JUICY_GARDEN_GAME_ID,
                Constants.NOEL_GAME_ID,
                Constants.TARZAN_GAME_ID,
                Constants.SIXIANG_GAME_ID,
            }
                .Contains(game.Code))
            {
                GameIcon slotGameIcon = Instantiate(gameIconPrefab, slotGameIconParent).GetComponent<GameIcon>();
                slotGameIcon.SetInfo(game.Code, true);
            }

        }
    }

    private void UpdateVisuals()
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
