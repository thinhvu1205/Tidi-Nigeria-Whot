using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class GameIcon : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic skeletonGraphic;
    private bool isBigIcon = false;

    public void SetInfo(string gameID, bool isBigIcon)
    {
        this.isBigIcon = isBigIcon;
        if (isBigIcon)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 size = rectTransform.sizeDelta;
            size.x = 315f;
            rectTransform.sizeDelta = size;
        }
        Utility.PlayAnimationByPath(skeletonGraphic, GetAnimationPath(gameID));

        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OnClick(gameID);
        });
    }

    private void OnClick(string gameID)
    {
        Config.currentGameId = gameID;
        if (Constants.SELECT_TABLE_GAMES_ID.Contains(gameID))
        {
            UIManager.Instance.OpenSelectTableView();
        }
        else
        {
            _ = UIManager.Instance.HandleFindMatch(0);
                        // UIManager.Instance.HandleOpenGame();

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
                _ => ""
            };
        }
        return animationPath;
    }
}
