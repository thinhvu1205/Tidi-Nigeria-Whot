using System;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WhotSuitPicker : MonoBehaviour
{
    public event Action<CardSuit> OnSuitPicked;

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


    public void OnPickSuit(CardSuit cardSuit)
    {
        switch (cardSuit)
        {
            case CardSuit.SuitCircle:
                Debug.Log("Picked Circle Suit");
                circleLightImage.gameObject.SetActive(true);
                break;
            case CardSuit.SuitTriangle:
                Debug.Log("Picked Triangle Suit");
                triangleLightImage.gameObject.SetActive(true);
                break;
            case CardSuit.SuitCross:
                Debug.Log("Picked Cross Suit");
                crossLightImage.gameObject.SetActive(true);
                break;
            case CardSuit.SuitStar:
                Debug.Log("Picked Star Suit");
                starLightImage.gameObject.SetActive(true);
                break;
            case CardSuit.SuitSquare:
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
        starButton.onClick.AddListener(() => OnPickSuit(CardSuit.SuitStar));
        circleButton.onClick.AddListener(() => OnPickSuit(CardSuit.SuitCircle));
        crossButton.onClick.AddListener(() => OnPickSuit(CardSuit.SuitCross));
        squareButton.onClick.AddListener(() => OnPickSuit(CardSuit.SuitSquare));
        triangleButton.onClick.AddListener(() => OnPickSuit(CardSuit.SuitTriangle));
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
