using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Google.Protobuf;
using Proto;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

public class GroupMenuView : BaseView
{
    [SerializeField] Button leaveTableButton, settingsButton, switchTableButton, musicButton, soundButton, ruleButton;
    [SerializeField] List<Sprite> toggleSprites;

    protected override void Awake()
    {
        base.Awake();
        SetupButtons();
        SetupPosition();
    }

    public void OnClickSwitchTable()
    {
        Hide();
        var gameView = UIManager.Instance.gameView;
        if (gameView == null) return;
        
        var changeTableRequest = new ChangeTableRequest
        {
            Cancel = gameView.WantSwitchTable
        };

        DataSender.SendMatchState(
            (long)OpCodeRequest.ChangeTable,
            changeTableRequest.ToByteArray()
        );
    }

    public void OnClickLeaveTable()
    {
        Hide();
        UniTask.Void(async () =>
        {
            UIManager.Instance.ShowProgressing();
            await UIManager.Instance.HandleLeaveGame();
            UIManager.Instance.HideProgressing();
        });
    }

    public void OnClickSettings()
    {
        Hide();
        UIManager.Instance.OpenSetting();
    }

    public void OnClickMusic()
    {

    }

    public void OnClickSound()
    {
        
    }

    public void OnClickRule()
    {
        Hide();
        string curGameId = Config.currentGameId;
        string urlRule = Config.currentUrlRule.Replace("%gameid%", curGameId + "");
        //var langLocal = cc.sys.localStorage.getItem("language_client");
        //var language = langLocal == LANGUAGE_TEXT_CONFIG.LANG_EN ? "en" : "thai"
        var language = "thai";
        // urlRule = urlRule.Replace("%language%", language);
        urlRule = urlRule.Replace("%language%", language);
        // https://conf.topbangkokclub.com/rule/index.html?gameid=%gameid%&language=%language%&list=true
        if (Constants.INGAME_RULES_ID.Contains(curGameId))
        {
            UIManager.Instance.OpenRule();
        }
        else
        {
            //require("Util").onCallWebView(urlRule);
            UIManager.Instance.OpenWebView(urlRule);

        }
    }

    private void SetupButtons()
    {
        musicButton.gameObject.SetActive(false);
        soundButton.gameObject.SetActive(false);
        string currentGameId = Config.currentGameId;
        if (Constants.SLOT_GAMES_ID.Contains(currentGameId) || currentGameId == Constants.ROULETTE_GAME_ID)
        {
            switchTableButton.gameObject.SetActive(false);
        }

    }

    private void SetupPosition()
    {
        RectTransform rectTransform = popupBackground.GetComponent<RectTransform>();
        Vector2 sizeDelta = rectTransform.sizeDelta;
        SetOriginPosition(-transform.parent.GetComponent<RectTransform>().rect.width * .5f + sizeDelta.x * .5f + 10f, 720.0f * .5f - sizeDelta.y * .5f - 30f);
    }

    private void SetOriginPosition(float x, float y)
    {
        originX = x;
        originY = y;
    }
}
