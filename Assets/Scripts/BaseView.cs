using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
public class BaseView : MonoBehaviour
{
    enum EFFECT_POPUP
    {
        NONE,
        SCALE,
        MOVE_LEFT,
        MOVE_RIGHT,
        MOVE_UP,
        MOVE_DOWN
    }

    [SerializeField] private EFFECT_POPUP effectPopup = EFFECT_POPUP.NONE;
    [SerializeField] private EFFECT_POPUP effectPopupReverse = EFFECT_POPUP.NONE;
    [SerializeField] private Image popupBackground;
    private float originX = 0, originY = 0;
    private const float ANIMATION_TIME = 0.3f;

    protected virtual void Awake()
    {
        if (popupBackground != null)
        {
            originX = popupBackground.transform.localPosition.x;
            originY = popupBackground.transform.localPosition.y;
        }
    }
    protected virtual void Start()
    {
        SetStretch();
    }

    protected virtual void OnEnable()
    {
        Show();
    }

    public virtual void OnClickCloseButton()
    {
        Hide();
    }
    public void Show()
    {
        gameObject.SetActive(true);
        if (popupBackground != null)
        {
            popupBackground.gameObject.SetActive(true);
            popupBackground.DOKill();

            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() => SetStretch());

            switch (effectPopup)
            {
                case EFFECT_POPUP.NONE:
                    effectPopupReverse = EFFECT_POPUP.NONE;
                    SetStretch();
                    break;
                case EFFECT_POPUP.SCALE:
                    effectPopupReverse = EFFECT_POPUP.SCALE;
                    Vector3 initialScale = new(0.8f, 0.8f, 0);
                    Vector3 targetScale = new(1f, 1f, 0);
                    popupBackground.rectTransform.localScale = initialScale;
                    Fade();
                    sequence.Append(popupBackground.rectTransform.DOScale(targetScale, ANIMATION_TIME).SetEase(Ease.OutBack).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_LEFT:
                    effectPopupReverse = EFFECT_POPUP.MOVE_RIGHT;
                    Fade();
                    popupBackground.transform.localPosition = new Vector3(-Screen.width, originY);
                    sequence.Append(popupBackground.transform.DOLocalMoveX(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_RIGHT:
                    effectPopupReverse = EFFECT_POPUP.MOVE_LEFT;
                    Fade();
                    popupBackground.rectTransform.localPosition = new Vector3(Screen.width, originY);
                    sequence.Append(popupBackground.rectTransform.DOLocalMoveX(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_UP:
                    effectPopupReverse = EFFECT_POPUP.MOVE_DOWN;
                    Fade();
                    popupBackground.rectTransform.localPosition = new Vector3(originX, -Screen.height);
                    sequence.Append(popupBackground.rectTransform.DOLocalMoveY(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_DOWN:
                    effectPopupReverse = EFFECT_POPUP.MOVE_UP;
                    Fade();
                    popupBackground.rectTransform.localPosition = new Vector3(originX, Screen.height);
                    sequence.Append(popupBackground.rectTransform.DOLocalMoveY(originX, ANIMATION_TIME).SetEase(Ease.InSine).SetAutoKill(true));
                    break;
            }
        }
    }

    public void Hide(bool isDestroy = true, Action onCompleteCallback = null)
    {
        if (popupBackground != null)
        {
            popupBackground.DOKill();
            Sequence sequence = DOTween.Sequence();

            switch (effectPopupReverse)
            {
                case EFFECT_POPUP.NONE:
                    break;
                case EFFECT_POPUP.SCALE:
                    Vector3 targetScale = Vector3.zero;
                    sequence.Append(popupBackground.rectTransform.DOScale(targetScale, ANIMATION_TIME).SetEase(Ease.InBack).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_LEFT:
                    Fade();
                    sequence.Append(popupBackground.rectTransform.DOLocalMoveX(-Screen.width, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));
                    break;
                case EFFECT_POPUP.MOVE_RIGHT:
                    Fade();
                    sequence.Append(popupBackground.rectTransform.DOLocalMoveX(Screen.width, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_UP:
                    Fade();
                    sequence.Append(popupBackground.rectTransform.DOLocalMoveY(Screen.height, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
                case EFFECT_POPUP.MOVE_DOWN:
                    Fade();
                    sequence.Append(popupBackground.rectTransform.DOLocalMoveY(-Screen.height, ANIMATION_TIME).SetEase(Ease.OutSine).SetAutoKill(true));

                    break;
            }
            sequence.AppendCallback(() =>
            {
                onCompleteCallback?.Invoke();
                DeactiveOrDestroy(isDestroy);
            });
        }
        else
        {
            DeactiveOrDestroy(isDestroy);
        }
    }

    private void SetStretch()
    {
        var rect = GetComponent<RectTransform>();
        var anchorMin = rect.anchorMin;
        var anchorMax = rect.anchorMax;

        if (anchorMin.Equals(Vector2.zero) && anchorMax.Equals(Vector2.one))
        {
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
        }
    }

    private void Fade()
    {
        CanvasGroup canvasGroup = popupBackground.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popupBackground.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0.75f;

        canvasGroup.DOKill();
        canvasGroup.DOFade(1, ANIMATION_TIME).SetEase(Ease.InSine);
    }

    private void DeactiveOrDestroy(bool isDestroy = true, bool isSetParentNull = false)
    {
        if (isDestroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
            if (isSetParentNull)
            {
                transform.SetParent(null);
            }
        }
    }
}
