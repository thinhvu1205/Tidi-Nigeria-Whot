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
        skeletonGraphic.TrimRenderers();
        skeletonGraphic.transform.localScale = Vector3.one;
        skeletonGraphic.transform.localPosition = Vector3.zero;
        skeletonGraphic.skeletonDataAsset = UIManager.Instance.LoadSkeletonData(GetAnimationPath(gameID));
        skeletonGraphic.Initialize(true);

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
            UIManager.Instance.HandleOpenGame();
        }
    }

    private string GetAnimationPath(string gameID)
    {
        string animationPath;
        if (isBigIcon)
        {
            animationPath = gameID switch
            {
                Constants.WHOT_GAME_ID => "whot/skeleton_SkeletonData",
                Constants.FRUIT_SLOT_GAME_ID => "fruit_big/skeleton_SkeletonData",
                Constants.JUICY_GARDEN_GAME_ID => "juicy_big/skeleton_SkeletonData",
                Constants.NOEL_GAME_ID => "noel_big/skeleton_SkeletonData",
                Constants.SIXIANG_GAME_ID => "sixiang_big/skeleton_SkeletonData",
                Constants.TARZAN_GAME_ID => "tz_big/skeleton_SkeletonData",
                Constants.INCA_GAME_ID => "inca_big/skeleton_SkeletonData",
                _ => ""
            };
        }
        else
        {
            animationPath = gameID switch
            {
                Constants.FRUIT_SLOT_GAME_ID => "fruit_small/skeleton_SkeletonData",
                Constants.JUICY_GARDEN_GAME_ID => "juicy_small/skeleton_SkeletonData",
                Constants.NOEL_GAME_ID => "noel_small/skeleton_SkeletonData",
                Constants.SIXIANG_GAME_ID => "sixiang_small/skeleton_SkeletonData",
                Constants.TARZAN_GAME_ID => "tz_small/skeleton_SkeletonData",
                Constants.INCA_GAME_ID => "inca_small/skeleton_SkeletonData",
                _ => ""
            };
        }
        return animationPath;
    }
}
