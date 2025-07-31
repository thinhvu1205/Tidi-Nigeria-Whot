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
        switch (cardSuit)
        {
            case WhotCardSuit.WhotSuitCircle:
                Debug.Log("Picked Circle Suit");
                circleLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitTriangle:
                Debug.Log("Picked Triangle Suit");
                triangleLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitCross:
                Debug.Log("Picked Cross Suit");
                crossLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitStar:
                Debug.Log("Picked Star Suit");
                starLightImage.gameObject.SetActive(true);
                break;
            case WhotCardSuit.WhotSuitSquare:
                Debug.Log("Picked Square Suit");
                squareLightImage.gameObject.SetActive(true);
                break;
            default:
                Debug.Log("Picked Unspecified Suit (Whot)");
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
