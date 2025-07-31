using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Proto;
using Cysharp.Threading.Tasks;

public class BaccaratGameManager : MonoBehaviour
{
    public static BaccaratGameManager Instance { get; private set; }
    
    [Header("Game State")]
    public BaccaratGameState CurrentGameState = BaccaratGameState.None;
    public float CountDownTime = 0f;
    public bool IsGameRunning = false;
    
    [Header("Game Data")]
    public List<int> BetOptions = new List<int> { 1000, 2000, 5000, 10000 };
    public int SelectedBetAmount = 1000;
    public Dictionary<int, int> CurrentBets = new Dictionary<int, int>(); // cell -> amount
    public Dictionary<int, int> LastBets = new Dictionary<int, int>();
    
    [Header("Game Results")]
    public BaccaratResult LastResult;
    public List<BaccaratResult> GameHistory = new List<BaccaratResult>();
    
    [Header("Player Data")]
    public BaccaratPlayer CurrentPlayer;
    public List<BaccaratPlayer> Players = new List<BaccaratPlayer>();
    
    [Header("Cards")]
    public List<BaccaratCard> PlayerCards = new List<BaccaratCard>();
    public List<BaccaratCard> BankerCards = new List<BaccaratCard>();
    
    [Header("UI References")]
    public BaccaratView GameView;
    
    // Events
    public event Action<BaccaratGameState> OnGameStateChanged;
    public event Action<float> OnCountDownChanged;
    public event Action<BaccaratResult> OnGameResult;
    public event Action<Dictionary<int, int>> OnBetsUpdated;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    public void InitializeGame()
    {
        CurrentGameState = BaccaratGameState.None;
        CountDownTime = 0f;
        IsGameRunning = false;
        CurrentBets.Clear();
        LastBets.Clear();
        GameHistory.Clear();
        PlayerCards.Clear();
        BankerCards.Clear();
        
        // Initialize bet options
        if (BetOptions.Count > 0)
        {
            SelectedBetAmount = BetOptions[0];
        }
    }
    
    public void SetGameState(BaccaratGameState newState)
    {
        if (CurrentGameState != newState)
        {
            CurrentGameState = newState;
            OnGameStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case BaccaratGameState.Prepare:
                    HandlePrepareState();
                    break;
                case BaccaratGameState.Running:
                    HandleRunningState();
                    break;
                case BaccaratGameState.Reward:
                    HandleRewardState();
                    break;
                case BaccaratGameState.Finished:
                    HandleFinishedState();
                    break;
            }
        }
    }
    
    public void SetCountDown(float time)
    {
        CountDownTime = time;
        OnCountDownChanged?.Invoke(time);
    }
    
    public void SelectBetAmount(int amount)
    {
        if (BetOptions.Contains(amount))
        {
            SelectedBetAmount = amount;
        }
    }
    
    public void PlaceBet(int cell, int amount)
    {
        if (CurrentGameState != BaccaratGameState.Running) return;
        
        if (CurrentBets.ContainsKey(cell))
        {
            CurrentBets[cell] += amount;
        }
        else
        {
            CurrentBets[cell] = amount;
        }
        
        OnBetsUpdated?.Invoke(CurrentBets);
    }
    
    public void Rebet()
    {
        if (CurrentGameState != BaccaratGameState.Running) return;
        
        foreach (var bet in LastBets)
        {
            PlaceBet(bet.Key, bet.Value);
        }
    }
    
    public void DoubleBet()
    {
        if (CurrentGameState != BaccaratGameState.Running) return;
        
        var newBets = new Dictionary<int, int>();
        foreach (var bet in CurrentBets)
        {
            newBets[bet.Key] = bet.Value * 2;
        }
        
        CurrentBets = newBets;
        OnBetsUpdated?.Invoke(CurrentBets);
    }
    
    public void ClearBets()
    {
        CurrentBets.Clear();
        OnBetsUpdated?.Invoke(CurrentBets);
    }
    
    public void SaveLastBets()
    {
        LastBets.Clear();
        foreach (var bet in CurrentBets)
        {
            LastBets[bet.Key] = bet.Value;
        }
    }
    
    public void SetGameResult(BaccaratResult result)
    {
        LastResult = result;
        GameHistory.Add(result);
        OnGameResult?.Invoke(result);
    }
    
    public void AddPlayer(BaccaratPlayer player)
    {
        if (!Players.Exists(p => p.UserId == player.UserId))
        {
            Players.Add(player);
        }
    }
    
    public void RemovePlayer(string userId)
    {
        Players.RemoveAll(p => p.UserId == userId);
    }
    
    public BaccaratPlayer GetPlayer(string userId)
    {
        return Players.Find(p => p.UserId == userId);
    }
    
    private void HandlePrepareState()
    {
        IsGameRunning = false;
        ClearBets();
        // Additional prepare logic
    }
    
    private void HandleRunningState()
    {
        IsGameRunning = true;
        // Additional running logic
    }
    
    private void HandleRewardState()
    {
        IsGameRunning = false;
        SaveLastBets();
        // Additional reward logic
    }
    
    private void HandleFinishedState()
    {
        IsGameRunning = false;
        // Additional finished logic
    }
}

public enum BaccaratGameState
{
    None,
    Prepare,
    Running,
    Reward,
    Finished
}

[System.Serializable]
public class BaccaratResult
{
    public BaccaratWinType WinType;
    public int PlayerScore;
    public int BankerScore;
    public List<BaccaratCard> PlayerCards;
    public List<BaccaratCard> BankerCards;
    public DateTime Timestamp;
}

public enum BaccaratWinType
{
    None,
    Player,
    Banker,
    Tie,
    PlayerPair,
    BankerPair
} 