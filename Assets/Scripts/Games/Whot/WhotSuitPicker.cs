using System;
using System.Collections.Generic;
using Proto;
using DG.Tweening;
using Proto;
using UnityEngine;
using UnityEngine.UI;

public class WhotSuitPicker : MonoBehaviour
{
    public event Action<WhotCardSuit> OnSuitPicked;

    [SerializeField] private Button starButton, circleButton, crossButton, squareButton, triangleButton;
    [SerializeField] private Image starLightImage, circleLightImage, crossLightImage, squareLightImage, triangleLightImage;
    private const float SHOW_ANIMATION_TIME = 0.5f;
    private const float HIDE_ANIMATION_TIME = 0.3f;
    private void Awake()
    {
        AssignButtonListeners();
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, SHOW_ANIMATION_TIME).SetEase(Ease.OutBack);
    }


    public void OnPickSuit(WhotCardSuit cardSuit)
    {
        DataSender.SendMatchState((long)OpCodeRequest.OpcodeUserInteractCards, new byte[0]);
        switch (cardSuit)
        {
            case WhotCardSuit.WhotSuitCircle:
                circleLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitTriangle:
                triangleLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitCross:
                crossLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitStar:
                starLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitSquare:
                squareLightImage.gameObject.SetActive(true);
                break;
            default:
                break;
        }
        transform.DOScale(Vector3.zero, HIDE_ANIMATION_TIME).SetEase(Ease.InBack).OnComplete(() =>
        {
            OnSuitPicked?.Invoke(cardSuit);
            gameObject.SetActive(false);
        });

    }

    private void AssignButtonListeners()
    {
        starButton.onClick.AddListener(() => OnPickSuit(WhotCardSuit.WhotSuitStar));
        circleButton.onClick.AddListener(() => OnPickSuit(WhotCardSuit.WhotSuitCircle));
        crossButton.onClick.AddListener(() => OnPickSuit(WhotCardSuit.WhotSuitCross));
        squareButton.onClick.AddListener(() => OnPickSuit(WhotCardSuit.WhotSuitSquare));
        triangleButton.onClick.AddListener(() => OnPickSuit(WhotCardSuit.WhotSuitTriangle));
    }

    private void OnDisable()
    {
        starLightImage.gameObject.SetActive(false);
        circleLightImage.gameObject.SetActive(false);
        crossLightImage.gameObject.SetActive(false);
        squareLightImage.gameObject.SetActive(false);
        triangleLightImage.gameObject.SetActive(false);
    }
}
