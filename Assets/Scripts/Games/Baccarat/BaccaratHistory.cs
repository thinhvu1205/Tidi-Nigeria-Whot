using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BaccaratHistory : MonoBehaviour
{
    [Header("History Components")]
    [SerializeField] private Transform historyContent;
    [SerializeField] private GameObject historyItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Big Road Components")]
    [SerializeField] private Transform bigRoadContent;
    [SerializeField] private GameObject bigRoadItemPrefab;
    [SerializeField] private int maxBigRoadItems = 50;
    
    [Header("Statistics")]
    [SerializeField] private TextMeshProUGUI bankerWinsText;
    [SerializeField] private TextMeshProUGUI playerWinsText;
    [SerializeField] private TextMeshProUGUI tieWinsText;
    [SerializeField] private TextMeshProUGUI playerPairText;
    [SerializeField] private TextMeshProUGUI bankerPairText;
    
    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.3f;
    [SerializeField] private float hideDuration = 0.2f;
    
    private List<BaccaratResult> _gameHistory = new List<BaccaratResult>();
    private List<GameObject> _historyItems = new List<GameObject>();
    private List<GameObject> _bigRoadItems = new List<GameObject>();
    
    // Big Road tracking variables
    private int _bankerWins = 0;
    private int _playerWins = 0;
    private int _tieWins = 0;
    private int _playerPairWins = 0;
    private int _bankerPairWins = 0;
    
    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }
    
    private void Start()
    {
        InitializeHistory();
    }
    
    public void InitializeHistory()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }
        
        // Set initial state
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        
        ClearHistory();
    }
    
    public void AddGameResult(BaccaratResult result)
    {
        _gameHistory.Add(result);
        
        // Update statistics
        UpdateStatistics(result);
        
        // Add to history display
        AddHistoryItem(result);
        
        // Update Big Road
        UpdateBigRoad(result);
        
        // Limit history items
        if (_historyItems.Count > maxBigRoadItems)
        {
            RemoveOldestHistoryItem();
        }
    }
    
    private void UpdateStatistics(BaccaratResult result)
    {
        switch (result.WinType)
        {
            case BaccaratWinType.Banker:
                _bankerWins++;
                break;
            case BaccaratWinType.Player:
                _playerWins++;
                break;
            case BaccaratWinType.Tie:
                _tieWins++;
                break;
            case BaccaratWinType.PlayerPair:
                _playerPairWins++;
                break;
            case BaccaratWinType.BankerPair:
                _bankerPairWins++;
                break;
        }
        
        UpdateStatisticsDisplay();
    }
    
    private void UpdateStatisticsDisplay()
    {
        if (bankerWinsText != null)
            bankerWinsText.text = _bankerWins.ToString();
            
        if (playerWinsText != null)
            playerWinsText.text = _playerWins.ToString();
            
        if (tieWinsText != null)
            tieWinsText.text = _tieWins.ToString();
            
        if (playerPairText != null)
            playerPairText.text = _playerPairWins.ToString();
            
        if (bankerPairText != null)
            bankerPairText.text = _bankerPairWins.ToString();
    }
    
    private void AddHistoryItem(BaccaratResult result)
    {
        if (historyItemPrefab == null || historyContent == null) return;
        
        GameObject historyItem = Instantiate(historyItemPrefab, historyContent);
        _historyItems.Add(historyItem);
        
        // Set history item data
        BaccaratHistoryItem historyItemComponent = historyItem.GetComponent<BaccaratHistoryItem>();
        if (historyItemComponent != null)
        {
            historyItemComponent.SetResult(result);
        }
        
        // Animate new item
        historyItem.transform.localScale = Vector3.zero;
        historyItem.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
    
    private void RemoveOldestHistoryItem()
    {
        if (_historyItems.Count > 0)
        {
            GameObject oldestItem = _historyItems[0];
            _historyItems.RemoveAt(0);
            
            // Animate removal
            oldestItem.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    Destroy(oldestItem);
                });
        }
    }
    
    private void UpdateBigRoad(BaccaratResult result)
    {
        if (bigRoadItemPrefab == null || bigRoadContent == null) return;
        
        GameObject bigRoadItem = Instantiate(bigRoadItemPrefab, bigRoadContent);
        _bigRoadItems.Add(bigRoadItem);
        
        // Set Big Road item data
        BaccaratBigRoadItem bigRoadItemComponent = bigRoadItem.GetComponent<BaccaratBigRoadItem>();
        if (bigRoadItemComponent != null)
        {
            bigRoadItemComponent.SetResult(result);
        }
        
        // Animate new item
        bigRoadItem.transform.localScale = Vector3.zero;
        bigRoadItem.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        // Limit Big Road items
        if (_bigRoadItems.Count > maxBigRoadItems)
        {
            RemoveOldestBigRoadItem();
        }
    }
    
    private void RemoveOldestBigRoadItem()
    {
        if (_bigRoadItems.Count > 0)
        {
            GameObject oldestItem = _bigRoadItems[0];
            _bigRoadItems.RemoveAt(0);
            
            // Animate removal
            oldestItem.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    Destroy(oldestItem);
                });
        }
    }
    
    public void ShowHistory()
    {
        gameObject.SetActive(true);
        
        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad));
        showSequence.Join(transform.DOScale(1f, showDuration).SetEase(Ease.OutBack));
    }
    
    public void HideHistory()
    {
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.Append(canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.InQuad));
        hideSequence.Join(transform.DOScale(0.8f, hideDuration).SetEase(Ease.InBack));
        hideSequence.OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
    
    public void ClearHistory()
    {
        // Clear history items
        foreach (var item in _historyItems)
        {
            if (item != null)
                Destroy(item);
        }
        _historyItems.Clear();
        
        // Clear Big Road items
        foreach (var item in _bigRoadItems)
        {
            if (item != null)
                Destroy(item);
        }
        _bigRoadItems.Clear();
        
        // Reset statistics
        _bankerWins = 0;
        _playerWins = 0;
        _tieWins = 0;
        _playerPairWins = 0;
        _bankerPairWins = 0;
        
        UpdateStatisticsDisplay();
    }
    
    public void SetHistoryData(List<BaccaratResult> history)
    {
        ClearHistory();
        _gameHistory = new List<BaccaratResult>(history);
        
        foreach (var result in _gameHistory)
        {
            UpdateStatistics(result);
            AddHistoryItem(result);
            UpdateBigRoad(result);
        }
    }
    
    private void OnCloseButtonClick()
    {
        HideHistory();
    }
    
    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }
    }
}

