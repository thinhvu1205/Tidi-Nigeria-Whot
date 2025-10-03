using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ItemGame : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic skeletonGraphic;
    private bool isBigIcon = false;

    public void SetInfo(string gameID, bool isBigIcon)
    {
        this.isBigIcon = isBigIcon;
        transform.localScale = Vector2.one;
        if (isBigIcon)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 size = rectTransform.sizeDelta;
            // size.x *= 1.5f;
            rectTransform.sizeDelta = size;
        }
        string animationName = "animation";
        if (gameID == Constants.BACCARAT_GAME_ID || gameID == Constants.HK_POKER_GAME_ID)
        {
            animationName = "eng";
        }
        Utility.PlayAnimationByPath(skeletonGraphic, GetAnimationPath(gameID), animationName, true);

        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OnClickItemGame(gameID);
        });
    }

    private void OnClickItemGame(string gameID)
    {
        Config.currentGameId = gameID;
        if (User.userProfile.VipLevel == 0)
        {
            _ = UIManager.Instance.HandleQuickMatch();
            return;
        }
        if (Constants.SELECT_TABLE_GAMES_ID.Contains(gameID))
        {
            UIManager.Instance.OpenSelectTableView();
            UIManager.Instance.OpenBanner(Proto.TypeInAppMessage.Banner);
        }
        else
        {
            _ = UIManager.Instance.HandleFindAndJoinMatch(0);
            // UIManager.Instance.HandleOpenGame();
        }
        // UIManager.getInstance().setBannerType(Constants.BANNER_SHOW_TYPE.CHOOSE_GAME, true);
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
