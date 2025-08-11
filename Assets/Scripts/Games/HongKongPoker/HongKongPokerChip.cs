using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HongKongPokerChip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject container;

    public void MoveToBoxBet(Vector2 targetPosition)
    {
        gameObject.GetComponent<Image>().enabled = true;
        container.gameObject.SetActive(false);
        canvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOLocalMove(targetPosition, 0.5f).SetEase(Ease.OutCubic))
            .Join(gameObject.GetComponent<Image>().DOFade(0, 0.8f).SetEase(Ease.InExpo))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

    }

    public void MoveToPot(Vector2 targetPosition, Vector2 potPosition)
    {
        gameObject.GetComponent<Image>().enabled = true;
        container.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        canvasGroup.alpha = 0f;
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOLocalMove(targetPosition, 0.8f).SetEase(Ease.InBack))
            .AppendInterval(0.6f)
            .Append(transform.DOMove(potPosition, 0.5f).SetEase(Ease.InElastic))
            // .PrependInterval(i * 0.02f)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }

    public void MoveToPlayer(Vector2 chipPosition, Vector2 playerPosition, int chipValue)
    {
        Debug.Log("MOVE TO PLAYER");
        gameObject.GetComponent<Image>().enabled = false;
        canvasGroup.alpha = 0;
        container.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(0.5f)
            .Append(transform.DOLocalMove(new Vector3(chipPosition.x, chipPosition.y - 40, 0), 0.6f).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(1.4f, 0.3f))
            .Append(transform.DOScale(1.0f, 0.3f).SetEase(Ease.InCubic))
            .AppendCallback(() =>
            {
                text.text = Utility.FormatMoney(chipValue);
                canvasGroup.DOFade(1, 0.4f);
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                text.transform.parent.gameObject.SetActive(false);
                transform.DOLocalMove(playerPosition, 1.0f).SetEase(Ease.OutCubic);
                // SoundManager.instance.playEffectFromPath(SOUND_GAME.THROW_CHIP);
            })
            .Join(transform.DOScale(0.5f, 1.0f).SetEase(Ease.OutCubic))
            .OnComplete(() => gameObject.SetActive(false));
    }
}
