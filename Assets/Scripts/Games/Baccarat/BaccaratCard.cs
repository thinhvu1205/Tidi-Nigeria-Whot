using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class BaccaratCard
{
    public int Suit; // 1: Spades, 2: Hearts, 3: Diamonds, 4: Clubs
    public int Rank; // 1-13 (A=1, J=11, Q=12, K=13)
    public bool IsVisible = true;
    
    public BaccaratCard(int suit, int rank)
    {
        Suit = suit;
        Rank = rank;
    }
    
    public BaccaratCard(int suit, int rank, bool isVisible)
    {
        Suit = suit;
        Rank = rank;
        IsVisible = isVisible;
    }
    
    public int GetBaccaratValue()
    {
        if (Rank >= 10) return 0; // Face cards (J, Q, K) = 0
        return Rank; // A = 1, 2-9 = their face value
    }
    
    public string GetCardName()
    {
        string rankName = Rank switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => Rank.ToString()
        };
        
        string suitName = Suit switch
        {
            1 => "Spades",
            2 => "Hearts", 
            3 => "Diamonds",
            4 => "Clubs",
            _ => "Unknown"
        };
        
        return $"{rankName} of {suitName}";
    }
    
    public string GetCardSpriteName()
    {
        string rankName = Rank switch
        {
            1 => "A",
            11 => "J",
            12 => "Q", 
            13 => "K",
            _ => Rank.ToString()
        };
        
        string suitName = Suit switch
        {
            1 => "S",
            2 => "H",
            3 => "D", 
            4 => "C",
            _ => "X"
        };
        
        return $"{rankName}{suitName}";
    }
}

public class BaccaratCardView : MonoBehaviour
{
    [Header("Card Components")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardBackImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Card Sprites")]
    [SerializeField] private Sprite[] cardSprites; // Array of card sprites
    [SerializeField] private Sprite cardBackSprite;
    
    [Header("Animation Settings")]
    [SerializeField] private float flipDuration = 0.5f;
    [SerializeField] private float dealDuration = 0.3f;
    [SerializeField] private float dealDelay = 0.1f;
    
    private BaccaratCard _cardData;
    private bool _isFlipped = false;
    private bool _isDealt = false;
    
    public BaccaratCard CardData => _cardData;
    public bool IsFlipped => _isFlipped;
    public bool IsDealt => _isDealt;
    
    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        if (cardImage == null)
            cardImage = GetComponent<Image>();
    }
    
    private void Start()
    {
        InitializeCard();
    }
    
    public void SetCardData(BaccaratCard cardData)
    {
        _cardData = cardData;
        UpdateCardDisplay();
    }
    
    public void InitializeCard()
    {
        // Set initial state
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        _isFlipped = false;
        _isDealt = false;
        
        // Hide both front and back initially
        if (cardImage != null) cardImage.gameObject.SetActive(false);
        if (cardBackImage != null) cardBackImage.gameObject.SetActive(false);
    }
    
    public void UpdateCardDisplay()
    {
        if (_cardData == null) return;
        
        if (_cardData.IsVisible && _isFlipped)
        {
            // Show card front
            if (cardImage != null)
            {
                cardImage.gameObject.SetActive(true);
                cardImage.sprite = GetCardSprite(_cardData);
            }
            if (cardBackImage != null)
                cardBackImage.gameObject.SetActive(false);
        }
        else
        {
            // Show card back
            if (cardBackImage != null)
            {
                cardBackImage.gameObject.SetActive(true);
                cardBackImage.sprite = cardBackSprite;
            }
            if (cardImage != null)
                cardImage.gameObject.SetActive(false);
        }
    }
    
    private Sprite GetCardSprite(BaccaratCard card)
    {
        if (cardSprites == null || cardSprites.Length == 0) return null;
        
        string spriteName = card.GetCardSpriteName();
        
        // Find sprite by name
        foreach (var sprite in cardSprites)
        {
            if (sprite.name == spriteName)
                return sprite;
        }
        
        // Fallback to first sprite if not found
        return cardSprites[0];
    }
    
    public void DealCard(Vector3 targetPosition, float delay = 0f)
    {
        if (_isDealt) return;
        
        _isDealt = true;
        
        // Reset position and scale
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        
        // Show card back initially
        if (cardBackImage != null)
        {
            cardBackImage.gameObject.SetActive(true);
            cardBackImage.sprite = cardBackSprite;
        }
        
        // Animate deal
        Sequence dealSequence = DOTween.Sequence();
        
        dealSequence.AppendInterval(delay);
        dealSequence.Append(transform.DOScale(Vector3.one, dealDuration).SetEase(Ease.OutBack));
        dealSequence.Join(transform.DOLocalMove(targetPosition, dealDuration).SetEase(Ease.OutBack));
        dealSequence.Join(canvasGroup.DOFade(1f, dealDuration));
    }
    
    public void FlipCard()
    {
        if (_isFlipped) return;
        
        _isFlipped = true;
        
        // Animate flip
        Sequence flipSequence = DOTween.Sequence();
        
        // First half of flip - scale down
        flipSequence.Append(transform.DOScaleX(0f, flipDuration * 0.5f).SetEase(Ease.InQuad));
        flipSequence.AppendCallback(() => {
            UpdateCardDisplay();
        });
        
        // Second half of flip - scale up
        flipSequence.Append(transform.DOScaleX(1f, flipDuration * 0.5f).SetEase(Ease.OutQuad));
    }
    
    public void ShowCard()
    {
        _isFlipped = true;
        UpdateCardDisplay();
    }
    
    public void HideCard()
    {
        _isFlipped = false;
        UpdateCardDisplay();
    }
    
    public void SetVisible(bool visible)
    {
        if (_cardData != null)
        {
            _cardData.IsVisible = visible;
            UpdateCardDisplay();
        }
    }
    
    public void ResetCard()
    {
        _isFlipped = false;
        _isDealt = false;
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        
        if (cardImage != null) cardImage.gameObject.SetActive(false);
        if (cardBackImage != null) cardBackImage.gameObject.SetActive(false);
    }
    
    public void AnimateWin()
    {
        // Animate card when it's part of winning hand
        Sequence winSequence = DOTween.Sequence();
        
        winSequence.Append(transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad));
        winSequence.Append(transform.DOScale(1f, 0.2f).SetEase(Ease.InQuad));
        winSequence.SetLoops(2);
    }
    
    public void AnimateLose()
    {
        // Animate card when it's part of losing hand
        Sequence loseSequence = DOTween.Sequence();
        
        loseSequence.Append(canvasGroup.DOFade(0.5f, 0.3f));
        loseSequence.Append(canvasGroup.DOFade(1f, 0.3f));
    }
} 