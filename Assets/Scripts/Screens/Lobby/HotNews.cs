using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using Avatar = Common.Objects.Avatar;

public class HotNews : Singleton<HotNews>
{
    [SerializeField] private TextMeshProUGUI textName, textDesc;
    [SerializeField] private Avatar avatar;
    [SerializeField] private SkeletonGraphic animationBackground;

    private RectTransform rectTransform;
    private Vector2 originPos;
    private Tween moveTween;
    private const string BACKGROUND_ANIMATION_PATH = "Lobby/HotNew/skeleton_SkeletonData";
    private const string CO_ANIMATION_NAME = "yellow";
    private const string BIG_WIN_ANIMATION_NAME = "purple";
    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        originPos = rectTransform.anchoredPosition;
    }

    public void SetInfo(HotNewsMessage hotNewsMessage)
    {
        if (hotNewsMessage.type == "co")
        {
            textDesc.text =
                "<color=#FFFFFF>exchanged </color>" +
                $"<color=#FFD700>{hotNewsMessage.co_value}</color>" +
                "<color=#FFFFFF> USD! </color>";
            Utility.PlayAnimationByPath(animationBackground, BACKGROUND_ANIMATION_PATH, CO_ANIMATION_NAME, true);
        }
        else if (hotNewsMessage.type == "big_win")
        {
            textDesc.text =
                "<color=#FFFFFF>won </color>" +
                $"<color=#FFD700>{Utility.FormatNumber(hotNewsMessage.chips_win)}</color>" +
                "<color=#FFFFFF> chips in </color>" + 
                $"<color=#FFD700>{Constants.GameNameFromCode[hotNewsMessage.game_name]}</color>";
            Utility.PlayAnimationByPath(animationBackground, BACKGROUND_ANIMATION_PATH, BIG_WIN_ANIMATION_NAME, true);
            
        }
        textName.text = hotNewsMessage.user_name;
        avatar.LoadAvatar(hotNewsMessage.avatar_id);
        avatar.SetShowBorder(false);
    }

    public void PlayShowAnimation()
    {
        // Kill tween cũ nếu đang chạy
        moveTween?.Kill();

        // Move sang trái (x - 320) trong 0.4s
        Vector2 targetPos = originPos + new Vector2(-320f, 0f);

        moveTween = rectTransform
            .DOAnchorPos(targetPos, 0.4f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // Sau 4s thì move về vị trí ban đầu
                DOVirtual.DelayedCall(4f, () =>
                {
                    rectTransform
                        .DOAnchorPos(originPos, 0.4f)
                        .SetEase(Ease.InCubic);
                });
            });
    }
}