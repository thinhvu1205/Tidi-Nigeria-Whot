using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Yuujins.Cfg.Game.V1;
using Config = Globals.Config;

public class ItemGame : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic skeletonGraphic;
    private bool isBigIcon = false;
    private uint gameID;
    private string gameName;

    public void SetInfo(string gameName, int gameId,  bool isBigIcon)
    {
        this.isBigIcon = isBigIcon;
        this.gameName = gameName;
        gameID = (uint) gameId;
        transform.localScale = Vector2.one;
        if (isBigIcon)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 size = rectTransform.sizeDelta;
            // size.x *= 1.5f;
            rectTransform.sizeDelta = size;
        }
        string animationName = "animation";
        if (gameName == Constants.BACCARAT_GAME_ID || gameName == Constants.HK_POKER_GAME_ID)
        {
            animationName = "eng";
        }
        Utility.PlayAnimationByPath(skeletonGraphic, GetAnimationPath(gameName), animationName);

        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _ = OnClickItemGameAsync());
    }

    /// <summary>Bấm item game: gọi cfg_bet_read; nếu có list bet (PVP/bet) thì mở SelectTableView chọn mức cược, không thì quick match.</summary>
    private async UniTaskVoid OnClickItemGameAsync()
    {
        Config.currentGameName = gameName;
        Config.currentGameId = gameID;
        if (ServerConfig.GameMap.TryGetValue(gameID, out var game) && game is { Type: Game.Types.Type.Slot })
        {
            _ = UIManager.Instance.HandleFindAndJoinMatch(0);
        }
        else
        {
            UIManager.Instance.OpenSelectTableView();
            // UIManager.Instance.OpenBanner(Proto.TypeInAppMessage.Banner);
        }
    }
    
    private string GetAnimationPath(string gameID)
    {
        string animationPath;
        if (isBigIcon)
        {
            animationPath = gameID switch
            {
                Constants.WHOT_GAME_ID => "anim_iconGames/whot/skeleton_SkeletonData",
                Constants.FRUIT_SLOT_GAME_ID => "anim_iconGames/fruit_big/skeleton_SkeletonData",
                Constants.JUICY_GARDEN_GAME_ID => "anim_iconGames/juicy_big/skeleton_SkeletonData",
                Constants.NOEL_GAME_ID => "anim_iconGames/noel_big/skeleton_SkeletonData",
                Constants.SIXIANG_GAME_ID => "anim_iconGames/sixiang_big/skeleton_SkeletonData",
                Constants.TARZAN_GAME_ID => "anim_iconGames/tz_big/skeleton_SkeletonData",
                Constants.INCA_GAME_ID => "anim_iconGames/inca_big/skeleton_SkeletonData",
                _ => ""
            };
        }
        else
        {
            animationPath = gameID switch
            {
                Constants.FRUIT_SLOT_GAME_ID => "anim_iconGames/fruit_small/skeleton_SkeletonData",
                Constants.JUICY_GARDEN_GAME_ID => "anim_iconGames/juicy_small/skeleton_SkeletonData",
                Constants.NOEL_GAME_ID => "anim_iconGames/noel_small/skeleton_SkeletonData",
                Constants.SIXIANG_GAME_ID => "anim_iconGames/sixiang_small/skeleton_SkeletonData",
                Constants.TARZAN_GAME_ID => "anim_iconGames/tz_small/skeleton_SkeletonData",
                Constants.INCA_GAME_ID => "anim_iconGames/inca_small/skeleton_SkeletonData",
                Constants.BACCARAT_GAME_ID => "anim_iconGames/baccarat/skeleton_SkeletonData",
                Constants.ROULETTE_GAME_ID => "anim_iconGames/roulette/skeleton_SkeletonData",
                Constants.HK_POKER_GAME_ID => "anim_iconGames/HK_Poker/skeleton_SkeletonData",
                Constants.BLACKJACK_GAME_ID => "anim_iconGames/blackjack/skeleton_SkeletonData",
                _ => ""
            };
        }
        return animationPath;
    }
}
