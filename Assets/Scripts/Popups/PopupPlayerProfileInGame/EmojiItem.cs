using System.Collections;
using DG.Tweening;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class EmojiItem : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private SkeletonGraphic skeletonGraphic;
    [SerializeField] private Sprite[] listSprite; // 0: bomb, 1: beer, 2: tomato, 3: kiss, 4: water, 5: rose
    [SerializeField] private string[] listAnimationName = new string[]
    {
        "bomb",
        "beer",
        "tomato",
        "kiss",
        "water",
        "rose",
    };
    private const string EMOJI_PATH = "emoticon/%id/skeleton_SkeletonData";
    private const string CHAT_ACTION_PATH = "chat_action/%name/skeleton_SkeletonData";

    public void ShowEmote(int emojiId)
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                image.gameObject.SetActive(false);
                if (emojiId >= 1 && emojiId <= 16)
                {
                    skeletonGraphic.transform.localScale = Vector3.one * 0.5f;
                }
                else if (emojiId >= 17 && emojiId <= 24)
                {
                    skeletonGraphic.transform.localScale = Vector3.one * 0.35f;
                }
                string animPath = EMOJI_PATH.Replace("%id", "e" + emojiId);
                Utility.PlayAnimationByPath(skeletonGraphic, animPath, "animation", true);
            })
            .AppendInterval(3f)
            .AppendCallback(() => Destroy(gameObject));
    }

    public IEnumerator SendEmojiTo(int emojiId, Transform target)
    {
        if (listSprite[emojiId] != null)
        {
            image.gameObject.SetActive(true);
            image.sprite = listSprite[emojiId];
            // image.SetNativeSize();
        }
        yield return new WaitForSeconds(0.1f);
        transform.DOMove(target.position, 1).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            image.gameObject.SetActive(false);
            string animPath = CHAT_ACTION_PATH.Replace("%name", listAnimationName[emojiId]);
            Utility.PlayAnimationByPath(skeletonGraphic, animPath, "animation", true);
            // skeletonGraphic.gameObject.SetActive(true);
            // skeletonGraphic.skeletonDataAsset = listAnimation[emojiId];
            // skeletonGraphic.Initialize(true);
            // skeletonGraphic.startingAnimation = "animation";
            // skeletonGraphic.startingLoop = false;
            // skeletonGraphic.transform.localScale = Vector3.one;
            skeletonGraphic.AnimationState.Complete += delegate
            {
                Destroy(gameObject);
            };
            string sound = "";
            switch (emojiId)
            {
                case 0:
                    sound = SoundEmoji.BOOM;
                    break;
                case 1:
                    sound = SoundEmoji.BEER;
                    break;
                case 2:
                    sound = SoundEmoji.TOMATO;
                    break;
                case 3:
                    sound = SoundEmoji.KISS;
                    break;
                case 4:
                    sound = SoundEmoji.WATER;
                    break;
                case 5:
                    sound = SoundEmoji.ROSE;
                    break;
            }
            SoundManager.Instance.PlayEffectFromPath(sound);
        });
    }
}
