using System;
using DG.Tweening;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class EmojiInGameView : BaseView
{
    [SerializeField] private EmojiChatItem emojiChatItemPrefab;
    [SerializeField] private ScrollRect scrollViewTab1, scrollViewTab2;
    [SerializeField] private Image imageTab1, imageTab2;
    private EmojiInGamePresenter emojiInGamePresenter;
    private bool canClick = true;
    private float interactTimer = 0, interactCountdown = 2;


    private const string EMOJI_PATH = "emoticon/%id/skeleton_SkeletonData";

    protected override void Update()
    {
        interactTimer -= Time.unscaledDeltaTime;
        if (interactTimer <= 0)
        {
            canClick = true;
            interactTimer = interactCountdown;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        emojiInGamePresenter = new EmojiInGamePresenter();
        emojiInGamePresenter.Init(this);
        OnClickTab1();
        for (var i = 1; i <= 15; i++)
        {
            EmojiChatItem item = Instantiate(emojiChatItemPrefab, scrollViewTab1.content);
            item.transform.localScale = Vector3.one;
            string emojiId = i + "";
            item.button.onClick.AddListener(() =>
            {
                OnClickEmoji(emojiId);
            });
            SkeletonGraphic skeletonGraphic = item.skeletonGraphic;
            skeletonGraphic.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            string animPath = EMOJI_PATH.Replace("%id", "e" + i);
            Utility.PlayAnimationByPath(skeletonGraphic, animPath, "animation", true);
        }

        for (var i = 16; i <= 24; i++)
        {
            EmojiChatItem item = Instantiate(emojiChatItemPrefab, scrollViewTab2.content);
            item.transform.localScale = Vector3.one;
            string emojiId = i + "";
            item.button.onClick.AddListener(() =>
            {
                OnClickEmoji(emojiId);
            });
            SkeletonGraphic skeletonGraphic = item.skeletonGraphic;
            skeletonGraphic.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            string animPath = EMOJI_PATH.Replace("%id", "e" + i);
            Utility.PlayAnimationByPath(skeletonGraphic, animPath, "animation", true);
        }
    }

    public void OnClickEmoji(string emojiId)
    {
        if (!canClick) return;
        canClick = false;
        _ = emojiInGamePresenter.SendEmoji(emojiId);
        OnClickCloseButton();
    }

    public void OnClickTab1()
    {
        imageTab1.gameObject.SetActive(true);
        imageTab2.gameObject.SetActive(false);
        scrollViewTab1.gameObject.SetActive(true);
        scrollViewTab2.gameObject.SetActive(false);
    }

    public void OnClickTab2()
    {
        imageTab1.gameObject.SetActive(false);
        imageTab2.gameObject.SetActive(true);
        scrollViewTab1.gameObject.SetActive(false);
        scrollViewTab2.gameObject.SetActive(true);
    }

    public override void OnClickCloseButton()
    {
        Hide(false, null, true);
    }

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        Image background = transform.GetComponent<Image>();
        if (background != null)
        {
            background.DOKill();
            Sequence sequence = DOTween.Sequence();

            switch (effectPopupReverse)
            {
                case EFFECT_POPUP.NONE:
                    break;
                case EFFECT_POPUP.SCALE:
                    Vector3 targetScale = Vector3.zero;
                    sequence.Append(background.rectTransform.DOScale(targetScale, ANIMATION_TIME).SetEase(Ease.InBack).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_LEFT:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveX(-Screen.width, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_RIGHT:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveX(Screen.width, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_UP:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveY(Screen.height, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_DOWN:
                    Fade();
                    sequence.Append(background.rectTransform.DOLocalMoveY(-Screen.height, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
            }
            sequence.AppendCallback(() =>
            {
                onCompleteCallback?.Invoke();
            });
        }
    }
}