// History item component
public class BaccaratHistoryItem : MonoBehaviour
{
    [Header("History Item Components")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI bankerScoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Result Icons")]
    [SerializeField] private Sprite bankerWinSprite;
    [SerializeField] private Sprite playerWinSprite;
    [SerializeField] private Sprite tieSprite;
    [SerializeField] private Sprite playerPairSprite;
    [SerializeField] private Sprite bankerPairSprite;
    
    private BaccaratResult _result;
    
    public void SetResult(BaccaratResult result)
    {
        _result = result;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (_result == null) return;
        
        // Set result icon
        if (resultIcon != null)
        {
            resultIcon.sprite = GetResultSprite(_result.WinType);
        }
        
        // Set scores
        if (playerScoreText != null)
            playerScoreText.text = _result.PlayerScore.ToString();
            
        if (bankerScoreText != null)
            bankerScoreText.text = _result.BankerScore.ToString();
            
        // Set time
        if (timeText != null)
            timeText.text = _result.Timestamp.ToString("HH:mm");
    }
    
    private Sprite GetResultSprite(BaccaratWinType winType)
    {
        return winType switch
        {
            BaccaratWinType.Banker => bankerWinSprite,
            BaccaratWinType.Player => playerWinSprite,
            BaccaratWinType.Tie => tieSprite,
            BaccaratWinType.PlayerPair => playerPairSprite,
            BaccaratWinType.BankerPair => bankerPairSprite,
            _ => tieSprite
        };
    }
}

// Big Road item component
public class BaccaratBigRoadItem : MonoBehaviour
{
    [Header("Big Road Item Components")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Result Icons")]
    [SerializeField] private Sprite bankerWinSprite;
    [SerializeField] private Sprite playerWinSprite;
    [SerializeField] private Sprite tieSprite;
    
    private BaccaratResult _result;
    
    public void SetResult(BaccaratResult result)
    {
        _result = result;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (_result == null) return;
        
        // Set result icon
        if (resultIcon != null)
        {
            resultIcon.sprite = GetResultSprite(_result.WinType);
        }
        
        // Set color based on result
        if (canvasGroup != null)
        {
            Color resultColor = GetResultColor(_result.WinType);
            canvasGroup.alpha = 0.8f;
            // You can also set the color of the icon or background
        }
    }
    
    private Sprite GetResultSprite(BaccaratWinType winType)
    {
        return winType switch
        {
            BaccaratWinType.Banker => bankerWinSprite,
            BaccaratWinType.Player => playerWinSprite,
            BaccaratWinType.Tie => tieSprite,
            _ => tieSprite
        };
    }
    
    private Color GetResultColor(BaccaratWinType winType)
    {
        return winType switch
        {
            BaccaratWinType.Banker => Color.red,
            BaccaratWinType.Player => Color.blue,
            BaccaratWinType.Tie => Color.green,
            _ => Color.white
        };
    }
}
