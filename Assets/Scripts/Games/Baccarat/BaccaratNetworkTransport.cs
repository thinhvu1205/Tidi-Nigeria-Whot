using System;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Proto;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

public class BaccaratNetworkTransport : MonoBehaviour
{
    public static BaccaratNetworkTransport Instance { get; private set; }
    
    [Header("Network Settings")]
    public string GameCode = "BACCARAT";
    
    private NetworkManager _networkManager;
    private BaccaratGameManager _gameManager;
    
    // Network event handlers
    public event Action<BaccaratTableData> OnTableUpdated;
    public event Action<BaccaratDealData> OnDealUpdated;
    public event Action<BaccaratGameState> OnGameStateChanged;
    public event Action<BaccaratResult> OnGameFinished;
    public event Action<BaccaratWalletData> OnWalletUpdated;
    // public event Action<List<BaccaratPlayer>> OnPlayersListUpdated;
    
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
        _networkManager = NetworkManager.INSTANCE;
        _gameManager = BaccaratGameManager.Instance;
        
        if (_networkManager != null)
        {
            SubscribeToNetworkEvents();
        }
    }
    
    private void SubscribeToNetworkEvents()
    {
        // Subscribe to match events
        // _networkManager.OnMatchStateReceived += HandleMatchState;
        // _networkManager.OnMatchPresenceReceived += HandleMatchPresence;
        // _networkManager.OnMatchJoined += HandleMatchJoined;
        // _networkManager.OnMatchLeft += HandleMatchLeft;
    }
    
    private void OnDestroy()
    {
        if (_networkManager != null)
        {
            // _networkManager.OnMatchStateReceived -= HandleMatchState;
            // _networkManager.OnMatchPresenceReceived -= HandleMatchPresence;
            // _networkManager.OnMatchJoined -= HandleMatchJoined;
            // _networkManager.OnMatchLeft -= HandleMatchLeft;
        }
    }
    
    public async UniTask JoinBaccaratMatch(string matchId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(matchId))
            {
                // Create new match
                // await _networkManager.CreateMatch(GameCode);
            }
            else
            {
                // Join existing match
                await _networkManager.JoinMatch(matchId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join Baccarat match: {e.Message}");
        }
    }
    
    public void LeaveBaccaratMatch()
    {
        _networkManager.LeaveMatch();
    }
    
    // public void SendBet(BaccaratBetData betData)
    // {
    //     try
    //     {
    //         var betRequest = new BaccaratBetRequest
    //         {
    //             UserId = User.Instance?.UserId ?? "",
    //             Bets = betData.Bets
    //         };
    //         
    //         string jsonData = JsonConvert.SerializeObject(betRequest);
    //         byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
    //         
    //         _networkManager.SendMatchState(BaccaratOpCodes.OPCODE_BET, data);
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"Failed to send bet: {e.Message}");
    //     }
    // }
    
    public void SendRebet()
    {
        try
        {
            var rebetRequest = new BaccaratRebetRequest
            {
                // UserId = User.Instance?.UserId ?? ""
            };
            
            string jsonData = JsonConvert.SerializeObject(rebetRequest);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            _networkManager.SendMatchState(BaccaratOpCodes.OPCODE_REBET, data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send rebet: {e.Message}");
        }
    }
    
    public void SendDoubleBet()
    {
        try
        {
            var doubleRequest = new BaccaratDoubleRequest
            {
                // UserId = User.Instance?.UserId ?? ""
            };
            
            string jsonData = JsonConvert.SerializeObject(doubleRequest);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            _networkManager.SendMatchState(BaccaratOpCodes.OPCODE_DOUBLE, data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send double bet: {e.Message}");
        }
    }
    
    public void RequestPlayersList()
    {
        try
        {
            var request = new BaccaratPlayersRequest();
            string jsonData = JsonConvert.SerializeObject(request);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            _networkManager.SendMatchState(BaccaratOpCodes.OPCODE_LIST_PLAYERS, data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to request players list: {e.Message}");
        }
    }
    
    private void HandleMatchState(IMatchState matchState)
    {
        try
        {
            string jsonData = System.Text.Encoding.UTF8.GetString(matchState.State);
            Debug.Log($"Received match state: {matchState.OpCode} - {jsonData}");
            
            switch (matchState.OpCode)
            {
                case BaccaratOpCodes.OPCODE_UPDATE_TABLE:
                    HandleUpdateTable(jsonData);
                    break;
                case BaccaratOpCodes.OPCODE_UPDATE_DEAL:
                    HandleUpdateDeal(jsonData);
                    break;
                case BaccaratOpCodes.OPCODE_UPDATE_GAME_STATE:
                    HandleUpdateGameState(jsonData);
                    break;
                case BaccaratOpCodes.OPCODE_UPDATE_FINISH:
                    HandleUpdateFinish(jsonData);
                    break;
                case BaccaratOpCodes.OPCODE_UPDATE_WALLET:
                    HandleUpdateWallet(jsonData);
                    break;
                case BaccaratOpCodes.OPCODE_LIST_PLAYERS:
                    HandleUpdatePlayersList(jsonData);
                    break;
                case BaccaratOpCodes.OPCODE_KICK_OFF_TABLE:
                    HandleKickOffTable(jsonData);
                    break;
                default:
                    Debug.LogWarning($"Unknown opcode: {matchState.OpCode}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling match state: {e.Message}");
        }
    }
    
    private void HandleMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        // Debug.Log($"Match presence event: {presenceEvent.Joins.Count} joins, {presenceEvent.Leaves.Count} leaves");
        
        foreach (var user in presenceEvent.Joins)
        {
            Debug.Log($"Player joined: {user.UserId}");
            // Handle player joined
        }
        
        foreach (var user in presenceEvent.Leaves)
        {
            Debug.Log($"Player left: {user.UserId}");
            // Handle player left
        }
    }
    
    private void HandleMatchJoined(IMatch match)
    {
        Debug.Log($"Joined Baccarat match: {match.Id}");
        // Initialize game state
    }
    
    private void HandleMatchLeft()
    {
        Debug.Log("Left Baccarat match");
        // Clean up game state
    }
    
    private void HandleUpdateTable(string jsonData)
    {
        try
        {
            var tableData = JsonConvert.DeserializeObject<BaccaratTableData>(jsonData);
            OnTableUpdated?.Invoke(tableData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing table data: {e.Message}");
        }
    }
    
    private void HandleUpdateDeal(string jsonData)
    {
        try
        {
            var dealData = JsonConvert.DeserializeObject<BaccaratDealData>(jsonData);
            OnDealUpdated?.Invoke(dealData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing deal data: {e.Message}");
        }
    }
    
    private void HandleUpdateGameState(string jsonData)
    {
        try
        {
            var gameStateData = JsonConvert.DeserializeObject<BaccaratGameStateData>(jsonData);
            
            switch (gameStateData.State)
            {
                case "PREPARE":
                    _gameManager.SetGameState(BaccaratGameState.Prepare);
                    break;
                case "RUN":
                    _gameManager.SetGameState(BaccaratGameState.Running);
                    if (gameStateData.CountDown.HasValue)
                    {
                        _gameManager.SetCountDown(gameStateData.CountDown.Value);
                    }
                    break;
                case "REWARD":
                    _gameManager.SetGameState(BaccaratGameState.Reward);
                    break;
                case "FINISH":
                    _gameManager.SetGameState(BaccaratGameState.Finished);
                    break;
            }
            
            OnGameStateChanged?.Invoke(_gameManager.CurrentGameState);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing game state data: {e.Message}");
        }
    }
    
    private void HandleUpdateFinish(string jsonData)
    {
        try
        {
            var resultData = JsonConvert.DeserializeObject<BaccaratResultData>(jsonData);
            var result = new BaccaratResult
            {
                WinType = resultData.WinType,
                PlayerScore = resultData.PlayerScore,
                BankerScore = resultData.BankerScore,
                PlayerCards = resultData.PlayerCards,
                BankerCards = resultData.BankerCards,
                Timestamp = DateTime.Now
            };
            
            _gameManager.SetGameResult(result);
            OnGameFinished?.Invoke(result);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing finish data: {e.Message}");
        }
    }
    
    private void HandleUpdateWallet(string jsonData)
    {
        try
        {
            var walletData = JsonConvert.DeserializeObject<BaccaratWalletData>(jsonData);
            OnWalletUpdated?.Invoke(walletData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing wallet data: {e.Message}");
        }
    }
    
    private void HandleUpdatePlayersList(string jsonData)
    {
        try
        {
            var playersData = JsonConvert.DeserializeObject<BaccaratPlayersData>(jsonData);
            // OnPlayersListUpdated?.Invoke(playersData.Players);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing players list data: {e.Message}");
        }
    }
    
    private void HandleKickOffTable(string jsonData)
    {
        Debug.Log("Kicked off the table");
        // Handle kick off logic
    }
}

// Data classes for network communication
[System.Serializable]
public class BaccaratBetRequest
{
    public string UserId;
    public List<BaccaratBet> Bets;
}

[System.Serializable]
public class BaccaratBet
{
    public int Cell;
    public int Amount;
}

[System.Serializable]
public class BaccaratRebetRequest
{
    public string UserId;
}

[System.Serializable]
public class BaccaratDoubleRequest
{
    public string UserId;
}

[System.Serializable]
public class BaccaratPlayersRequest
{
    // Empty for now
}

[System.Serializable]
public class BaccaratTableData
{
    public Dictionary<int, int> Bets;
    public float CountDown;
}

[System.Serializable]
public class BaccaratDealData
{
    public List<BaccaratCard> PlayerCards;
    public List<BaccaratCard> BankerCards;
    public int PlayerScore;
    public int BankerScore;
}

[System.Serializable]
public class BaccaratGameStateData
{
    public string State;
    public float? CountDown;
}

[System.Serializable]
public class BaccaratResultData
{
    public BaccaratWinType WinType;
    public int PlayerScore;
    public int BankerScore;
    public List<BaccaratCard> PlayerCards;
    public List<BaccaratCard> BankerCards;
}

[System.Serializable]
public class BaccaratWalletData
{
    public string UserId;
    public long Balance;
}

[System.Serializable]
public class BaccaratPlayersData
{
    // public List<BaccaratPlayer> Players;
}

public static class BaccaratOpCodes
{
    public const long OPCODE_BET = 1001;
    public const long OPCODE_REBET = 1002;
    public const long OPCODE_DOUBLE = 1003;
    public const long OPCODE_LIST_PLAYERS = 1004;
    public const long OPCODE_UPDATE_TABLE = 2001;
    public const long OPCODE_UPDATE_DEAL = 2002;
    public const long OPCODE_UPDATE_GAME_STATE = 2003;
    public const long OPCODE_UPDATE_FINISH = 2004;
    public const long OPCODE_UPDATE_WALLET = 2005;
    public const long OPCODE_KICK_OFF_TABLE = 2006;
} 